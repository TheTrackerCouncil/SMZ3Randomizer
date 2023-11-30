using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Enums;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

public class VisibleItemZeldaCheck : IZeldaStateCheck
{
    private readonly Dictionary<int, List<VisibleItemZelda>> _overworldVisibleItems;
    private readonly Dictionary<int, List<VisibleItemZelda>> _underworldVisibleItems;
    private readonly HashSet<LocationId> _trackedLocationIds = new();

    public VisibleItemZeldaCheck()
    {
        var visibleItems = VisibleItems.GetVisibleItems().ZeldaItems;
        _overworldVisibleItems = visibleItems.Where(x => x.OverworldScreen > 0)
            .Select(x => (int)x.OverworldScreen!)
            .ToDictionary(s => s, s => visibleItems.Where(i => i.OverworldScreen == s).ToList());
        _underworldVisibleItems = visibleItems.Where(x => x.OverworldScreen is null or 0)
            .Select(x => (int)x.Room!)
            .ToDictionary(r => r, r => visibleItems.Where(i => i.Room == r).ToList());
    }

    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (currentState.OverworldScreen > 0 && _overworldVisibleItems.TryGetValue(currentState.OverworldScreen, out var visibleItems))
        {
            var locations = visibleItems.Where(x =>
                    currentState.IsWithinRegion(x.TopLeftX, x.TopLeftY, x.BottomRightX, x.BottomRightY))
                .SelectMany(x => x.Locations)
                .Where(x => !_trackedLocationIds.Contains(x))
                .Select(x => tracker.World.FindLocation(x))
                .ToList();

            return CheckItems(locations, _overworldVisibleItems, currentState, tracker);
        }
        else if (currentState.OverworldScreen == 0 && _underworldVisibleItems.TryGetValue(currentState.CurrentRoom, out visibleItems))
        {
            var locations = visibleItems.Where(x =>
                    currentState.IsWithinRegion(x.TopLeftX, x.TopLeftY, x.BottomRightX, x.BottomRightY))
                .SelectMany(x => x.Locations)
                .Where(x => !_trackedLocationIds.Contains(x))
                .Select(x => tracker.World.FindLocation(x))
                .ToList();

            return CheckItems(locations, _underworldVisibleItems, currentState, tracker);
        }
        return false;
    }

    private bool CheckItems(List<Location> locations, Dictionary<int, List<VisibleItemZelda>> items, AutoTrackerZeldaState currentState, TrackerBase tracker)
    {
        if (!locations.Any() || locations.All(x => x.State.Cleared || x.State.Autotracked || x.State.MarkedItem == x.State.Item))
        {
            return false;
        }

        tracker.AutoTracker!.SetLatestViewAction($"VisibleItemZeldaCheck_{locations.First().Id}", () =>
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

            // Remove
            var key = currentState.OverworldScreen == 0 ? currentState.CurrentRoom : currentState.OverworldScreen;
            if (items[key]
                .SelectMany(x => x.Locations.Select(l => tracker.World.FindLocation(l)))
                .All(x => x.State.Cleared || x.State.Autotracked ||
                          x.State.Item.IsEquivalentTo(x.State.MarkedItem)))
            {
                items.Remove(key);
            }
        });

        return true;
    }
}
