using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for changing overworld locations
/// Checks if the game is in the overworld state and was either not in the overworld state previously or has changed overworld screens
/// </summary>
public class ChangedOverworld : IZeldaStateCheck
{
    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="trackerBase">The tracker instance</param>
    /// <param name="currentState">The current state in Zelda</param>
    /// <param name="prevState">The previous state in Zelda</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase trackerBase, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (currentState.State != 0x09 ||
            (prevState.State == 0x09 && currentState.OverworldScreen == prevState.OverworldScreen) ||
            currentState.OverworldScreen == null)
        {
            return false;
        }

        var region = trackerBase.World.Regions
            .OfType<Z3Region>()
            .FirstOrDefault(x => x.StartingRooms.Count != 0 && x.StartingRooms.Contains(currentState.OverworldScreen.Value) && x.IsOverworld);
        if (region == null) return false;

        trackerBase.GameStateTracker.UpdateRegion(region, trackerBase.Options.AutoMapUpdateBehavior);
        return true;
    }
}
