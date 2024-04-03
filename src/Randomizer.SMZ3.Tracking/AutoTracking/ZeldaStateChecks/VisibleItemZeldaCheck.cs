using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;

public class VisibleItemZeldaCheck : IZeldaStateCheck
{
    private readonly Dictionary<int, VisibleItemZelda> _overworldVisibleItems;
    private readonly Dictionary<int, VisibleItemZelda> _underworldVisibleItems;
    private readonly HashSet<VisibleItemArea> _trackedAreas = new();
    private readonly IWorldService _worldService;

    public VisibleItemZeldaCheck(IWorldService worldService)
    {
        _worldService = worldService;
        var visibleItems = VisibleItems.GetVisibleItems().ZeldaItems;
        _overworldVisibleItems = visibleItems.Where(x => x.OverworldScreen > 0)
            .ToDictionary(s => (int)s.OverworldScreen!, s => s);
        _underworldVisibleItems = visibleItems.Where(x => x.OverworldScreen is null or 0)
            .ToDictionary(s => (int)s.Room!, s => s);
    }

    public bool ExecuteCheck(TrackerBase tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (currentState.OverworldScreen > 0 && _overworldVisibleItems.TryGetValue(currentState.OverworldScreen.Value, out var visibleItems))
        {
            return CheckItems(visibleItems, _overworldVisibleItems, currentState, tracker);
        }
        else if (currentState is { CurrentRoom: not null, OverworldScreen: 0 } && _underworldVisibleItems.TryGetValue(currentState.CurrentRoom.Value, out visibleItems))
        {
            return CheckItems(visibleItems, _underworldVisibleItems, currentState, tracker);
        }
        return false;
    }

    private bool CheckItems(VisibleItemZelda details, Dictionary<int, VisibleItemZelda> itemsDictionary, AutoTrackerZeldaState currentState, TrackerBase tracker)
    {
        var areas = details.Areas
            .Where(x => !_trackedAreas.Contains(x) && currentState.IsWithinRegion(x.TopLeftX, x.TopLeftY, x.BottomRightX, x.BottomRightY))
            .ToList();

        if (!areas.Any())
        {
            return false;
        }

        tracker.AutoTracker!.SetLatestViewAction($"VisibleItemZeldaCheck_{areas.First().Locations.First()}", () =>
        {
            var toClearLocations = new List<Location>();

            foreach (var area in areas)
            {
                var locations = area.Locations.Select(x => tracker.World.FindLocation(x)).ToList();

                if (locations.All(x => x.State.Cleared || x.State.Autotracked || x.State.MarkedItem == x.State.Item))
                {
                    _trackedAreas.Add(area);
                    continue;
                }

                toClearLocations.AddRange(locations.Where(x => x.State is { Cleared: false, Autotracked: false } &&
                                                               !x.State.Item.IsEquivalentTo(x.State.MarkedItem)));
            }

            foreach (var location in toClearLocations)
            {
                tracker.MarkLocation(location, location.Item.Type.GetGenericType());
            }

            if (toClearLocations.Any())
            {
                tracker.UpdateLastMarkedLocations(toClearLocations);
            }

            // Remove overworld/dungeon room if all items have been collected or marked
            var key = currentState.OverworldScreen == 0 ? currentState.CurrentRoom : currentState.OverworldScreen;

            if (key == null)
            {
                return;
            }

            if (itemsDictionary[key.Value].Areas
                .SelectMany(x => x.Locations.Select(l => tracker.World.FindLocation(l)))
                .All(x => x.State.Cleared || x.State.Autotracked ||
                          x.State.Item.IsEquivalentTo(x.State.MarkedItem)))
            {
                itemsDictionary.Remove(key.Value);
            }
        });

        return true;
    }
}
