﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.Data.WorldData;
using Randomizer.SMZ3.Tracking.Services;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;

namespace Randomizer.SMZ3.Tracking.AutoTracking.AutoTrackerModules;

public class MetroidLocations(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<MetroidLocations> logger, IWorldService worldService) : AutoTrackerModule(tracker, snesConnector, logger)
{
    private List<Location> _locations = new();

    public override void Initialize()
    {
        _locations = worldService.AllLocations().Where(x => (int)x.Id < 256).ToList();

        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7ed870,
            Length = 0x20,
            FrequencySeconds = 1,
            OnResponse = CheckLocations,
            Filter = () => IsInMetroid
        });
    }

    private void CheckLocations(SnesData data, SnesData? prevData)
    {
        if (prevData == null)
        {
            return;
        }

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
