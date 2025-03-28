using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

/// <summary>
/// Metroid state check for nearing Shaktool
/// Player enters the room with the grapple block from the left
/// </summary>
public class Shaktool(TrackerBase tracker) : IMetroidStateCheck
{
    private readonly HashSet<int> _shaktoolRooms = [70, 154, 197, 208];
    private bool _enabledOnGameChangedCheck;

    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="trackerBase">The tracker instance</param>
    /// <param name="currentState">The current state in Super Metroid</param>
    /// <param name="prevState">The previous state in Super Metroid</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase trackerBase, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
    {
        if (currentState is { CurrentRegion: 4, CurrentRoomInRegion: 36 } && prevState.CurrentRoomInRegion == 28 &&
            tracker.World.FindLocation(LocationId.InnerMaridiaSpringBall).Cleared != true &&
            tracker.World.AllBosses.FirstOrDefault(x => x.Name == "Shaktool")?.Defeated != true)
        {
            tracker.ShutUp();
            tracker.Say(x => x.AutoTracker.NearShaktool, once: true);
            tracker.ModeTracker.StartShaktoolMode();
            EnableGameChangedCheck();
            return true;
        }

        if (tracker.ModeTracker.ShaktoolMode && (currentState.CurrentRoom == null ||
            !_shaktoolRooms.Contains(currentState.CurrentRoom.Value)))
        {
            DisableShaktoolMode();
            return true;
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
        DisableShaktoolMode();
    }

    private void DisableShaktoolMode()
    {
        if (tracker.ModeTracker.ShaktoolMode)
        {
            tracker.ModeTracker.StopShaktoolMode();
        }

        DisableGameChangedCheck();
    }
}
