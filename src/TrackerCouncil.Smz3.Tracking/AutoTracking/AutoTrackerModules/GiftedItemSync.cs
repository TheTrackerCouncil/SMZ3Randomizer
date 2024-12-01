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
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class GiftedItemSync(
    TrackerBase tracker,
    ISnesConnectorService snesConnector,
    ILogger<GiftedItemSync> logger,
    IGameService gameService,
    IWorldQueryService worldQueryService) : AutoTrackerModule(tracker, snesConnector, logger)
{
    public override void Initialize()
    {
        snesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0xA26602,
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
        if (newItemCount == null || trackerState == null)
        {
            return;
        }

        var currentGiftedItemCount = trackerState.GiftedItemCount;
        var itemCountDifference = newItemCount.Value - currentGiftedItemCount;
        var startingAddress = 0xA26000 + currentGiftedItemCount * 4;
        var bytes = itemCountDifference * 4;

        var firstBlockResponse = await snesConnector.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
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
            var fromPlayerId = firstBlockResponse.Data.ReadUInt16(i * 4)!.Value;
            var itemId = (ItemType)firstBlockResponse.Data.ReadUInt16(i * 4 + 2)!.Value;
            receivedItems.Add(Tracker.World.AllItems.First(x => x.Type == itemId));
        }

        Tracker.ItemTracker.TrackItems(receivedItems, true, true);

        trackerState.GiftedItemCount = newItemCount.Value;
        await Tracker.SaveAsync();
    }
}
