using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

public class ViewedTourianBossCount : IMetroidStateCheck
{
    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState,
        AutoTrackerMetroidState prevState)
    {
        var requiredBossCount = tracker.World.State?.TourianBossCount ?? tracker.World.Config.TourianBossCount;

        if (tracker.World.State?.MarkedTourianBossCount == requiredBossCount)
        {
            return false;
        }

        if (currentState is { CurrentRegion: 0, CurrentRoomInRegion: 48, SamusX: > 1169 and < 1250 })
        {
            tracker.GameStateTracker.UpdateTourianRequirement(requiredBossCount, true);
            return true;
        }

        return false;
    }
}
