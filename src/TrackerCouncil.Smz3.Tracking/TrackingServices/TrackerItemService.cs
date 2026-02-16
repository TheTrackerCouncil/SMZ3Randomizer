using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary.Responses;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerItemService(ILogger<TrackerTreasureService> logger, IPlayerProgressionService playerProgressionService, ICommunicator communicator, IWorldQueryService worldQueryService) : TrackerService, ITrackerItemService
{
    private readonly List<Item> _pendingSpeechItems = [];
    private readonly ConcurrentDictionary<ItemType, int> _maxItemCounts = [];

    public override void Initialize()
    {
        communicator.SpeakCompleted += CommunicatorOnSpeakCompleted;
    }

    public event EventHandler<ItemTrackedEventArgs>? ItemTracked;

    public bool TrackItem(Item item, string? trackedAs = null, float? confidence = null, bool tryClear = true, bool autoTracked = false, Location? location = null, bool giftedItem = false, bool silent = false, bool force = false)
    {
        if (!autoTracked && Tracker.AutoTracker?.IsConnected == true && !force && item.Type != ItemType.Nothing)
        {
            Tracker.Say(response: Responses.AutoTrackingEnabledSass, args: [$"Hey tracker, would you please track {item.RandomName}"]);
            return false;
        }

        var didTrack = false;
        var accessibleBefore = worldQueryService.AccessibleLocations(false);
        var itemName = item.Name;
        var originalTrackingState = item.TrackingState;

        var isGTPreBigKey = (!World.Config.ZeldaKeysanity || World.Config.PlaceGTBigKeyInGT)
                            && autoTracked
                            && location?.Region.GetType() == typeof(GanonsTower)
                            && !playerProgressionService.GetProgression(false).BigKeyGT;
        var stateResponse = !isGTPreBigKey && !silent && (!autoTracked
                                                          || !item.Metadata.IsDungeonItem()
                                                          || World.Config.ZeldaKeysanity);

        if (stateResponse && communicator.IsSpeaking)
        {
            if (item.IsLocalPlayerItem)
            {
                _pendingSpeechItems.Add(item);
            }

            stateResponse = false;
        }

        // Actually track the item if it's for the local player's world
        if (item.World == World && item.IsLocalPlayerItem)
        {
            if (item.Metadata.HasStages)
            {
                logger.LogInformation("Tracking local player staged item {ItemType}", item.Type.GetDescription());

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
                        else if (!autoTracked)
                        {
                            Tracker.Say(response: Responses.TrackedOlderProgressiveItem, args: [itemName, item.Metadata.Stages[item.TrackingState].ToString()]);
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
                                var stageName = item.Metadata.Stages[item.TrackingState].ToString();
                                Tracker.Say(response: Responses.TrackedProgressiveItem, args: [itemName, stageName]);
                            }
                        }
                        else if (!autoTracked)
                        {
                            Tracker.Say(response: Responses.TrackedTooManyOfAnItem, args: [itemName]);
                        }
                    }
                }
            }
            else if (item.Metadata.Multiple)
            {
                logger.LogInformation("Tracking local player multiple item {ItemType}", item.Type.GetDescription());

                didTrack = item.Track();

                if (stateResponse)
                {
                    if (item.TryGetTrackingResponse(out var response))
                    {
                        Tracker.Say(response: response, args: [item.Counter]);
                    }
                    else if (item.Counter == 1)
                    {
                        Tracker.Say(response: Responses.TrackedItem, args: [itemName, item.Metadata.NameWithArticle]);
                    }
                    else if (item.Counter > 1)
                    {
                        Tracker.Say(response: Responses.TrackedItemMultiple, args: [item.Metadata.Plural ?? $"{itemName}s", item.Counter, item.Name]);
                    }
                    else if (!autoTracked)
                    {
                        logger.LogWarning("Encountered multiple item with counter 0: {Item} has counter {Counter}", item, item.Counter);
                        Tracker.Say(response: Responses.TrackedAlreadyTrackedItem, args: [itemName]);
                    }
                }

            }
            else
            {
                logger.LogInformation("Tracking local player basic item {ItemType}", item.Type.GetDescription());

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
                    else if (!autoTracked)
                    {
                        Tracker.Say(response: Responses.TrackedAlreadyTrackedItem, args: [itemName]);
                    }
                }
            }
        }
        else if (location != null)
        {
            logger.LogInformation("Clearing location for item {ItemType}", item.Type.GetDescription());

            Tracker.LocationTracker.Clear(location, confidence, autoTracked, stateResponse: false, allowLocationComments: true, updateTreasureCount: true, stateTreasureResponse: true);

            // If this is a parsed AP/Mainline rom and this is an item for another player, let's comment on it
            if (stateResponse && item is { IsLocalPlayerItem: false } && World.Config.RomGenerator != RomGenerator.Cas)
            {
                if (item.Type == ItemType.OtherGameProgressionItem)
                {
                    Tracker.Say(response: Responses.TrackedParsedOtherGameUnknownProgressionItem,
                        args: [item.PlayerName]);
                }
                else if (item.Type == ItemType.OtherGameItem)
                {
                    Tracker.Say(response: Responses.TrackedParsedOtherGameUnknownRegularItem,
                        args: [item.PlayerName]);
                }
                else
                {
                    Tracker.Say(response: Responses.TrackedParsedOtherGameKnownItem, args: [item.Metadata.Name,
                        item.Metadata.NameWithArticle, item.PlayerName]);
                }
            }

            return true;
        }

        if (!didTrack)
        {
            // If we couldn't track the item for some reason (e.g. an AP seed where you had starting items), but this
            // is an uncleared auto tracked location, we should still clear it
            if (location != null && autoTracked && !location.Cleared)
            {
                Tracker.LocationTracker.Clear(location, confidence, autoTracked, stateResponse: false, allowLocationComments: true, updateTreasureCount: true);
            }

            return false;
        }

        ItemTracked?.Invoke(this, new ItemTrackedEventArgs(item, trackedAs, confidence, autoTracked));

        List<Action?> undoActions =
        [
            () => {
                item.TrackingState = originalTrackingState;
                ItemTracked?.Invoke(this, new ItemTrackedEventArgs(item, trackedAs, null, false));
                playerProgressionService.ResetProgression();
            }
        ];

        // If this was not gifted to the player, try to clear the location
        if (!giftedItem && item.Type != ItemType.Nothing && tryClear)
        {
            if (location == null && World.Config.GameMode != GameMode.Multiworld)
            {
                location = worldQueryService.Locations(outOfLogic: true, itemFilter: item.Type).TrySingle();
            }

            // Clear the location if it's for the local player's world
            if (location != null && location.World == World && !location.Cleared)
            {
                Tracker.LocationTracker.Clear(location, confidence, autoTracked, !silent && !isGTPreBigKey, true);
                undoActions.Add(autoTracked ? null : PopUndo().Action);
            }
        }

        var addedEvent = History.AddEvent(
            HistoryEventType.TrackedItem,
            item.Metadata.IsPossibleProgression(World.Config, item.IsLocalPlayerItem),
            item.Metadata.NameWithArticle,
            location
        );

        IsDirty = true;
        UpdateAllAccessibility(false, item);
        GiveLocationHint(accessibleBefore);
        RestartIdleTimers();

        AddUndo(autoTracked, () =>
        {
            foreach (var undo in undoActions.NonNull())
            {
                undo.Invoke();
            }
            UpdateAllAccessibility(true, item);
            addedEvent.IsUndone = true;
        });

        return true;
    }

    public void TrackItemFrom(Item item, IHasTreasure hasTreasure, string? trackedAs = null, float? confidence = null, bool force = false)
    {
        if (Tracker.AutoTracker?.IsConnected == true && !force && item.Type != ItemType.Nothing)
        {
            Tracker.Say(response: Responses.AutoTrackingEnabledSass, args: [$"Hey tracker, would you please track {item.RandomName} from {hasTreasure.RandomRegionName}"]);
            return;
        }

        if (!TrackItem(item, trackedAs, confidence, tryClear: false, force: force))
        {
            return;
        }

        List<Action> undoActions = [PopUndo().Action];

        // Check if we can remove something from the remaining treasures in
        // a dungeon
        hasTreasure = GetDungeonFromItem(item, hasTreasure)!;
        if (Tracker.TreasureTracker.TrackDungeonTreasure(hasTreasure, confidence))
            undoActions.Add(PopUndo().Action);

        IsDirty = true;

        // Check if we can remove something from the marked location
        var location = worldQueryService.Locations(itemFilter: item.Type, inRegion: hasTreasure as Region).TrySingle();
        if (location != null)
        {
            Tracker.LocationTracker.Clear(location, stateResponse: false, updateTreasureCount: false);
            undoActions.Add(PopUndo().Action);
        }

        AddUndo(() =>
        {
            foreach (var undo in undoActions)
            {
                undo();
            }
        });
    }

    public void TrackItemFrom(Item item, IHasLocations area, string? trackedAs = null, float? confidence = null, bool force = false)
    {
        if (Tracker.AutoTracker?.IsConnected == true && !force && item.Type != ItemType.Nothing)
        {
            Tracker.Say(response: Responses.AutoTrackingEnabledSass, args: [$"Hey tracker, would you please track {item.RandomName} from {area.RandomAreaName}"]);
            return;
        }

        var locations = area.Locations
            .Where(x => x.Item.Type == item.Type)
            .ToImmutableList();

        if (locations.Count == 0)
        {
            Tracker.Say(response: Responses.AreaDoesNotHaveItem, args: [item.Name, area.Name, item.Metadata.NameWithArticle]);
            return;
        }
        else if (locations.Count > 1)
        {
            // Consider tracking/clearing everything?
            Tracker.Say(response: Responses.AreaHasMoreThanOneItem, args: [item.Name, area.Name, item.Metadata.NameWithArticle]);
            return;
        }

        IsDirty = true;

        TrackItem(item, trackedAs, confidence, tryClear: false);

        List<Action> undoActions =
        [
            PopUndo().Action
        ];

        if (locations.Count == 1)
        {
            Tracker.LocationTracker.Clear(locations.Single());
            undoActions.Add(PopUndo().Action);
        }
        else if (area is IHasTreasure treasureRegion && (item.IsTreasure || area.IsKeysanityForArea))
        {
            Tracker.TreasureTracker.TrackDungeonTreasure(treasureRegion, confidence);
            undoActions.Add(PopUndo().Action);
        }

        AddUndo(() =>
        {
            foreach (var undo in undoActions)
            {
                undo();
            }
        });
    }

    public void TrackItemAmount(Item item, int count, float confidence, bool autoTracked = false, bool force = false, bool silent = false)
    {
        if (Tracker.AutoTracker?.IsConnected == true && !force && !autoTracked && item.Type != ItemType.Nothing)
        {
            Tracker.Say(response: Responses.AutoTrackingEnabledSass, args: [$"Hey tracker, would you please track {count} {item.Metadata.Plural}"]);
            return;
        }

        var newItemCount = count;
        if (item.Metadata.CounterMultiplier > 1
            && count % item.Metadata.CounterMultiplier == 0)
        {
            newItemCount = count / item.Metadata.CounterMultiplier.Value;
        }

        var oldItemCount = item.TrackingState;
        if (newItemCount == oldItemCount && !silent)
        {
            Tracker.Say(response: Responses.TrackedExactAmountDuplicate, args: [item.Metadata.Plural, count]);
            return;
        }

        item.TrackingState = newItemCount;
        if (item.TryGetTrackingResponse(out var response) && !silent)
        {
            Tracker.Say(response: response, args: [item.Counter]);
        }
        else if (newItemCount > oldItemCount && !silent)
        {
            Tracker.Say(text: Responses.TrackedItemMultiple?.Format(item.Metadata.Plural ?? $"{item.Name}s", item.Counter, item.Name));
        }
        else if (!silent)
        {
            Tracker.Say(text: Responses.UntrackedItemMultiple?.Format(item.Metadata.Plural ?? $"{item.Name}s", item.Metadata.Plural ?? $"{item.Name}s"));
        }

        IsDirty = true;
        ItemTracked?.Invoke(this, new ItemTrackedEventArgs(item, null, confidence, false));
        UpdateAllAccessibility(false, item);

        AddUndo(autoTracked, () =>
        {
            item.TrackingState = oldItemCount;
            ItemTracked?.Invoke(this, new ItemTrackedEventArgs(item, null, confidence, false));
            playerProgressionService.ResetProgression();
            UpdateAllAccessibility(true, item);
        });
    }

    public void TrackItems(List<Item> items, bool autoTracked, bool giftedItem)
    {
        if (items.Count == 1)
        {
            TrackItem(items.First(), null, null, false, autoTracked, null, giftedItem);
            return;
        }

        foreach (var item in items)
        {
            item.Track();
        }

        AnnounceTrackedItems(items);

        UpdateAllAccessibility(false, items.ToArray());

        IsDirty = true;
        RestartIdleTimers();
    }

    public void UntrackItem(Item item, float? confidence = null, bool autoTracked = false, bool force = false, bool silent = false)
    {
        if (Tracker.AutoTracker?.IsConnected == true && !force && !autoTracked && item.Type != ItemType.Nothing)
        {
            Tracker.Say(response: Responses.AutoTrackingEnabledSass, args: [$"Hey tracker, would you please untrack {item.RandomName}"]);
            return;
        }

        var originalTrackingState = item.TrackingState;
        playerProgressionService.ResetProgression();

        if (!item.Untrack() && !silent)
        {
            Tracker.Say(response: Responses.UntrackedNothing, args: [item.Name, item.Metadata.NameWithArticle]);
            return;
        }

        if (item.Metadata.HasStages && !silent)
        {
            Tracker.Say(response: Responses.UntrackedProgressiveItem, args: [item.Name, item.Metadata.NameWithArticle]);
        }
        else if (item.Metadata.Multiple && !silent)
        {
            if (item.TrackingState > 0)
            {
                if (item.Metadata.CounterMultiplier > 1)
                    Tracker.Say(text: Responses.UntrackedItemMultiple?.Format($"{item.Metadata.CounterMultiplier} {item.Metadata.Plural}", $"{item.Metadata.CounterMultiplier} {item.Metadata.Plural}"));
                else
                    Tracker.Say(response: Responses.UntrackedItemMultiple, args: [item.Name, item.Metadata.NameWithArticle]);
            }
            else
                Tracker.Say(response: Responses.UntrackedItemMultipleLast, args: [item.Name, item.Metadata.NameWithArticle]);
        }
        else if(!silent)
        {
            Tracker.Say(response: Responses.UntrackedItem, args: [item.Name, item.Metadata.NameWithArticle]);
        }

        UpdateAllAccessibility(true, item);
        IsDirty = true;

        AddUndo(autoTracked, () =>
        {
            item.TrackingState = originalTrackingState;
            playerProgressionService.ResetProgression();
            UpdateAllAccessibility(false, item);
        });
    }

    public void UpdateItemFromSnesMemory(ItemType itemType, ItemSnesMemoryType memoryType, SnesData currentData, SnesData previousData, int memoryOffset, int flagPosition = 0)
    {
        var currentValue = GetValueFromSnesData(memoryType, currentData, memoryOffset, flagPosition);
        var previousValue = GetValueFromSnesData(memoryType, previousData, memoryOffset, flagPosition);

        if (currentValue != previousValue || (memoryType == ItemSnesMemoryType.ByteSingleItem1 && currentValue > 1))
        {
            return;
        }
        else if (memoryType == ItemSnesMemoryType.ByteSingleItem12 && currentValue > 0)
        {
            if (currentValue > 2)
            {
                return;
            }

            currentValue = 1;
        }
        else if (memoryType == ItemSnesMemoryType.ByteSingleItemPositive && currentValue > 1)
        {
            currentValue = 1;
        }

        var item = worldQueryService.FirstOrDefault(itemType);

        if (item == null)
        {
            return;
        }

        if (!_maxItemCounts.TryGetValue(itemType, out var previousMaxValue))
        {
            _maxItemCounts.TryAdd(itemType, 0);
        }
        else
        {
            _maxItemCounts[itemType] = currentValue;
        }

        var silent = itemType is ItemType.HeartContainer or ItemType.HeartPiece or ItemType.ThreeHundredRupees || currentValue <= previousMaxValue;

        if (currentValue == item.TrackingState + 1)
        {
            TrackItem(item, autoTracked: true, tryClear: false, force: true, silent: silent);
        }
        else if (currentValue == item.TrackingState - 1)
        {
            UntrackItem(item, autoTracked: false, force: true, silent: silent);
        }
        else if (currentValue != item.TrackingState)
        {
            TrackItemAmount(item, currentValue, 0, true, force: true, silent: silent);
        }
    }

    private int GetValueFromSnesData(ItemSnesMemoryType memoryType, SnesData currentData, int memoryOffset, int flagPosition)
    {
        return memoryType switch
        {
            ItemSnesMemoryType.Byte => currentData.ReadUInt8(memoryOffset) ?? 0,
            ItemSnesMemoryType.ByteSingleItem1 => currentData.ReadUInt8(memoryOffset) ?? 0,
            ItemSnesMemoryType.ByteSingleItem12 => currentData.ReadUInt8(memoryOffset) ?? 0,
            ItemSnesMemoryType.ByteSingleItemPositive => currentData.ReadUInt8(memoryOffset) ?? 0,
            ItemSnesMemoryType.WordEnergy => ((currentData.ReadUInt16(memoryOffset) ?? 0) - 99) / 100,
            ItemSnesMemoryType.WordReserves => (currentData.ReadUInt16(memoryOffset) ?? 0) / 100,
            ItemSnesMemoryType.WordMetroidAmmo => (currentData.ReadUInt16(memoryOffset) ?? 0) / 5,
            ItemSnesMemoryType.ByteFlag => currentData.CheckUInt8Flag(memoryOffset, flagPosition) ? 1 : 0,
            ItemSnesMemoryType.Bottle => (currentData.ReadUInt8(memoryOffset) > 0 ? 1 : 0) + (currentData.ReadUInt8(memoryOffset + 1) > 0 ? 1 : 0) + (currentData.ReadUInt8(memoryOffset + 2) > 0 ? 1 : 0) + (currentData.ReadUInt8(memoryOffset + 3) > 0 ? 1 : 0),
            ItemSnesMemoryType.Flute => currentData.CheckUInt8Flag(memoryOffset, 0x1) || currentData.CheckUInt8Flag(memoryOffset, 0x2) ? 1 : 0,
            ItemSnesMemoryType.Sum2Bytes => (currentData.ReadUInt8(memoryOffset) ?? 0) + (currentData.ReadUInt8(memoryOffset + 1) ?? 0),
            ItemSnesMemoryType.HyruleCastleMap => currentData.CheckUInt8Flag(memoryOffset, 0x40) || currentData.CheckUInt8Flag(memoryOffset, 0x80) ? 1 : 0,
            ItemSnesMemoryType.HeartContainers => (currentData.ReadUInt8(memoryOffset) ?? 0) / 0x08 - 3,
            ItemSnesMemoryType.Rupees => (currentData.ReadUInt16(memoryOffset) ?? 0) / 250,
            _ => throw new ArgumentOutOfRangeException(nameof(memoryType), memoryType, null)
        };
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
        var locations = worldQueryService.Locations(itemFilter: item.Type)
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
            var itemsToSay = items.Where(x => x.Type.IsPossibleProgression(World.Config.ZeldaKeysanity, World.Config.MetroidKeysanity, x.IsLocalPlayerItem)).Take(2).ToList();
            if (itemsToSay.Count() < 2)
            {
                var numToTake = 2 - itemsToSay.Count();
                itemsToSay.AddRange(items.Where(x => !x.Type.IsPossibleProgression(World.Config.ZeldaKeysanity, World.Config.MetroidKeysanity, x.IsLocalPlayerItem)).Take(numToTake));
            }

            Tracker.Say(x => x.TrackedManyItems, args: [itemsToSay[0].Metadata.NameWithArticle, itemsToSay[1].Metadata.NameWithArticle, items.Count - 2]);
        }
    }

    private void GiveLocationHint(IEnumerable<Location> accessibleBefore)
    {
        var accessibleAfter = worldQueryService.AccessibleLocations(false);
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
