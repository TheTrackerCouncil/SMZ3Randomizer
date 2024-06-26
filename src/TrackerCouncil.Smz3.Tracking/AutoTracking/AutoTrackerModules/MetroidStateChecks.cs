using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SnesConnectorLibrary.Responses;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;

public class MetroidStateChecks(TrackerBase tracker, ISnesConnectorService snesConnector, ILogger<MetroidStateChecks> logger, IEnumerable<IMetroidStateCheck> metroidStateChecks) : AutoTrackerModule(tracker, snesConnector, logger)
{
    public override void Initialize()
    {
        SnesConnector.AddRecurringMemoryRequest(new SnesRecurringMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7e0750,
            Length = 0x400,
            OnResponse = CheckMetroidState,
            Filter = () => IsInMetroid
        });
    }

    private void CheckMetroidState(SnesData data, SnesData? prevData)
    {
        var prevState = AutoTracker.MetroidState;
        var MetroidState = AutoTracker.MetroidState = new AutoTrackerMetroidState(data);
        Logger.LogDebug("{StateDetails}", MetroidState.ToString());
        if (prevState == null) return;

        // If the game hasn't booted up, wait until we find valid data in the Metroid state before we start
        // checking locations
        if (AutoTracker.HasValidState != MetroidState.IsValid)
        {
            AutoTracker.UpdateValidState(MetroidState.IsValid);
            if (MetroidState.IsValid)
            {
                Logger.LogInformation("Valid game state detected");
            }
        }

        if (!MetroidState.IsValid) return;

        foreach (var check in metroidStateChecks)
        {
            if (check.ExecuteCheck(Tracker, MetroidState, prevState))
            {
                Logger.LogInformation("{StateName} detected", check.GetType().Name);
            }
        }
    }
}
