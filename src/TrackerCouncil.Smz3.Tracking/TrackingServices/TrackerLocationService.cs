using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

public class TrackerLocationService(ILogger<TrackerTreasureService> logger, ItemService itemService, IMetadataService metadataService) : TrackerService, ITrackerLocationService
{
    private IEnumerable<ItemType>? _previousMissingItems;

    /// <summary>
    /// Clears an item from the specified location.
    /// </summary>
    /// <param name="location">The location to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    /// <param name="stateResponse"></param>
    /// <param name="allowLocationComments"></param>
    public void Clear(Location location, float? confidence = null, bool autoTracked = false, bool stateResponse = true, bool allowLocationComments = false)
    {
        itemService.ResetProgression();

        if (!stateResponse && allowLocationComments)
        {
            GiveLocationComment(location.Item.Type, location, isTracking: true, confidence, location.Item.Metadata);
            GivePreConfiguredLocationSass(location);
        }

        if (confidence != null && !stateResponse)
        {
            // Only use TTS if called from a voice command
            var locationName = location.Metadata.Name;
            Tracker.Say(response: Responses.LocationCleared, args: [locationName]);
        }

        ItemType? prevMarkedItem = null;
        if (location.State.HasMarkedItem)
        {
            prevMarkedItem = location.State.MarkedItem;
            location.State.MarkedItem = null;
        }

        var undoTrackTreasure = Tracker.TreasureTracker.TryTrackDungeonTreasure(location, confidence, stateResponse: stateResponse);

        // Important: clear only after tracking dungeon treasure, as
        // the "guess dungeon from location" algorithm excludes
        // cleared items
        location.State.Cleared = true;
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
            var items = itemService.GetProgression(!isKeysanityForLocation);

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
                            var missingItemNames = NaturalLanguage.Join(missingItems.Select(itemService.GetName));
                            Tracker.Say(x => x.TrackedOutOfLogicItem, args: [item.Metadata.Name, locationInfo.Name, missingItemNames]);
                        }
                    }

                    _previousMissingItems = allPossibleMissingItems;
                }

            }
        }

        IsDirty = true;

        if (!autoTracked)
        {
            AddUndo(() =>
            {
                location.State.Cleared = false;
                location.State.MarkedItem = prevMarkedItem;
                undoTrackTreasure?.Invoke();
                undoStopPegWorldMode?.Invoke();
                itemService.ResetProgression();
            });
        }
    }

    /// <summary>
    /// Clears an item from the specified locations.
    /// </summary>
    /// <param name="locations">The locations to clear.</param>
    /// <param name="confidence">The speech recognition confidence</param>
    public void Clear(List<Location> locations, float? confidence = null)
    {
        if (locations.Count == 1)
        {
            Clear(locations.First(), confidence);
            return;
        }

        itemService.ResetProgression();

        var originalClearedValues = new Dictionary<Location, bool>();
        var originalMarkedItems = new Dictionary<Location, ItemType>();

        // Clear the locations
        foreach (var location in locations)
        {
            originalClearedValues.Add(location, location.State.Cleared);
            if (location.State.MarkedItem != null)
            {
                originalMarkedItems.Add(location, location.State.MarkedItem.Value);
            }
            location.State.Cleared = true;
            location.State.MarkedItem = null;
        }

        Action? undoDungeonTreasure = null;
        if (locations.Select(x => x.Region).Distinct().Count() == 1)
        {
            Tracker.Say(x => x.LocationsClearedSameRegion, args: [locations.Count, locations.First().Region.GetName()]);
            if (locations.First().Region is IHasTreasure dungeon)
            {
                var treasureCount = locations.Count(x => x.Item.IsTreasure || World.Config.ZeldaKeysanity);
                if (Tracker.TreasureTracker.TrackDungeonTreasure(dungeon, confidence, treasureCount))
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
                location.State.Cleared = originalClearedValues[location];
                if (originalMarkedItems.TryGetValue(location, out var item))
                {
                    location.State.MarkedItem = item;
                }
            }
            undoDungeonTreasure?.Invoke();
            itemService.ResetProgression();
        });

        World.LastClearedLocation = locations.First();
        IsDirty = true;
    }

    /// <summary>
    /// Marks an item at the specified location.
    /// </summary>
    /// <param name="location">The location to mark.</param>
    /// <param name="item">
    /// The item that is found at <paramref name="location"/>.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked location was auto tracked</param>
    public void MarkLocation(Location location, Item item, float? confidence = null, bool autoTracked = false)
    {
        MarkLocation(location, item.Type, confidence, autoTracked, item.Metadata);
    }

    /// <summary>
    /// Marks an item at the specified location.
    /// </summary>
    /// <param name="location">The location to mark.</param>
    /// <param name="item">
    /// The item that is found at <paramref name="location"/>.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked location was auto tracked</param>
    /// <param name="metadata">The metadata of the item</param>
    public void MarkLocation(Location location, ItemType item, float? confidence = null, bool autoTracked = false, ItemData? metadata = null)
    {
        var locationName = location.Metadata.Name;

        metadata ??= metadataService.Item(item);

        GiveLocationComment(location.Item.Type, location, isTracking: false, confidence, metadata);

        if (item == ItemType.Nothing)
        {
            Clear(location);
            Tracker.Say(response: Responses.LocationMarkedAsBullshit, args: [locationName]);
        }
        else if (location.State.MarkedItem != null)
        {
            var oldType = location.State.MarkedItem;
            location.State.MarkedItem = item;
            Tracker.Say(x => x.LocationMarkedAgain, args: [locationName, metadata?.Name ?? item.GetDescription(), oldType.GetDescription()]);
            if (!autoTracked)
            {
                AddUndo(() =>
                {
                    location.State.MarkedItem = oldType;
                });
            }
        }
        else
        {
            location.State.MarkedItem = item;
            Tracker.Say(x => x.LocationMarked, args: [locationName, metadata?.Name ?? item.GetDescription()]);
            if (!autoTracked)
            {
                AddUndo(() =>
                {
                    location.State.MarkedItem = null;
                });
            }
        }

        GivePreConfiguredLocationSass(location);

        IsDirty = true;
    }

    /// <summary>
    /// Clears every item in the specified area, optionally tracking the
    /// cleared items.
    /// </summary>
    /// <param name="area">The area whose items to clear.</param>
    /// <param name="trackItems">
    /// <c>true</c> to track any items found; <c>false</c> to only clear the
    /// affected locations.
    /// </param>
    /// <param name="includeUnavailable">
    /// <c>true</c> to include every item in <paramref name="area"/>, even
    /// those that are not in logic. <c>false</c> to only include chests
    /// available with current items.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="assumeKeys">
    /// Set to true to ignore keys when clearing the location.
    /// </param>
    public void ClearArea(IHasLocations area, bool trackItems, bool includeUnavailable = false, float? confidence = null, bool assumeKeys = false)
    {
        var locations = area.Locations
            .Where(x => x.State.Cleared == false)
            .WhereUnless(includeUnavailable, x => x.IsAvailable(itemService.GetProgression(area)))
            .ToImmutableList();

        itemService.ResetProgression();

        if (locations.Count == 0)
        {
            var outOfLogicLocations = area.Locations
                .Count(x => x.State.Cleared == false);

            if (outOfLogicLocations > 0)
                Tracker.Say(responses: Responses.TrackedNothingOutOfLogic, tieredKey: outOfLogicLocations, args: [area.Name, outOfLogicLocations]);
            else
                Tracker.Say(response: Responses.TrackedNothing, args: [area.Name]);
        }
        else
        {
            // If there is only one (available) item here, just call the
            // regular TrackItem instead
            var onlyLocation = locations.TrySingle();
            if (onlyLocation != null)
            {
                if (!trackItems)
                {
                    Tracker.LocationTracker.Clear(onlyLocation, confidence);
                }
                else
                {
                    var item = onlyLocation.Item;
                    Tracker.ItemTracker.TrackItem(item: item, trackedAs: null, confidence: confidence, tryClear: true, autoTracked: false, location: onlyLocation);
                }
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
                        location.State.Cleared = true;
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

                    location.State.Cleared = true;
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
                        var progression = itemService.GetProgression(area);
                        var someOutOfLogicLocation = locations.Where(x => !x.IsAvailable(progression)).Random(Random);
                        if (someOutOfLogicLocation != null && confidence >= Options.MinimumSassConfidence)
                        {
                            var someOutOfLogicItem = someOutOfLogicLocation.Item;
                            var missingItems = Logic.GetMissingRequiredItems(someOutOfLogicLocation, progression, out _).MinBy(x => x.Length);
                            if (missingItems != null)
                            {
                                var missingItemNames = NaturalLanguage.Join(missingItems.Select(itemService.GetName));
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

        IsDirty = true;

        AddUndo(() =>
        {
            foreach (var location in locations)
            {
                if (trackItems)
                {
                    var item = location.Item;
                    if (item.Type != ItemType.Nothing && item.State.TrackingState > 0)
                        item.State.TrackingState--;
                }

                location.State.Cleared = false;
            }
            itemService.ResetProgression();
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
        else if (location.Item.Type != ItemType.Nothing && !item.IsEquivalentTo(location.Item.Type) && (item != ItemType.Nothing || location.Item.Metadata.IsProgression(World.Config)))
        {
            if (confidence == null || confidence < Options.MinimumSassConfidence)
                return;

            var actualItemName = itemService.GetName(location.Item.Type);
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

    /// <summary>
    /// Makes Tracker respond to a location if it was pre-configured by the user.
    /// </summary>
    /// <param name="location">The location at which an item was marked or tracked.</param>
    /// <param name="marking"><see langword="true"/> if marking, <see langword="false"/> if tracking.</param>
    private void GivePreConfiguredLocationSass(Location location, bool marking = false)
    {
        // "What a surprise."
        if (LocalConfig != null && LocalConfig.LocationItems.ContainsKey(location.Id))
        {
            // I guess we're not storing the other options? We could respond to those, too, if we had those here.
            Tracker.Say(x => marking ? x.LocationMarkedPreConfigured : x.TrackedPreConfigured, args: [location.Metadata.Name]);
        }
    }
}
