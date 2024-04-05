using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for changing overworld locations
/// Checks if the game is in the overworld state and was either not in the overworld state previously or has changed overworld screens
/// </summary>
public class ChangedOverworld : IZeldaStateCheck
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
        if (currentState.State != 0x09 ||
            (prevState.State == 0x09 && currentState.OverworldScreen == prevState.OverworldScreen) ||
            currentState.OverworldScreen == null)
        {
            return false;
        }

        var region = tracker.World.Regions
            .OfType<Z3Region>()
            .FirstOrDefault(x => x.StartingRooms.Count != 0 && x.StartingRooms.Contains(currentState.OverworldScreen.Value) && x.IsOverworld);
        if (region == null) return false;

        tracker.UpdateRegion(region, tracker.Options.AutoTrackerChangeMap);
        return true;
    }
}
