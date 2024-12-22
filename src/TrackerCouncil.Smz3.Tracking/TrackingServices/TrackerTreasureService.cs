using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerTreasureService(ILogger<TrackerTreasureService> logger, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService) : TrackerService, ITrackerTreasureService
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

        List<Action> undoActions = [];

        if (!autoTracked)
        {
            region.HasManuallyClearedTreasure = true;
        }

        if (region.RemainingTreasure > 0)
        {
            region.RemainingTreasure -= amount;

            // If there are no more treasures and the boss is defeated, clear all locations in the dungeon
            if (region.RemainingTreasure == 0 && region is IHasBoss { BossDefeated: true })
            {
                foreach (var location in ((Region)region).Locations.Where(x => !x.Cleared))
                {
                    Tracker.LocationTracker.Clear(location, confidence, autoTracked, false, false, false);
                    undoActions.Add(PopUndo().Action);
                }
            }

            // Always add a response if there's treasure left, even when
            // clearing a dungeon (because that means it was out of logic
            // and could be relevant)
            if (stateResponse && (confidence != null || region.RemainingTreasure >= 0 || autoTracked))
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

            AddUndo(autoTracked, () =>
            {
                region.RemainingTreasure += amount;
                foreach (var action in undoActions)
                {
                    action.Invoke();
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

    public bool UntrackDungeonTreasure(IHasTreasure region, int amount = 1)
    {
        if (amount < 1)
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount of items must be greater than zero.");

        if (region.RemainingTreasure == region.TotalTreasure)
        {
            logger.LogWarning("Trying to untrack {Amount} treasures in a dungeon where no treasures have been removed.", amount);
            return false;
        }
        else if (region.RemainingTreasure + amount > region.TotalTreasure)
        {
            amount = region.TotalTreasure - region.RemainingTreasure;
        }

        IsDirty = true;

        region.RemainingTreasure += amount;
        AddUndo(() =>
        {
            region.RemainingTreasure -= amount;
        });

        return true;
    }

    public Action? TryTrackDungeonTreasure(Location location, float? confidence, bool autoTracked = false, bool stateResponse = true)
    {
        if (!autoTracked && confidence < Options.MinimumSassConfidence || location.Type == LocationType.NotInDungeon)
        {
            // Tracker response could give away the location of an item if
            // it is in a dungeon but tracker misheard.
            return null;
        }

        var dungeon = location.GetTreasureRegion();
        if (dungeon != null && (location.Item.IsTreasure || World.Config.ZeldaKeysanity))
        {
            if (TrackDungeonTreasure(dungeon, confidence, 1, autoTracked, stateResponse))
            {
                return !autoTracked ? PopUndo().Action : null;
            }
        }

        IsDirty = true;

        return null;
    }

    public Action? TryUntrackDungeonTreasure(Location location)
    {
        var dungeon = location.GetTreasureRegion();

        if (dungeon == null || (!location.Item.IsTreasure && !World.Config.ZeldaKeysanity))
        {
            return null;
        }

        return UntrackDungeonTreasure(dungeon) ? PopUndo().Action : null;
    }

    public void ClearDungeon(IHasTreasure treasureRegion, float? confidence = null)
    {
        var remaining = treasureRegion.RemainingTreasure;
        if (remaining > 0)
        {
            treasureRegion.RemainingTreasure = 0;
        }

        List<Action> undoActions = [];

        var region = (Region)treasureRegion;
        var progress = playerProgressionService.GetProgression(assumeKeys: !World.Config.ZeldaKeysanity);
        var locations = region.Locations.Where(x => !x.Cleared).ToList();

        if (remaining <= 0 && locations.Count <= 0)
        {
            // We didn't do anything
            Tracker.Say(x => x.DungeonAlreadyCleared, args: [treasureRegion.Metadata.Name]);
            return;
        }

        foreach (var location in locations)
        {
            Tracker.LocationTracker.Clear(location, confidence, false, false, false);
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
                    .Select(worldQueryService.FirstOrDefault)
                    .NonNull();
                var missingItemsText = NaturalLanguage.Join(missingItems, World.Config);
                Tracker.Say(x => x.DungeonClearedWithInaccessibleItems, args: [treasureRegion.Metadata.Name, locationInfo.Name, missingItemsText]);
            }
            else
            {
                Tracker.Say(x => x.DungeonClearedWithTooManyInaccessibleItems, args: [treasureRegion.Metadata.Name, locationInfo.Name]);
            }
        }
        playerProgressionService.ResetProgression();

        AddUndo(() =>
        {
            treasureRegion.RemainingTreasure = remaining;
            foreach (var undo in undoActions)
            {
                undo();
            }
            playerProgressionService.ResetProgression();
        });
    }
}
