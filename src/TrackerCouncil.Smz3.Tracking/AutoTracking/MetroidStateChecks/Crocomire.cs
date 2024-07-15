using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

/// <summary>
/// Metroid state check for nearing Crocomire
/// Player is in the room above Crocomire and is near the door to Crocomire
/// </summary>
public class Crocomire(IItemService itemService) : IMetroidStateCheck
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
        if (currentState is { CurrentRegion: 2, CurrentRoomInRegion: 9, SamusX: >= 3000, SamusY: > 500 } && (!tracker.World.Config.MetroidKeysanity || itemService.IsTracked(ItemType.CardNorfairBoss)))
        {
            tracker.Say(x => x.AutoTracker.NearCrocomire, args: [currentState.SuperMissiles, currentState.MaxSuperMissiles], once: true);
            return true;
        }
        return false;
    }
}
