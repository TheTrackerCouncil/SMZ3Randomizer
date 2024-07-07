using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

/// <summary>
/// Metroid state check related to greeting the Ridley face
/// Player is in that room and has reached the face
/// </summary>
public class Ridley : IMetroidStateCheck
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
        if (currentState.CurrentRegion == 2 && currentState.CurrentRoomInRegion == 37 && currentState.SamusX <= 375 && currentState.SamusX >= 100 && currentState.SamusY <= 200)
        {
            tracker.Say(x => x.AutoTracker.RidleyFace, once: true);
            return true;
        }
        return false;
    }
}
