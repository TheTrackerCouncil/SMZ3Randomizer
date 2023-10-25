using Randomizer.Abstractions;
using Randomizer.Data.Tracking;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for breaking into the Tower of Hera pot
/// player is in the pot room and did not get there from falling from the two rooms above it
/// </summary>
public class HeraPot : IZeldaStateCheck
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
        if (currentState.CurrentRoom == 167 && prevState.CurrentRoom == 119 && prevState.PreviousRoom != 49)
        {
            tracker.SayOnce(x => x.AutoTracker.HeraPot);
            return true;
        }
        return false;
    }
}
