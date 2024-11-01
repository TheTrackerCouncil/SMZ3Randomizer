using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.ZeldaStateChecks;

/// <summary>
/// Zelda State check for when dying
/// Checks if the player is in the death spiral animation without a fairy
/// </summary>
public class ZeldaDeath(IWorldQueryService worldQueryService) : IZeldaStateCheck
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
        if (tracker.AutoTracker == null || currentState.State != 0x12 || prevState.State == 0x12 || tracker.AutoTracker.PlayerHasFairy) return false;

        var silent = tracker.GameService!.PlayerRecentlyKilled;

        // Say specific message for dying in the particular screen/room the player is in
        if (!silent && tracker.GameStateTracker.CurrentRegion?.Metadata is { WhenDiedInRoom: not null })
        {
            var region = tracker.GameStateTracker.CurrentRegion as Z3Region;
            if (region is { IsOverworld: true } && prevState.OverworldScreen != null && tracker.GameStateTracker.CurrentRegion?.Metadata.WhenDiedInRoom?.TryGetValue(prevState.OverworldScreen.Value, out var locationResponse) == true)
            {
                tracker.Say(response: locationResponse);
            }
            else if (region is { IsOverworld: false } && prevState.CurrentRoom != null && tracker.GameStateTracker.CurrentRegion?.Metadata.WhenDiedInRoom?.TryGetValue(prevState.CurrentRoom.Value, out locationResponse) == true)
            {
                tracker.Say(response: locationResponse);
            }
        }

        tracker.GameStateTracker.TrackDeath(true);

        var death = worldQueryService.FirstOrDefault("Death");
        if (death is not null)
        {
            tracker.ItemTracker.TrackItem(death, autoTracked: true, silent: silent);
            return true;
        }
        return false;
    }
}
