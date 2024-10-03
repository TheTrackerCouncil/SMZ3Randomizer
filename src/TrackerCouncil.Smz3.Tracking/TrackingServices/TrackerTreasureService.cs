using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

public class TrackerTreasureService(ILogger<TrackerTreasureService> logger, IItemService itemService) : TrackerService, ITrackerTreasureService
{
    public bool TrackDungeonTreasure(IHasTreasure region, float? confidence = null, int amount = 1,
        bool autoTracked = false, bool stateResponse = true)
    {
        if (amount < 1)
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount of items must be greater than zero.");

        if (amount > region.RemainingTreasure && !region.HasManuallyClearedTreasure)
        {
            logger.LogWarning("Trying to track {Amount} treasures in a dungeon with only {Left} treasures left.", amount, region.RemainingTreasure);
            Tracker.Say(response: Responses.DungeonTooManyTreasuresTracked, args: [region.Metadata.Name, region.RemainingTreasure, amount]);
            return false;
        }

        if (!autoTracked)
        {
            region.HasManuallyClearedTreasure = true;
        }

        if (region.RemainingTreasure > 0)
        {
            region.RemainingTreasure -= amount;

            // If there are no more treasures and the boss is defeated, clear all locations in the dungeon
            var clearedLocations = new List<Location>();
            if (region.RemainingTreasure == 0 && region is IHasBoss { BossDefeated: true })
            {
                foreach (var location in ((Region)region).Locations.Where(x => !x.State.Cleared))
                {
                    location.State.Cleared = true;
                    if (autoTracked)
                    {
                        location.State.Autotracked = true;
                    }
                    clearedLocations.Add(location);
                }
            }

            // Always add a response if there's treasure left, even when
            // clearing a dungeon (because that means it was out of logic
            // and could be relevant)
            if (stateResponse && (confidence != null || region.RemainingTreasure >= 1 || autoTracked))
            {
                // Try to get the response based on the amount of items left
                if (Responses.DungeonTreasureTracked?.TryGetValue(region.RemainingTreasure, out var response) == true)
                    Tracker.Say(response: response, args: [region.Metadata.Name, region.RemainingTreasure]);
                // If we don't have a response for the exact amount and we
                // have multiple left, get the one for 2 (considered
                // generic)
                else if (region.RemainingTreasure >= 2 && Responses.DungeonTreasureTracked?.TryGetValue(2, out response) == true)
                    Tracker.Say(response: response, args: [region.Metadata.Name, region.RemainingTreasure]);
            }

            AddUndo(() =>
            {
                region.RemainingTreasure += amount;
                foreach (var location in clearedLocations)
                {
                    location.State.Cleared = false;
                }
            });

            return true;
        }
        else if (stateResponse && confidence != null && Responses.DungeonTreasureTracked?.TryGetValue(-1, out var response) == true)
        {
            // Attempted to track treasure when all treasure items were
            // already cleared out
            Tracker.Say(response: response, args: [region.Metadata.Name]);
        }

        return false;
    }

    public Action? TryTrackDungeonTreasure(Location location, float? confidence, bool autoTracked = false, bool stateResponse = true)
    {
        if (confidence < Options.MinimumSassConfidence)
        {
            // Tracker response could give away the location of an item if
            // it is in a dungeon but tracker misheard.
            return null;
        }

        var dungeon = location.GetTreasureRegion();
        if (dungeon != null && (location.Item.IsTreasure || World.Config.ZeldaKeysanity))
        {
            if (TrackDungeonTreasure(dungeon, confidence, 1, autoTracked, stateResponse))
                return PopUndo().Action;
        }

        IsDirty = true;

        return null;
    }

    /// <summary>
    /// Marks all locations and treasure within a dungeon as cleared.
    /// </summary>
    /// <param name="treasureRegion">The dungeon to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void ClearDungeon(IHasTreasure treasureRegion, float? confidence = null)
    {
        var remaining = treasureRegion.RemainingTreasure;
        if (remaining > 0)
        {
            treasureRegion.RemainingTreasure = 0;
        }

        List<Action> undoActions = [];

        var region = (Region)treasureRegion;
        var progress = itemService.GetProgression(assumeKeys: !World.Config.ZeldaKeysanity);
        var locations = region.Locations.Where(x => x.State.Cleared == false).ToList();

        if (remaining <= 0 && locations.Count <= 0)
        {
            // We didn't do anything
            Tracker.Say(x => x.DungeonAlreadyCleared, args: [treasureRegion.Metadata.Name]);
            return;
        }

        foreach (var location in locations)
        {
            Tracker.LocationTracker.Clear(location, confidence, false, false);
            undoActions.Add(PopUndo().Action);
        }

        Tracker.Say(x => x.DungeonCleared, args: [treasureRegion.Metadata.Name]);

        var inaccessibleLocations = locations.Where(x => !x.IsAvailable(progress)).ToList();
        if (inaccessibleLocations.Count > 0 && confidence >= Options.MinimumSassConfidence)
        {
            var anyMissedLocation = inaccessibleLocations.Random(Random) ?? inaccessibleLocations.First();
            var locationInfo = anyMissedLocation.Metadata;
            var missingItemCombinations = Logic.GetMissingRequiredItems(anyMissedLocation, progress, out _).ToList();
            if (missingItemCombinations.Any())
            {
                var missingItems = (missingItemCombinations.Random(Random) ?? missingItemCombinations.First())
                    .Select(itemService.FirstOrDefault)
                    .NonNull();
                var missingItemsText = NaturalLanguage.Join(missingItems, World.Config);
                Tracker.Say(x => x.DungeonClearedWithInaccessibleItems, args: [treasureRegion.Metadata.Name, locationInfo.Name, missingItemsText]);
            }
            else
            {
                Tracker.Say(x => x.DungeonClearedWithTooManyInaccessibleItems, args: [treasureRegion.Metadata.Name, locationInfo.Name]);
            }
        }
        itemService.ResetProgression();

        AddUndo(() =>
        {
            treasureRegion.RemainingTreasure = remaining;
            foreach (var undo in undoActions)
            {
                undo();
            }
            itemService.ResetProgression();
        });
    }
}
