using System.Collections.Generic;
using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks;

public class VisibleItemMetroidCheck : IMetroidStateCheck
{
    public Dictionary<int, List<VisibleItemMetroid>> Items;

    public VisibleItemMetroidCheck()
    {
        var visibleItems = VisibleItems.GetVisibleItems().MetroidItems;
        Items = visibleItems.Select(x => x.Room)
            .ToDictionary(s => s, s => visibleItems.Where(i => i.Room == s).ToList());
    }

    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState,
        AutoTrackerMetroidState prevState)
    {
        if (Items.TryGetValue(currentState.CurrentRoom, out var visibleItems))
        {
            var locations = visibleItems.Where(x =>
                    currentState.IsSamusInArea(x.TopLeftX, x.BottomRightX, x.TopLeftY, x.BottomRightY))
                .SelectMany(x => x.Locations)
                .Select(x => tracker.World.FindLocation(x))
                .ToList();

            if (!locations.Any())
            {
                return false;
            }

            if (locations.All(x => x.State.Cleared || x.State.Autotracked || x.State.MarkedItem == x.State.Item))
            {
                Items.Remove(currentState.CurrentRoom);
                return false;
            }

            tracker.AutoTracker!.SetLatestViewAction(() =>
            {
                var toClearLocations = locations.Where(x => x.State is { Cleared: false, Autotracked: false } &&
                                                            x.State.MarkedItem != x.State.Item).ToList();
                foreach (var location in toClearLocations)
                {
                    tracker.MarkLocation(location, location.Item);
                }

                if (toClearLocations.Any())
                {
                    tracker.UpdateLastMarkedLocations(toClearLocations);
                }

                Items.Remove(currentState.CurrentRoom);
            });

            return true;
        }
        return false;
    }
}
