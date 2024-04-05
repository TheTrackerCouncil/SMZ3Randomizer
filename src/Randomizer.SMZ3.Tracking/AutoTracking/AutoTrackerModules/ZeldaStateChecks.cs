using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;

namespace Randomizer.SMZ3.Tracking.AutoTracking.AutoTrackerModules;

public class ZeldaStateChecks(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<ZeldaStateChecks> logger, IEnumerable<IZeldaStateCheck> zeldaStateChecks) : AutoTrackerModule(tracker, snesConnector, logger)
{
    private bool _seenGTTorch;

    public override void Initialize()
    {
        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7e0000,
            Length = 0x250,
            OnResponse = CheckZeldaState,
            Filter = () => IsInZelda
        });
    }

    private void CheckZeldaState(SnesData data, SnesData? prevData)
    {
        var prevState = AutoTracker.ZeldaState;
        var zeldaState = AutoTracker.ZeldaState = new(data);
        Logger.LogDebug("{StateDetails}", zeldaState.ToString());
        if (prevState == null) return;

        if (!_seenGTTorch
            && zeldaState is { CurrentRoom: 140, IsOnBottomHalfOfRoom: false, IsOnRightHalfOfRoom: false }
            && zeldaState.Substate != 14)
        {
            _seenGTTorch = true;
            AutoTracker.IncrementGTItems(Tracker.World.GanonsTower.BobsTorch);
        }

        // Entered the triforce room
        if (zeldaState.State == 0x19)
        {
            if (AutoTracker.HasDefeatedBothBosses)
            {
                Tracker.GameBeaten(true);
            }
        }

        foreach (var check in zeldaStateChecks)
        {
            if (check.ExecuteCheck(Tracker, zeldaState, prevState))
            {
                Logger.LogInformation("{StateName} detected", check.GetType().Name);
            }
        }
    }
}
