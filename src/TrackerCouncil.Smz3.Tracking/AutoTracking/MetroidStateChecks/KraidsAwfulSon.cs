﻿using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

/// <summary>
/// Metroid state check for nearing Kraid's awful son
/// Player enters the same room as KAD from the left
/// </summary>
public class KraidsAwfulSon : IMetroidStateCheck
{
    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="tracker">The tracker instance</param>
    /// <param name="currentState">The current state in Super Metroid</param>
    /// <param name="prevState">The previous state in Super Metroid</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
    {
        if (currentState.CurrentRegion == 1 && currentState.CurrentRoomInRegion == 45 && prevState.CurrentRoomInRegion == 44)
        {
            tracker.Say(x => x.AutoTracker.NearKraidsAwfulSon, once: true);
            return true;
        }
        return false;
    }
}
