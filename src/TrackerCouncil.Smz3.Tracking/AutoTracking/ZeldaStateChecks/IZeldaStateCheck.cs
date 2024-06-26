using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Interface for Zelda state checks
/// </summary>
public interface IZeldaStateCheck
{
    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="trackerBase">The tracker instance</param>
    /// <param name="currentState">The current state in Zelda</param>
    /// <param name="prevState">The previous state in Zelda</param>
    /// <returns>True if the state was found</returns>
    bool ExecuteCheck(TrackerBase trackerBase, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState);
}
