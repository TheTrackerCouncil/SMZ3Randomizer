using Randomizer.Abstractions;
using Randomizer.Data.Tracking;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for falling from Ganon's room
/// Checks if the current room is the one below Ganon and the previous room was the Ganon room
/// </summary>
public class FallFromGanon : IZeldaStateCheck
{
    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="tracker">The tracker instance</param>
    /// <param name="currentState">The current state in Zelda</param>
    /// <param name="prevState">The previous state in Zelda</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (currentState.CurrentRoom == 16 && currentState.PreviousRoom == 0 && prevState.CurrentRoom == 0)
        {
            tracker.SayOnce(x => x.AutoTracker.FallFromGanon);
            return true;
        }
        return false;
    }
}
