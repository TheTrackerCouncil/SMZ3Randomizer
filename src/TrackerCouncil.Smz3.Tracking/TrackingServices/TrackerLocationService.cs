using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerLocationService(ILogger<TrackerTreasureService> logger, IPlayerProgressionService playerProgressionService, IMetadataService metadataService, IWorldQueryService worldQueryService) : TrackerService, ITrackerLocationService
{
    private IEnumerable<ItemType>? _previousMissingItems;

    public event EventHandler<LocationClearedEventArgs>? LocationCleared;
    public event EventHandler<LocationClearedEventArgs>? LocationMarked;

    public void Clear(Location location, float? confidence = null, bool autoTracked = false, bool stateResponse = true, bool allowLocationComments = false, bool updateTreasureCount = true)
    {
        if (!stateResponse && allowLocationComments)
        {
            GiveLocationComment(location.Item.Type, location, isTracking: true, confidence, location.Item.Metadata);
            GivePreConfiguredLocationSass(location);
        }

        if (confidence != null && stateResponse)
        {
            // Only use TTS if called from a voice command
            var locationName = location.Metadata.Name;
            Tracker.Say(response: Responses.LocationCleared, args: [locationName]);
        }

        ItemType? prevMarkedItem = null;
        if (location.HasMarkedItem)
        {
            prevMarkedItem = location.MarkedItem;
            location.MarkedItem = null;
            LocationMarked?.Invoke(this, new LocationClearedEventArgs(location, confidence, autoTracked));
        }

        var undoTrackTreasure = updateTreasureCount
            ? Tracker.TreasureTracker.TryTrackDungeonTreasure(location, confidence, stateResponse: stateResponse)
            : null;

        // Important: clear only after tracking dungeon treasure, as
        // the "guess dungeon from location" algorithm excludes
        // cleared items
        location.Cleared = true;
        location.SetAccessibility(Accessibility.Cleared);
        World.LastClearedLocation = location;

        Action? undoStopPegWorldMode = null;
        if (location == World.DarkWorldNorthWest.PegWorld)
        {
            Tracker.ModeTracker.StopPegWorldMode();

            if (!autoTracked)
            {
                undoStopPegWorldMode = PopUndo().Action;
            }
        }

        // Comment on if the location is out of logic
        if (allowLocationComments && stateResponse)
        {
            var item = location.Item;
            var isKeysanityForLocation = (location.Region is Z3Region && World.Config.ZeldaKeysanity) || (location.Region is SMRegion && World.Config.MetroidKeysanity);
            var items = playerProgressionService.GetProgression(!isKeysanityForLocation);

            if (!location.IsAvailable(items) && (confidence >= Options.MinimumSassConfidence || autoTracked))
            {
                var locationInfo = location.Metadata;
                var roomInfo = location.Room?.Metadata;
                var regionInfo = location.Region.Metadata;

                if (locationInfo.OutOfLogic != null)
                {
                    Tracker.Say(response: locationInfo.OutOfLogic);
                }
                else if (roomInfo?.OutOfLogic != null)
                {
                    Tracker.Say(response: roomInfo.OutOfLogic);
                }
                else if (regionInfo.OutOfLogic != null)
                {
                    Tracker.Say(response: regionInfo.OutOfLogic);
                }
                else
                {
                    var allMissingCombinations = Logic.GetMissingRequiredItems(location, items, out var allMissingItems);
                    allMissingItems = allMissingItems.OrderBy(x => x);

                    var missingItems = allMissingCombinations.MinBy(x => x.Length);
                    var allPossibleMissingItems = allMissingItems.ToList();
                    if (missingItems == null)
                    {
                        Tracker.Say(x => x.TrackedOutOfLogicItemTooManyMissing, args: [item.Metadata.Name, locationInfo.Name]);
                    }
                    // Do not say anything if the only thing missing are keys
                    else
                    {
                        var itemsChanged = _previousMissingItems == null || !allPossibleMissingItems.SequenceEqual(_previousMissingItems);
                        var onlyKeys = allPossibleMissingItems.All(x => x.IsInAnyCategory(ItemCategory.BigKey, ItemCategory.SmallKey, ItemCategory.Keycard));
                        _previousMissingItems = allPossibleMissingItems;

                        if (itemsChanged && !onlyKeys)
                        {
                            var missingItemNames = NaturalLanguage.Join(missingItems.Select(metadataService.GetName));
                            Tracker.Say(x => x.TrackedOutOfLogicItem, args: [item.Metadata.Name, locationInfo.Name, missingItemNames]);
                        }
                    }

                    _previousMissingItems = allPossibleMissingItems;
                }

            }
        }

        LocationCleared?.Invoke(this, new LocationClearedEventArgs(location, confidence, autoTracked));

        IsDirty = true;

        AddUndo(autoTracked, () =>
        {
            location.Cleared = false;

            if (prevMarkedItem != null)
            {
                location.MarkedItem = prevMarkedItem;
                LocationMarked?.Invoke(this, new LocationClearedEventArgs(location, null, false));
            }

            UpdateAccessibility(location);
            undoTrackTreasure?.Invoke();
            undoStopPegWorldMode?.Invoke();
            LocationCleared?.Invoke(this, new LocationClearedEventArgs(location, null, false));
        });
    }

    public void Unclear(Location location, bool updateTreasureCount = true)
    {
        var undoTrackTreasure = updateTreasureCount
            ? Tracker.TreasureTracker.TryUntrackDungeonTreasure(location)
            : null;

        // Important: clear only after tracking dungeon treasure, as
        // the "guess dungeon from location" algorithm excludes
        // cleared items
        location.Cleared = false;
        UpdateAccessibility(location);

        LocationCleared?.Invoke(this, new LocationClearedEventArgs(location, null, false));

        IsDirty = true;

        AddUndo(() =>
        {
            location.Cleared = true;
            location.SetAccessibility(Accessibility.Cleared);
            undoTrackTreasure?.Invoke();
        });
    }

    public void Clear(List<Location> locations, float? confidence = null)
    {
        if (locations.Count == 1)
        {
            Clear(locations.First(), confidence);
            return;
        }

        var originalClearedValues = new Dictionary<Location, bool>();
        var originalMarkedItems = new Dictionary<Location, ItemType>();

        // Clear the locations
        foreach (var location in locations)
        {
            originalClearedValues.Add(location, location.Cleared);
            if (location.MarkedItem != null)
            {
                originalMarkedItems.Add(location, location.MarkedItem.Value);
                location.MarkedItem = null;
                LocationMarked?.Invoke(this, new LocationClearedEventArgs(location, confidence, false));
            }

            location.Cleared = true;
            location.SetAccessibility(Accessibility.Cleared);
            LocationCleared?.Invoke(this, new LocationClearedEventArgs(location, confidence, false));
        }

        Action? undoDungeonTreasure = null;
        var allSameRegion = false;
        if (locations.Select(x => x.Region).Distinct().Count() == 1)
        {
            allSameRegion = true;
            Tracker.Say(x => x.LocationsClearedSameRegion, args: [locations.Count, locations.First().Region.GetName()]);
            if (locations.First().Region is IHasTreasure dungeon)
            {
                var treasureCount = locations.Count(x => x.Item.IsTreasure || World.Config.ZeldaKeysanity);
                if (treasureCount > 0 && Tracker.TreasureTracker.TrackDungeonTreasure(dungeon, confidence, treasureCount))
                    undoDungeonTreasure = PopUndo().Action;
            }
        }
        else
        {
            Tracker.Say(x => x.LocationsCleared, args: [locations.Count]);
        }

        AddUndo(() =>
        {
            foreach (var location in locations)
            {
                location.Cleared = originalClearedValues[location];
                if (originalMarkedItems.TryGetValue(location, out var item))
                {
                    location.MarkedItem = item;
                    LocationMarked?.Invoke(this, new LocationClearedEventArgs(location, null, false));
                }
                LocationCleared?.Invoke(this, new LocationClearedEventArgs(location, null, false));
            }

            UpdateAccessibility(locations, allSameRegion ? playerProgressionService.GetProgression(locations[0].Region) : null);
            undoDungeonTreasure?.Invoke();
        });

        World.LastClearedLocation = locations.First();
        IsDirty = true;
    }

    public void MarkLocation(Location location, Item item, float? confidence = null, bool autoTracked = false)
    {
        MarkLocation(location, item.Type, confidence, autoTracked, item.Metadata);
    }

    public void MarkLocation(Location location, ItemType item, float? confidence = null, bool autoTracked = false, ItemData? metadata = null)
    {
        var locationName = location.Metadata.Name;

        metadata ??= metadataService.Item(item);

        if (location.Cleared)
        {
            Tracker.Say(response: Responses.LocationMarkedButAlreadyCleared, args: [locationName, metadata?.Name ?? item.GetDescription() ]);
            return;
        }

        GiveLocationComment(location.Item.Type, location, isTracking: false, confidence, metadata);

        if (item == ItemType.Nothing)
        {
            Clear(location);
            Tracker.Say(response: Responses.LocationMarkedAsBullshit, args: [locationName]);
        }
        else if (location.MarkedItem != null)
        {
            var oldType = location.MarkedItem;
            location.MarkedItem = item;
            Tracker.Say(x => x.LocationMarkedAgain, args: [locationName, metadata?.Name ?? item.GetDescription(), oldType.GetDescription()]);
            LocationMarked?.Invoke(this, new LocationClearedEventArgs(location, confidence, autoTracked));
            AddUndo(autoTracked, () =>
            {
                location.MarkedItem = oldType;
                LocationMarked?.Invoke(this, new LocationClearedEventArgs(location, confidence, autoTracked));
            });
        }
        else
        {
            location.MarkedItem = item;
            Tracker.Say(x => x.LocationMarked, args: [locationName, metadata?.Name ?? item.GetDescription()]);
            LocationMarked?.Invoke(this, new LocationClearedEventArgs(location, confidence, autoTracked));
            AddUndo(autoTracked, () =>
            {
                location.MarkedItem = null;
                LocationMarked?.Invoke(this, new LocationClearedEventArgs(location, confidence, autoTracked));
            });
        }

        GivePreConfiguredLocationSass(location);

        IsDirty = true;
    }

    public void ClearArea(IHasLocations area, bool trackItems, bool includeUnavailable = false, float? confidence = null, bool assumeKeys = false)
    {
        var locations = area.Locations
            .Where(x => x.Cleared == false)
            .WhereUnless(includeUnavailable, x => x.IsAvailable(playerProgressionService.GetProgression(area)))
            .ToImmutableList();

        playerProgressionService.ResetProgression();

        if (locations.Count == 0)
        {
            var outOfLogicLocations = area.Locations
                .Count(x => x.Cleared == false);

            if (outOfLogicLocations > 0)
                Tracker.Say(responses: Responses.TrackedNothingOutOfLogic, tieredKey: outOfLogicLocations, args: [area.Name, outOfLogicLocations]);
            else
                Tracker.Say(response: Responses.TrackedNothing, args: [area.Name]);
        }
        else
        {
            // If there is only one location...
            var onlyLocation = locations.TrySingle();
            if (onlyLocation != null)
            {
                // Use the basic clear if we're not tracking items
                if (!trackItems)
                {
                    Tracker.LocationTracker.Clear(onlyLocation, confidence);
                }
                // Use the item tracker if we're tracking an item
                else
                {
                    var item = onlyLocation.Item;
                    Tracker.ItemTracker.TrackItem(item: item, trackedAs: null, confidence: confidence, tryClear: true, autoTracked: false, location: onlyLocation);
                }

                return;
            }
            else
            {
                // Otherwise, start counting
                var itemsCleared = 0;
                var itemsTracked = new List<Item>();
                var treasureTracked = 0;
                foreach (var location in locations)
                {
                    itemsCleared++;
                    if (!trackItems)
                    {
                        if (location.Item.IsTreasure || World.Config.ZeldaKeysanity)
                            treasureTracked++;
                        location.Cleared = true;
                        location.SetAccessibility(Accessibility.Cleared);
                        World.LastClearedLocation = location;
                        continue;
                    }

                    var item = location.Item;
                    if (!item.Track())
                        logger.LogWarning("Failed to track {ItemType} in {Area}.", item.Name, area.Name); // Probably the compass or something, who cares
                    else
                        itemsTracked.Add(item);
                    if (location.Item.IsTreasure || World.Config.ZeldaKeysanity)
                        treasureTracked++;

                    location.Cleared = true;
                    location.SetAccessibility(Accessibility.Cleared);
                }

                if (trackItems)
                {
                    var itemNames = confidence >= Options.MinimumSassConfidence
                        ? NaturalLanguage.Join(itemsTracked, World.Config)
                        : $"{itemsCleared} items";
                    Tracker.Say(x => x.TrackedMultipleItems, args: [itemsCleared, area.Name, itemNames]);

                    var roomInfo = area is Room room ? room.Metadata : null;
                    var regionInfo = area is Region region ? region.Metadata : null;

                    if (roomInfo?.OutOfLogic != null)
                    {
                        Tracker.Say(response: roomInfo.OutOfLogic);
                    }
                    else if (regionInfo?.OutOfLogic != null)
                    {
                        Tracker.Say(response: regionInfo.OutOfLogic);
                    }
                    else
                    {
                        var progression = playerProgressionService.GetProgression(area);
                        var someOutOfLogicLocation = locations.Where(x => !x.IsAvailable(progression)).Random(Random);
                        if (someOutOfLogicLocation != null && confidence >= Options.MinimumSassConfidence)
                        {
                            var someOutOfLogicItem = someOutOfLogicLocation.Item;
                            var missingItems = Logic.GetMissingRequiredItems(someOutOfLogicLocation, progression, out _).MinBy(x => x.Length);
                            if (missingItems != null)
                            {
                                var missingItemNames = NaturalLanguage.Join(missingItems.Select(metadataService.GetName));
                                Tracker.Say(x => x.TrackedOutOfLogicItem, args: [someOutOfLogicItem.Metadata.Name, someOutOfLogicLocation.Metadata.Name, missingItemNames]);
                            }
                            else
                            {
                                Tracker.Say(x => x.TrackedOutOfLogicItemTooManyMissing, args: [someOutOfLogicItem.Metadata.Name, someOutOfLogicLocation.Metadata.Name]);
                            }
                        }
                    }
                }
                else
                {
                    Tracker.Say(x => x.ClearedMultipleItems, args: [itemsCleared, area.Name]);
                }

                if (treasureTracked > 0)
                {
                    var dungeon = area.GetTreasureRegion();
                    if (dungeon != null)
                    {
                        Tracker.TreasureTracker.TrackDungeonTreasure(dungeon, amount: treasureTracked);
                    }
                }
            }
        }

        foreach (var location in locations)
        {
            location.SetAccessibility(Accessibility.Cleared);
        }

        IsDirty = true;

        AddUndo(() =>
        {
            foreach (var location in locations)
            {
                if (trackItems)
                {
                    var item = location.Item;
                    if (item.Type != ItemType.Nothing && item.TrackingState > 0)
                        item.TrackingState--;
                }

                location.Cleared = false;
            }

            playerProgressionService.ResetProgression();
            UpdateAccessibility(locations, playerProgressionService.GetProgression(area));
            Tracker.BossTracker.UpdateAccessibility();
            Tracker.RewardTracker.UpdateAccessibility();
        });
    }

    private void GiveLocationComment(ItemType item, Location location, bool isTracking, float? confidence, ItemData? metadata)
    {
        metadata ??= metadataService.Item(item);

        // If the plando config specifies a specific line for this location, say it
        if (World.Config.PlandoConfig?.TrackerLocationLines.ContainsKey(location.ToString()) == true)
        {
            Tracker.Say(text: World.Config.PlandoConfig?.TrackerLocationLines[location.ToString()]);
        }
        // Give some sass if the user tracks or marks the wrong item at a
        // location unless the user is clearing a useless item like missiles
        else if (location.Item.Type != ItemType.Nothing && !item.IsEquivalentTo(location.Item.Type) && (item != ItemType.Nothing || location.Item.Metadata.IsProgression(World.Config, location.Item.IsLocalPlayerItem)))
        {
            if (confidence == null || confidence < Options.MinimumSassConfidence)
                return;

            var actualItemName = metadataService.GetName(location.Item.Type);
            if (HintsEnabled) actualItemName = "another item";

            Tracker.Say(response: Responses.LocationHasDifferentItem, args: [metadata?.NameWithArticle ?? item.GetDescription(), actualItemName]);
        }
        else
        {
            if (item == location.VanillaItem && item != ItemType.Nothing)
            {
                Tracker.Say(x => x.TrackedVanillaItem);
                return;
            }

            var locationInfo = location.Metadata;
            var isJunk = metadata?.IsJunk(World.Config) ?? true;
            if (isJunk)
            {
                if (!isTracking && locationInfo.WhenMarkingJunk?.Count > 0)
                {
                    Tracker.Say(text: locationInfo.WhenMarkingJunk.Random(Random)!);
                }
                else if (locationInfo.WhenTrackingJunk?.Count > 0)
                {
                    Tracker.Say(text: locationInfo.WhenTrackingJunk.Random(Random)!);
                }
            }
            else if (!isJunk)
            {
                if (!isTracking && locationInfo.WhenMarkingProgression?.Count > 0)
                {
                    Tracker.Say(text: locationInfo.WhenMarkingProgression.Random(Random)!);
                }
                else if (locationInfo.WhenTrackingProgression?.Count > 0)
                {
                    Tracker.Say(text: locationInfo.WhenTrackingProgression.Random(Random)!);
                }
            }
        }
    }

    private void GivePreConfiguredLocationSass(Location location, bool marking = false)
    {
        // "What a surprise."
        if (LocalConfig != null && LocalConfig.LocationItems.ContainsKey(location.Id))
        {
            // I guess we're not storing the other options? We could respond to those, too, if we had those here.
            Tracker.Say(x => marking ? x.LocationMarkedPreConfigured : x.TrackedPreConfigured, args: [location.Metadata.Name]);
        }
    }

    public void UpdateAccessibility(bool unclearedOnly = true, Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        actualProgression ??= playerProgressionService.GetProgression(false);
        withKeysProgression ??= playerProgressionService.GetProgression(true);
        UpdateAccessibility(worldQueryService.AllLocations().Where(x => !unclearedOnly || !x.Cleared), actualProgression, withKeysProgression);
    }

    public void UpdateAccessibility(IEnumerable<Location> locations, Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        actualProgression ??= playerProgressionService.GetProgression(false);
        withKeysProgression ??= playerProgressionService.GetProgression(true);
        foreach (var location in locations)
        {
            UpdateAccessibility(location, actualProgression, withKeysProgression);
        }
    }

    public void UpdateAccessibility(Location location, Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        actualProgression ??= playerProgressionService.GetProgression(false);
        withKeysProgression ??= playerProgressionService.GetProgression(true);
        if (location.Region is HyruleCastle)
        {
            withKeysProgression = actualProgression;
        }
        location.UpdateAccessibility(actualProgression, withKeysProgression);
    }
}
