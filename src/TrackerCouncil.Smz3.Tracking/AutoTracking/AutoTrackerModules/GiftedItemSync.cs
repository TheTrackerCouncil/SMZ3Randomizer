using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class GiftedItemSync(
    TrackerBase tracker,
    ISnesConnectorService snesConnector,
    ILogger<GiftedItemSync> logger) : AutoTrackerModule(tracker, snesConnector, logger)
{
    private int _itemCountAddress = 0xA26D38;
    private int _itemDetailLength = 2;

    public override void Initialize()
    {
        if (Tracker.World.Config.RomGenerator == RomGenerator.Cas)
        {
            return;
        }

        if (Tracker.World.Config.RomGenerator == RomGenerator.Mainline)
        {
            _itemCountAddress = 0xA26602;
            _itemDetailLength = 4;
        }

        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = _itemCountAddress,
            Length = 2,
            FrequencySeconds = 2,
            OnResponse = OnMemorySync,
            Filter = () => IsInGame
        });
    }

    private async void OnMemorySync(SnesData firstDataSet, SnesData? firstPrevData)
    {
        var newItemCount = firstDataSet.ReadUInt16(0);
        var trackerState = Tracker.World.State;
        if (newItemCount == null || trackerState == null || newItemCount > 10000)
        {
            return;
        }

        var currentGiftedItemCount = trackerState.GiftedItemCount;
        var itemCountDifference = newItemCount.Value - currentGiftedItemCount;
        var startingAddress = 0xA26000 + currentGiftedItemCount * _itemDetailLength;
        var bytes = itemCountDifference * _itemDetailLength;
        if (bytes <= 0)
        {
            return;
        }

        var firstBlockResponse = await SnesConnector.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = startingAddress,
            Length = bytes
        });

        if (!firstBlockResponse.Successful || !firstBlockResponse.HasData)
        {
            return;
        }

        List<Item> receivedItems = [];

        for (var i = 0; i < itemCountDifference; i++)
        {
            if (_itemDetailLength == 4)
            {
                var itemId = (ItemType)firstBlockResponse.Data.ReadUInt16(i * 4 + 2)!.Value;
                receivedItems.Add(Tracker.World.AllItems.First(x => x.Type == itemId && x.IsLocalPlayerItem));
            }
            else
            {
                var itemId = firstBlockResponse.Data.ReadUInt8(i * 2 + 1)!.Value;
                try
                {
                    var itemType = (ItemType)itemId;
                    receivedItems.Add(Tracker.World.AllItems.First(x => x.Type == itemType && x.IsLocalPlayerItem));
                }
                catch (Exception)
                {
                    logger.LogWarning("Invalid item type {Id} received", itemId);
                }
            }
        }

        if (receivedItems.Count > 0)
        {
            logger.LogInformation("Received Items: {Items}", string.Join(",", receivedItems));
            Tracker.ItemTracker.TrackItems(receivedItems, true, true);
        }

        trackerState.GiftedItemCount = newItemCount.Value;
        await Tracker.SaveAsync();
    }
}
