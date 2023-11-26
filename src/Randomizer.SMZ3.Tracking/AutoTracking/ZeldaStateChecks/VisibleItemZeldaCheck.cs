using System.Collections.Generic;
using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

public class VisibleItemZeldaCheck : IZeldaStateCheck
{
    public Dictionary<int, List<VisibleItemZelda>> OverworldVisibleItems;
    public Dictionary<int, List<VisibleItemZelda>> UnderworldVisibleItems;

    public VisibleItemZeldaCheck()
    {
        var visibleItems = VisibleItems.GetVisibleItems().ZeldaItems;
        OverworldVisibleItems = visibleItems.Where(x => x.OverworldScreen > 0)
            .Select(x => (int)x.OverworldScreen!)
            .ToDictionary(s => s, s => visibleItems.Where(i => i.OverworldScreen == s).ToList());
        UnderworldVisibleItems = visibleItems.Where(x => x.OverworldScreen is null or 0)
            .Select(x => (int)x.Room!)
            .ToDictionary(r => r, r => visibleItems.Where(i => i.Room == r).ToList());
    }

    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (currentState.OverworldScreen > 0 && OverworldVisibleItems.TryGetValue(currentState.OverworldScreen, out var visibleItems))
        {
            var locations = visibleItems.Where(x =>
                    currentState.IsWithinRegion(x.TopLeftX, x.TopLeftY, x.BottomRightX, x.BottomRightY))
                .SelectMany(x => x.Locations)
                .Select(x => tracker.World.FindLocation(x))
                .ToList();

            return CheckItems(locations, OverworldVisibleItems, currentState, tracker);
        }
        else if (currentState.OverworldScreen == 0 && UnderworldVisibleItems.TryGetValue(currentState.CurrentRoom, out visibleItems))
        {
            var locations = visibleItems.Where(x =>
                    currentState.IsWithinRegion(x.TopLeftX, x.TopLeftY, x.BottomRightX, x.BottomRightY))
                .SelectMany(x => x.Locations)
                .Select(x => tracker.World.FindLocation(x))
                .ToList();

            return CheckItems(locations, UnderworldVisibleItems, currentState, tracker);
        }
        return false;
    }

    private bool CheckItems(List<Location> locations, Dictionary<int, List<VisibleItemZelda>> items, AutoTrackerZeldaState currentState, TrackerBase tracker)
    {
        if (!locations.Any())
        {
            return false;
        }

        if (locations.All(x => x.State.Cleared || x.State.Autotracked || x.State.MarkedItem == x.State.Item))
        {
            items.Remove(currentState.CurrentRoom);
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

            items.Remove(currentState.CurrentRoom);
        });

        return true;
    }
}
