using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

/// <summary>
/// Metroid state check for changing regions
/// Checks if the game is in the overworld state and was either not in the overworld state previously or has changed overworld screens
/// </summary>
public class ChangedMetroidRegion : IMetroidStateCheck
{
    private int _previousMetroidRegionValue = -1;

    /// <summary>
    /// Executes the check for the current state
    /// </summary>
    /// <param name="tracker">The tracker instance</param>
    /// <param name="currentState">The current state in Super Metroid</param>
    /// <param name="prevState">The previous state in Super Metroid</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
    {
        if ((currentState.CurrentRegion == _previousMetroidRegionValue &&
             tracker.GameStateTracker.CurrentRegion is not Z3Region) || currentState.CurrentRegion == null)
        {
            return false;
        }

        var startedAtShip = currentState.CurrentRoomInRegion == 0 && currentState.CurrentRegion == 0 && currentState.IsSamusInArea(1125, 1175, 1050, 1100);
        var newRegion = tracker.World.Regions.OfType<SMRegion>().FirstOrDefault(x => x.MemoryRegionId == currentState.CurrentRegion);
        if (newRegion != null)
        {
            tracker.GameStateTracker.UpdateRegion(newRegion, tracker.Options.AutoMapUpdateBehavior, startedAtShip);
        }
        _previousMetroidRegionValue = currentState.CurrentRegion.Value;
        return true;
    }
}
