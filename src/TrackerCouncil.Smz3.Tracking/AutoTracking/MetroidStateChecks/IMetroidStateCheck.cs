using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

/// <summary>
/// Abstract class for various Metroid state checks
/// </summary>
public interface IMetroidStateCheck
{
    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="tracker">The tracker instance</param>
    /// <param name="currentState">The current state in Super Metroid</param>
    /// <param name="prevState">The previous state in Super Metroid</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState);
}
