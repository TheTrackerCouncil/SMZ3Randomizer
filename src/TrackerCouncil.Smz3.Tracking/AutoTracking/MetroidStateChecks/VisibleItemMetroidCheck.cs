using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;

public class VisibleItemMetroidCheck : IMetroidStateCheck
{
    private readonly Dictionary<int, VisibleItemMetroid> _items;
    private readonly HashSet<VisibleItemArea> _trackedAreas = new();

    public VisibleItemMetroidCheck(IWorldQueryService worldQueryService)
    {
        var visibleItems = VisibleItems.GetVisibleItems().MetroidItems;
        _items = visibleItems
            .ToDictionary(s => s.Room, s => s);
    }

    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState,
        AutoTrackerMetroidState prevState)
    {
        if (currentState.CurrentRoom == null)
        {
            return false;
        }

        if (_items.TryGetValue(currentState.CurrentRoom.Value, out var visibleItems))
        {
            CheckItems(visibleItems, currentState, tracker);
            return true;
        }
        return false;
    }

    private bool CheckItems(VisibleItemMetroid details, AutoTrackerMetroidState currentState, TrackerBase tracker)
    {
        var areas = details.Areas
            .Where(x => !_trackedAreas.Contains(x) && currentState.IsSamusInArea(x.TopLeftX, x.BottomRightX, x.TopLeftY, x.BottomRightY))
            .ToList();

        if (!areas.Any())
        {
            return false;
        }

        tracker.AutoTracker!.SetLatestViewAction($"VisibleItemMetroidCheck_{areas.First().Locations.First()}", () =>
        {
            var toClearLocations = new List<Location>();

            foreach (var area in areas)
            {
                var locations = area.Locations.Select(x => tracker.World.FindLocation(x)).ToList();

                if (locations.All(x => x.Cleared || x.Autotracked || x.HasMarkedCorrectItem))
                {
                    _trackedAreas.Add(area);
                    continue;
                }

                toClearLocations.AddRange(locations.Where(x => x is
                    { Cleared: false, Autotracked: false, HasMarkedCorrectItem: false }));
            }

            foreach (var location in toClearLocations)
            {
                tracker.LocationTracker.MarkLocation(location, location.ItemType.GetGenericType());
            }

            if (toClearLocations.Any())
            {
                tracker.GameStateTracker.UpdateLastMarkedLocations(toClearLocations);
            }

            // Remove room if all items have been collected or marked
            if (currentState.CurrentRoom != null && _items[currentState.CurrentRoom.Value].Areas
                .SelectMany(x => x.Locations.Select(l => tracker.World.FindLocation(l)))
                .All(x => x.Cleared || x.Autotracked || x.HasMarkedCorrectItem))
            {
                _items.Remove(currentState.CurrentRoom.Value);
            }
        });

        return true;
    }
}
