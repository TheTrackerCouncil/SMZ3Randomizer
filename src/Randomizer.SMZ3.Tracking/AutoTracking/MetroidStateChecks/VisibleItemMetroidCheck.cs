using System.Collections.Generic;
using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Enums;

namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks;

public class VisibleItemMetroidCheck : IMetroidStateCheck
{
    private readonly Dictionary<int, List<VisibleItemMetroid>> _items;
    private readonly HashSet<LocationId> _trackedLocationIds = new();

    public VisibleItemMetroidCheck()
    {
        var visibleItems = VisibleItems.GetVisibleItems().MetroidItems;
        _items = visibleItems.Select(x => x.Room)
            .ToDictionary(s => s, s => visibleItems.Where(i => i.Room == s).ToList());
    }

    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState,
        AutoTrackerMetroidState prevState)
    {
        if (_items.TryGetValue(currentState.CurrentRoom, out var visibleItems))
        {
            var locations = visibleItems.Where(x =>
                    currentState.IsSamusInArea(x.TopLeftX, x.BottomRightX, x.TopLeftY, x.BottomRightY))
                .SelectMany(x => x.Locations)
                .Where(x => !_trackedLocationIds.Contains(x))
                .Select(x => tracker.World.FindLocation(x))
                .ToList();

            if (!locations.Any() || locations.All(x => x.State.Cleared || x.State.Autotracked ||  x.State.Item.IsEquivalentTo(x.State.MarkedItem)))
            {
                return false;
            }

            tracker.AutoTracker!.SetLatestViewAction($"VisibleItemMetroidCheck_{locations.First().Id}", () =>
            {
                var toClearLocations = locations.Where(x => x.State is { Cleared: false, Autotracked: false } &&
                                                            !x.State.Item.IsEquivalentTo(x.State.MarkedItem)).ToList();
                foreach (var location in toClearLocations)
                {
                    tracker.MarkLocation(location, location.Item.Type.GetGenericType());
                    _trackedLocationIds.Add(location.Id);
                }

                if (toClearLocations.Any())
                {
                    tracker.UpdateLastMarkedLocations(toClearLocations);
                }

                if (_items[currentState.CurrentRoom]
                    .SelectMany(x => x.Locations.Select(l => tracker.World.FindLocation(l)))
                    .All(x => x.State.Cleared || x.State.Autotracked ||
                              x.State.Item.IsEquivalentTo(x.State.MarkedItem)))
                {
                    _items.Remove(currentState.CurrentRoom);
                }
            });

            return true;
        }
        return false;
    }
}
