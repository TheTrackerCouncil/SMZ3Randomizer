using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class MultiplayerSync(
    TrackerBase tracker,
    ISnesConnectorService snesConnector,
    ILogger<MultiplayerSync> logger,
    IGameService gameService,
    IWorldService worldService) : AutoTrackerModule(tracker, snesConnector, logger)
{
    public override void Initialize()
    {
        if (!Tracker.World.Config.MultiWorld)
        {
            return;
        }

        // Get first block of memory
        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0xA26000,
            Length = 0x300,
            FrequencySeconds = 30,
            OnResponse = OnMemorySync,
            Filter = () => IsInGame
        });
    }

    private async void OnMemorySync(SnesData firstDataSet, SnesData? firstPrevData)
    {
        // Get the second block of memory upon receiving first block of memory
        var response = await SnesConnector.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.CartridgeSave,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0xA26300,
            Length = 0x300
        });

        if (!response.Successful || !response.HasData)
        {
            return;
        }

        var secondDataSet = response.Data;
        var data = firstDataSet.Raw.Concat(secondDataSet.Raw).ToArray();

        var previouslyGiftedItems = new List<(ItemType type, int fromPlayerId)>();
        for (var i = 0; i < 0x150; i++)
        {
            var item = (ItemType)BitConverter.ToUInt16(data.AsSpan(i * 4 + 2, 2));
            if (item == ItemType.Nothing)
            {
                continue;
            }

            var playerId = BitConverter.ToUInt16(data.AsSpan(i * 4, 2));
            previouslyGiftedItems.Add((item, playerId));
        }

        var otherCollectedItems = worldService.Worlds.SelectMany(x => x.Locations)
            .Where(x => x.State.ItemWorldId == Tracker.World.Id && x.State.WorldId != Tracker.World.Id &&
                        x.Autotracked && (!x.World.HasCompleted || !x.Item.Type.IsInCategory(ItemCategory.IgnoreOnMultiplayerCompletion)))
            .Select(x => (x.State.Item, x.State.WorldId)).ToList();

        foreach (var item in previouslyGiftedItems)
        {
            otherCollectedItems.Remove(item);
        }

        if (otherCollectedItems.Any())
        {
            Logger.LogInformation("Giving player {ItemCount} missing items", otherCollectedItems.Count);
            _ = gameService.TryGiveItemTypesAsync(otherCollectedItems);
        }
    }
}
