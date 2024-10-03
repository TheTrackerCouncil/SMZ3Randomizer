using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

public class TrackerItemService(ILogger<TrackerTreasureService> logger, ItemService itemService, ICommunicator communicator, IWorldService worldService) : TrackerService, ITrackerItemService
{
    private List<Item> _pendingSpeechItems = [];

    public override void Initialize()
    {
        communicator.SpeakCompleted += CommunicatorOnSpeakCompleted;
    }

    /// <summary>
    /// Tracks the specifies item.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="tryClear">
    /// <see langword="true"/> to attempt to clear a location for the
    /// tracked item; <see langword="false"/> if that is done by the caller.
    /// </param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    /// <param name="location">The location an item was tracked from</param>
    /// <param name="giftedItem">If the item was gifted to the player by tracker or another player</param>
    /// <param name="silent">If tracker should not say anything</param>
    /// <returns>
    /// <see langword="true"/> if the item was actually tracked; <see
    /// langword="false"/> if the item could not be tracked, e.g. when
    /// tracking Bow twice.
    /// </returns>
    public bool TrackItem(Item item, string? trackedAs = null, float? confidence = null, bool tryClear = true, bool autoTracked = false, Location? location = null, bool giftedItem = false, bool silent = false)
    {
        var didTrack = false;
        var accessibleBefore = worldService.AccessibleLocations(false);
        var itemName = item.Name;
        var originalTrackingState = item.State.TrackingState;
        itemService.ResetProgression();

        var isGTPreBigKey = !World.Config.ZeldaKeysanity
                            && autoTracked
                            && location?.Region.GetType() == typeof(GanonsTower)
                            && !itemService.GetProgression(false).BigKeyGT;
        var stateResponse = !isGTPreBigKey && !silent && (!autoTracked
                                                          || !item.Metadata.IsDungeonItem()
                                                          || World.Config.ZeldaKeysanity);

        if (stateResponse && communicator.IsSpeaking)
        {
            _pendingSpeechItems.Add(item);
            stateResponse = false;
        }

        // Actually track the item if it's for the local player's world
        if (item.World == World)
        {
            if (item.Metadata.HasStages)
            {
                if (trackedAs != null && item.Metadata.GetStage(trackedAs) != null)
                {
                    var stage = item.Metadata.GetStage(trackedAs)!;

                    // Tracked by specific stage name (e.g. Tempered Sword), set
                    // to that stage specifically
                    var stageName = item.Metadata.Stages[stage.Value].ToString();

                    didTrack = item.Track(stage.Value);
                    if (stateResponse)
                    {
                        if (didTrack)
                        {
                            if (item.TryGetTrackingResponse(out var response))
                            {
                                Tracker.Say(response: response, args: [item.Counter]);
                            }
                            else
                            {
                                Tracker.Say(response: Responses.TrackedItemByStage, args: [itemName, stageName]);
                            }
                        }
                        else
                        {
                            Tracker.Say(response: Responses.TrackedOlderProgressiveItem, args: [itemName, item.Metadata.Stages[item.State.TrackingState].ToString()]);
                        }
                    }
                }
                else
                {
                    // Tracked by regular name, upgrade by one step
                    didTrack = item.Track();
                    if (stateResponse)
                    {
                        if (didTrack)
                        {
                            if (item.TryGetTrackingResponse(out var response))
                            {
                                Tracker.Say(response: response, args: [item.Counter]);
                            }
                            else
                            {
                                var stageName = item.Metadata.Stages[item.State.TrackingState].ToString();
                                Tracker.Say(response: Responses.TrackedProgressiveItem, args: [itemName, stageName]);
                            }
                        }
                        else
                        {
                            Tracker.Say(response: Responses.TrackedTooManyOfAnItem, args: [itemName]);
                        }
                    }
                }
            }
            else if (item.Metadata.Multiple)
            {
                didTrack = item.Track();
                if (item.TryGetTrackingResponse(out var response))
                {
                    if (stateResponse)
                        Tracker.Say(response: response, args: [item.Counter]);
                }
                else if (item.Counter == 1)
                {
                    if (stateResponse)
                        Tracker.Say(response: Responses.TrackedItem, args: [itemName, item.Metadata.NameWithArticle]);
                }
                else if (item.Counter > 1)
                {
                    if (stateResponse)
                        Tracker.Say(response: Responses.TrackedItemMultiple, args: [item.Metadata.Plural ?? $"{itemName}s", item.Counter, item.Name]);
                }
                else
                {
                    logger.LogWarning("Encountered multiple item with counter 0: {Item} has counter {Counter}", item, item.Counter);
                    if (stateResponse)
                        Tracker.Say(response: Responses.TrackedItem, args: [itemName, item.Metadata.NameWithArticle]);
                }
            }
            else
            {
                didTrack = item.Track();
                if (stateResponse)
                {
                    if (didTrack)
                    {
                        if (item.TryGetTrackingResponse(out var response))
                        {
                            Tracker.Say(response: response, args: [item.Counter]);
                        }
                        else
                        {
                            Tracker.Say(response: Responses.TrackedItem, args: [itemName, item.Metadata.NameWithArticle]);
                        }
                    }
                    else
                    {
                        Tracker.Say(response: Responses.TrackedAlreadyTrackedItem, args: [itemName]);
                    }
                }
            }
        }

        var undoTrack = () =>
        {
            item.State.TrackingState = originalTrackingState; itemService.ResetProgression();
        };

        // Check if we can clear a location
        Action? undoClear = null;
        Action? undoTrackDungeonTreasure = null;

        // If this was not gifted to the player, try to clear the location
        if (!giftedItem && item.Type != ItemType.Nothing)
        {
            if (location == null && !World.Config.MultiWorld)
            {
                location = worldService.Locations(outOfLogic: true, itemFilter: item.Type).TrySingle();
            }

            // Clear the location if it's for the local player's world
            if (location != null && location.World == World && !location.Cleared)
            {
                Tracker.LocationTracker.Clear(location, confidence, autoTracked, !stateResponse, true);
                undoClear = PopUndo().Action;
            }
        }

        var addedEvent = History.AddEvent(
            HistoryEventType.TrackedItem,
            item.Metadata.IsProgression(World.Config),
            item.Metadata.NameWithArticle,
            location
        );

        IsDirty = true;

        if (!autoTracked)
        {
            AddUndo(() =>
            {
                undoTrack();
                undoClear?.Invoke();
                undoTrackDungeonTreasure?.Invoke();
                itemService.ResetProgression();
                addedEvent.IsUndone = true;
            });
        }

        GiveLocationHint(accessibleBefore);
        RestartIdleTimers();

        return didTrack;
    }

    /// <summary>
    /// Tracks the specifies item and clears it from the specified dungeon.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="hasTreasure">The dungeon the item was tracked in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void TrackItemFrom(Item item, IHasTreasure hasTreasure, string? trackedAs = null, float? confidence = null)
    {
        var tracked = TrackItem(item, trackedAs, confidence, tryClear: false);
        var undoTrack = PopUndo();
        itemService.ResetProgression();

        // Check if we can remove something from the remaining treasures in
        // a dungeon
        Action? undoTrackTreasure = null;
        if (tracked) // Only track treasure if we actually tracked anything
        {
            hasTreasure = GetDungeonFromItem(item, hasTreasure)!;
            if (Tracker.TreasureTracker.TrackDungeonTreasure(hasTreasure, confidence))
                undoTrackTreasure = PopUndo().Action;
        }

        IsDirty = true;

        // Check if we can remove something from the marked location
        var location = worldService.Locations(itemFilter: item.Type, inRegion: hasTreasure as Region).TrySingle();
        if (location != null)
        {
            location.State.Cleared = true;
            World.LastClearedLocation = location;

            if (location.State.HasMarkedItem)
            {
                location.State.MarkedItem = null;
            }

            AddUndo(() =>
            {
                undoTrack.Action();
                undoTrackTreasure?.Invoke();
                location.State.Cleared = false;
                itemService.ResetProgression();
            });
        }
        else
        {
            AddUndo(() =>
            {
                undoTrack.Action();
                undoTrackTreasure?.Invoke();
                itemService.ResetProgression();
            });
        }
    }

    /// <summary>
    /// Tracks the specified item and clears it from the specified room.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="area">The area the item was found in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void TrackItemFrom(Item item, IHasLocations area, string? trackedAs = null, float? confidence = null)
    {
        var locations = area.Locations
            .Where(x => x.Item.Type == item.Type)
            .ToImmutableList();
        itemService.ResetProgression();

        if (locations.Count == 0)
        {
            Tracker.Say(response: Responses.AreaDoesNotHaveItem, args: [item.Name, area.Name, item.Metadata.NameWithArticle]);
        }
        else if (locations.Count > 1)
        {
            // Consider tracking/clearing everything?
            Tracker.Say(response: Responses.AreaHasMoreThanOneItem, args: [item.Name, area.Name, item.Metadata.NameWithArticle]);
        }

        IsDirty = true;

        TrackItem(item, trackedAs, confidence, tryClear: false);
        if (locations.Count == 1)
        {
            Tracker.LocationTracker.Clear(locations.Single());
            var undoClear = PopUndo();
            var undoTrack = PopUndo();
            AddUndo(() =>
            {
                undoClear.Action();
                undoTrack.Action();
                itemService.ResetProgression();
            });
        }
    }

    /// <summary>
    /// Sets the item count for the specified item.
    /// </summary>
    /// <param name="item">The item to track.</param>
    /// <param name="count">
    /// The amount of the item that is in the player's inventory now.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void TrackItemAmount(Item item, int count, float confidence)
    {
        itemService.ResetProgression();

        var newItemCount = count;
        if (item.Metadata.CounterMultiplier > 1
            && count % item.Metadata.CounterMultiplier == 0)
        {
            newItemCount = count / item.Metadata.CounterMultiplier.Value;
        }

        var oldItemCount = item.State.TrackingState;
        if (newItemCount == oldItemCount)
        {
            Tracker.Say(response: Responses.TrackedExactAmountDuplicate, args: [item.Metadata.Plural, count]);
            return;
        }

        item.State.TrackingState = newItemCount;
        if (item.TryGetTrackingResponse(out var response))
        {
            Tracker.Say(response: response, args: [item.Counter]);
        }
        else if (newItemCount > oldItemCount)
        {
            Tracker.Say(text: Responses.TrackedItemMultiple?.Format(item.Metadata.Plural ?? $"{item.Name}s", item.Counter, item.Name));
        }
        else
        {
            Tracker.Say(text: Responses.UntrackedItemMultiple?.Format(item.Metadata.Plural ?? $"{item.Name}s", item.Metadata.Plural ?? $"{item.Name}s"));
        }

        IsDirty = true;

        AddUndo(() =>
        {
            item.State.TrackingState = oldItemCount; itemService.ResetProgression();
        });
    }

    public void TrackItems(List<Item> items, bool autoTracked, bool giftedItem)
    {
        if (items.Count == 1)
        {
            TrackItem(items.First(), null, null, false, autoTracked, null, giftedItem);
            return;
        }

        itemService.ResetProgression();

        foreach (var item in items)
        {
            item.Track();
        }

        AnnounceTrackedItems(items);

        IsDirty = true;
        RestartIdleTimers();
    }

    /// <summary>
    /// Removes an item from the tracker.
    /// </summary>
    /// <param name="item">The item to untrack.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void UntrackItem(Item item, float? confidence = null)
    {
        var originalTrackingState = item.State.TrackingState;
        itemService.ResetProgression();

        if (!item.Untrack())
        {
            Tracker.Say(response: Responses.UntrackedNothing, args: [item.Name, item.Metadata.NameWithArticle]);
            return;
        }

        if (item.Metadata.HasStages)
        {
            Tracker.Say(response: Responses.UntrackedProgressiveItem, args: [item.Name, item.Metadata.NameWithArticle]);
        }
        else if (item.Metadata.Multiple)
        {
            if (item.State.TrackingState > 0)
            {
                if (item.Metadata.CounterMultiplier > 1)
                    Tracker.Say(text: Responses.UntrackedItemMultiple?.Format($"{item.Metadata.CounterMultiplier} {item.Metadata.Plural}", $"{item.Metadata.CounterMultiplier} {item.Metadata.Plural}"));
                else
                    Tracker.Say(response: Responses.UntrackedItemMultiple, args: [item.Name, item.Metadata.NameWithArticle]);
            }
            else
                Tracker.Say(response: Responses.UntrackedItemMultipleLast, args: [item.Name, item.Metadata.NameWithArticle]);
        }
        else
        {
            Tracker.Say(response: Responses.UntrackedItem, args: [item.Name, item.Metadata.NameWithArticle]);
        }

        IsDirty = true;
        AddUndo(() =>
        {
            item.State.TrackingState = originalTrackingState;
            itemService.ResetProgression();
        });
    }

    private void CommunicatorOnSpeakCompleted(object? sender, SpeakCompletedEventArgs e)
    {
        if (_pendingSpeechItems.Count > 0)
        {
            AnnounceTrackedItems(_pendingSpeechItems);
            _pendingSpeechItems.Clear();
        }

        if (e.SpeechDuration.TotalSeconds > 60)
        {
            Tracker.Say(x => x.LongSpeechResponse);
        }
    }

    private IHasTreasure? GetDungeonFromItem(Item item, IHasTreasure? dungeon = null)
    {
        var locations = worldService.Locations(itemFilter: item.Type)
            .Where(x => x.Type != LocationType.NotInDungeon)
            .ToImmutableList();

        if (locations.Count == 1 && dungeon == null)
        {
            // User didn't have a guess and there's only one location that
            // has the tracker item
            return locations[0].GetTreasureRegion();
        }

        if (locations.Count > 0 && dungeon != null)
        {
            // Does the dungeon even have that item?
            if (locations.All(x => dungeon != x.Region))
            {
                // Be a smart-ass about it
                Tracker.Say(response: Responses.ItemTrackedInIncorrectDungeon, args: [dungeon.Metadata.Name, item.Metadata.NameWithArticle]);
            }
        }

        // - If tracker was started before generating a seed, we don't know
        // better.
        // - If we do know better, we should still go with the user's
        // choice.
        // - If there are multiple copies of the item, we don't know which
        // was tracked. Either way, we have to assume `dungeon` is correct.
        // If it's `null`, nobody knows.
        return dungeon;
    }

    private void AnnounceTrackedItems(List<Item> items)
    {
        if (items.Count == 1)
        {
            var item = items[0];
            if (item.TryGetTrackingResponse(out var response))
            {
                Tracker.Say(text: response.Format(item.Counter));
            }
            else
            {
                Tracker.Say(text: Responses.TrackedItem?.Format(item.Name, item.Metadata.NameWithArticle));
            }
        }
        else if (items.Count == 2)
        {
            Tracker.Say(x => x.TrackedTwoItems, args: [items[0].Metadata.NameWithArticle, items[1].Metadata.NameWithArticle]);
        }
        else if (items.Count == 3)
        {
            Tracker.Say(x => x.TrackedThreeItems, args: [items[0].Metadata.NameWithArticle, items[1].Metadata.NameWithArticle, items[2].Metadata.NameWithArticle]);
        }
        else
        {
            var itemsToSay = items.Where(x => x.Type.IsPossibleProgression(World.Config.ZeldaKeysanity, World.Config.MetroidKeysanity)).Take(2).ToList();
            if (itemsToSay.Count() < 2)
            {
                var numToTake = 2 - itemsToSay.Count();
                itemsToSay.AddRange(items.Where(x => !x.Type.IsPossibleProgression(World.Config.ZeldaKeysanity, World.Config.MetroidKeysanity)).Take(numToTake));
            }

            Tracker.Say(x => x.TrackedManyItems, args: [itemsToSay[0].Metadata.NameWithArticle, itemsToSay[1].Metadata.NameWithArticle, items.Count - 2]);
        }
    }

    private void GiveLocationHint(IEnumerable<Location> accessibleBefore)
    {
        var accessibleAfter = worldService.AccessibleLocations(false);
        var newlyAccessible = accessibleAfter.Except(accessibleBefore).ToList();
        if (newlyAccessible.Any())
        {
            if (newlyAccessible.Contains(World.FindLocation(LocationId.InnerMaridiaSpringBall)))
                Tracker.Say(response: Responses.ShaktoolAvailable);

            if (newlyAccessible.Contains(World.DarkWorldNorthWest.PegWorld))
                Tracker.Say(response: Responses.PegWorldAvailable);
        }
        else if (Responses.TrackedUselessItem != null)
        {
            Tracker.Say(response: Responses.TrackedUselessItem);
        }
    }
}
