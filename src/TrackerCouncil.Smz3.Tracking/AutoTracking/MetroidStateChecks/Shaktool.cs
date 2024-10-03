using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

/// <summary>
/// Metroid state check for nearing Shaktool
/// Player enters the room with the grapple block from the left
/// </summary>
public class Shaktool : IMetroidStateCheck
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
        if (currentState is { CurrentRegion: 4, CurrentRoomInRegion: 36 } && prevState.CurrentRoomInRegion == 28 &&
            tracker.World.FindLocation(LocationId.InnerMaridiaSpringBall).State.Cleared != true &&
            tracker.World.AllBosses.FirstOrDefault(x => x.Name == "Shaktool")?.State.Defeated != true)
        {
            tracker.ShutUp();
            tracker.Say(x => x.AutoTracker.NearShaktool, once: true);
            tracker.ModeTracker.StartShaktoolMode();
            return true;
        }

        if (tracker.ModeTracker.ShaktoolMode &&
            ((currentState is { CurrentRegion: 4, CurrentRoomInRegion: 28 } && prevState.CurrentRoomInRegion == 36) ||
             currentState.CurrentRegion != 4))
        {
            tracker.ModeTracker.StopShaktoolMode();
            return true;
        }

        return false;
    }
}
