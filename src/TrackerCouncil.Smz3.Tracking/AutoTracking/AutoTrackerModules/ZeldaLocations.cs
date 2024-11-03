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
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class ZeldaLocations(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<ZeldaLocations> logger, IWorldQueryService worldQueryService)
    : AutoTrackerModule(tracker, snesConnector, logger)
{
    private List<Location> _locations = new();

    public override void Initialize()
    {
        _locations = worldQueryService.AllLocations().Where(x =>
            x.MemoryType == LocationMemoryType.Default && (int)x.Id >= 256).ToList();

        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7ef000,
            Length = 0x250,
            FrequencySeconds = 1,
            OnResponse = CheckZeldaRoomsAndDungeons,
            Filter = () => IsInZelda
        });
    }

    private void CheckZeldaRoomsAndDungeons(SnesData data, SnesData? prevData)
    {
        if (prevData == null)
        {
            return;
        }

        CheckLocations(data, prevData);
        CheckDungeons(data, prevData);
    }

    private void CheckLocations(SnesData data, SnesData prevData)
    {
        foreach (var location in _locations)
        {
            try
            {
                var loc = location.MemoryAddress ?? 0;
                var flag = location.MemoryFlag ?? 0;
                var currentCleared = data.CheckInt16Flag(loc * 2, flag);
                var prevCleared = prevData.CheckInt16Flag(loc * 2, flag);
                if (location.Autotracked == false && currentCleared && prevCleared)
                {
                    // Increment GT guessing game number
                    if (location.Region is GanonsTower gt && location != gt.BobsTorch)
                    {
                        AutoTracker.IncrementGTItems(location);
                    }

                    TrackLocation(location);

                    // Mark HC as cleared if this was Zelda's Cell
                    if (location.Id == LocationId.HyruleCastleZeldasCell && !Tracker.World.HyruleCastle.Boss.Defeated)
                    {
                        Tracker.BossTracker.MarkBossAsDefeated(Tracker.World.HyruleCastle, autoTracked: true);
                    }
                }

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to auto track location: {LocationName}", location.Name);
                Tracker.Error();
            }
        }
    }

    private void CheckDungeons(SnesData data, SnesData prevData)
    {
        foreach (var dungeon in Tracker.World.BossRegions.Where(x => x is Z3Region))
        {
            var region = (Z3Region)dungeon;

            // Skip if we don't have any memory addresses saved for this dungeon
            if (region.MemoryAddress == null || region.MemoryFlag == null)
            {
                continue;
            }

            try
            {
                var prevValue = prevData.CheckInt16Flag((int)(region.MemoryAddress * 2), region.MemoryFlag ?? 0);
                var currentValue = data.CheckInt16Flag((int)(region.MemoryAddress * 2), region.MemoryFlag ?? 0);
                if (dungeon.BossState.AutoTracked == false && prevValue && currentValue)
                {
                    dungeon.BossState.AutoTracked = true;
                    Tracker.BossTracker.MarkBossAsDefeated(dungeon, autoTracked: true, admittedGuilt: true);
                    Logger.LogInformation("Auto tracked {DungeonName} as cleared", dungeon.Name);
                }

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to auto track Dungeon: {DungeonName}", dungeon.Name);
                Tracker.Error();
            }
        }
    }
}
