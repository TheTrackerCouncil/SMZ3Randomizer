using System;
using System.Threading.Tasks;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using SNI;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for tracking how many Peg World pegs have been hammered
/// </summary>
public class PegWorld(TrackerBase tracker, ISnesConnectorService snesConnector) : IZeldaStateCheck
{
    private bool _enabledOnGameChangedCheck;

    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="trackerBase">The tracker instance</param>
    /// <param name="currentState">The current state in Zelda</param>
    /// <param name="prevState">The previous state in Zelda</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase trackerBase, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (currentState.OverworldScreen == 0x62)
        {
            _ = CountPegs();
            return true;
        }

        if (tracker.ModeTracker.PegWorldMode && !(currentState.OverworldScreen == 0x62 || currentState is { OverworldScreen: 0, CurrentRoom: 295 }))
        {
            DisablePegWorldNode();
        }

        return false;
    }

    private void EnableGameChangedCheck()
    {
        if (_enabledOnGameChangedCheck || tracker.AutoTracker == null) return;
        _enabledOnGameChangedCheck = true;
        tracker.AutoTracker.GameChanged += AutoTrackerOnGameChanged;
    }

    private void DisableGameChangedCheck()
    {
        if (!_enabledOnGameChangedCheck || tracker.AutoTracker == null ) return;
        _enabledOnGameChangedCheck = false;
        tracker.AutoTracker.GameChanged -= AutoTrackerOnGameChanged;
    }

    private void AutoTrackerOnGameChanged(object? sender, EventArgs e)
    {
        DisablePegWorldNode();
    }

    private void DisablePegWorldNode()
    {
        if (tracker.ModeTracker.PegWorldMode)
        {
            tracker.ModeTracker.StopPegWorldMode();
        }

        DisableGameChangedCheck();
    }

    private async Task CountPegs()
    {
        var response = await snesConnector.MakeMemoryRequestAsync(new SnesSingleMemoryRequest()
        {
            MemoryRequestType = SnesMemoryRequestType.RetrieveMemory,
            SnesMemoryDomain = SnesMemoryDomain.ConsoleRAM,
            AddressFormat = AddressFormat.Snes9x,
            SniMemoryMapping = MemoryMapping.ExHiRom,
            Address = 0x7e04c8,
            Length = 0x01, // This is actually a four-byte value, but practically, only the lowest byte is important
        });

        if (!response.Successful || !response.HasData) return;

        var count = response.Data.ReadUInt8(0);

        if (count != null && tracker.ModeTracker.SetPegs((int)count))
        {
            EnableGameChangedCheck();
        }
    }
}
