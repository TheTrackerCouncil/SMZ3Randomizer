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
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class ZeldaMiscData(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<ZeldaMiscData> logger, IWorldService worldService, IItemService itemService) : AutoTrackerModule(tracker, snesConnector, logger)
{
    private List<Location> _locations = new();

    public override void Initialize()
    {
        _locations = worldService.AllLocations().Where(x =>
            x.MemoryType == LocationMemoryType.ZeldaMisc && (int)x.Id >= 256).ToList();

        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7ef280,
            Length = 0x200,
            FrequencySeconds = 1,
            OnResponse = CheckZeldaMisc,
            Filter = () => IsInZelda
        });
    }

    private void CheckZeldaMisc(SnesData data, SnesData? prevData)
    {
        if (!HasValidState || prevData == null) return;

        // Failsafe to prevent incorrect checking
        if (data.ReadUInt8(0x190) == 0xFF && data.ReadUInt8(0x191) == 0xFF)
        {
            Logger.LogInformation("Ignoring due to transition");
            return;
        }

        CheckLocations(data, prevData);

        var hasFairy = false;
        for (var i = 0; i < 4; i++)
        {
            hasFairy |= data.ReadUInt8(0xDC + i) == 6;
        }

        AutoTracker.PlayerHasFairy = hasFairy;

        // Activated flute
        if (data.CheckUInt8Flag(0x10C, 0x01) && !prevData.CheckUInt8Flag(0x10C, 0x01))
        {
            var duckItem = itemService.FirstOrDefault("Duck");
            if (duckItem?.State.TrackingState == 0)
            {
                Tracker.ItemTracker.TrackItem(duckItem, null, null, false, true);
            }
        }

        // Check if the player cleared Aga
        if (data.ReadUInt8(0x145) >= 3)
        {
            var castleTower = Tracker.World.CastleTower;
            if (castleTower.BossState.Defeated == false)
            {
                Tracker.BossTracker.MarkRegionBossAsDefeated(castleTower, null, autoTracked: true);
                Logger.LogInformation("Auto tracked {Name} as cleared", castleTower.Name);
            }
        }
    }

    private void CheckLocations(SnesData data, SnesData prevData)
    {
        foreach (var location in _locations)
        {
            try
            {
                var loc = location.MemoryAddress ?? 0;
                var flag = location.MemoryFlag ?? 0;
                var currentCleared = data.CheckUInt8Flag(loc, flag);
                var prevCleared = prevData.CheckUInt8Flag(loc, flag);
                if (location.State.Autotracked == false && currentCleared && prevCleared)
                {
                    TrackLocation(location);
                }

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unable to auto track location: {LocationName}", location.Name);
                Tracker.Error();
            }
        }
    }
}
