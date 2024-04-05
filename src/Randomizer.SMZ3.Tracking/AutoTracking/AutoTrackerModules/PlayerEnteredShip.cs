﻿using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;

namespace Randomizer.SMZ3.Tracking.AutoTracking.AutoTrackerModules;

public class PlayerEnteredShip(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<PlayerEnteredShip> logger) : AutoTrackerModule(tracker, snesConnector, logger)
{
    public override void Initialize()
    {
        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7e0FB2,
            Length = 0x2,
            OnResponse = CheckShip,
            Filter = () => IsInMetroid && AutoTracker.HasDefeatedBothBosses
        });
    }

    private void CheckShip(SnesData data, SnesData? prevData)
    {
        if (prevData == null)
        {
            return;
        }
        if (data.ReadUInt16(0) == 0xAA4F)
        {
            Tracker.GameBeaten(true);
        }
    }
}
