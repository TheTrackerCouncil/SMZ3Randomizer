﻿using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for tracking items.
    /// </summary>
    public class ItemTrackingModule : TrackerModule
    {
        private const string ItemCountKey = "ItemCount";

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemTrackingModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to log information.</param>
        public ItemTrackingModule(Tracker tracker, IItemService itemService, ILogger<ItemTrackingModule> logger)
            : base(tracker, itemService, logger)
        {
            AddCommand("Track item", GetTrackItemRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out var itemName);

                if (result.Semantics.ContainsKey(DungeonKey))
                {
                    var dungeon = GetDungeonFromResult(tracker, result);
                    tracker.TrackItem(item, dungeon,
                        trackedAs: itemName,
                        confidence: result.Confidence);
                }
                else if (result.Semantics.ContainsKey(RoomKey))
                {
                    var room = GetRoomFromResult(tracker, result);
                    tracker.TrackItem(item, room,
                        trackedAs: itemName,
                        confidence: result.Confidence);
                }
                else if (result.Semantics.ContainsKey(LocationKey))
                {
                    var location = GetLocationFromResult(tracker, result);
                    tracker.TrackItem(item: item,
                        trackedAs: itemName,
                        confidence: result.Confidence,
                        tryClear: true,
                        autoTracked: false,
                        location: location);
                }
                else
                {
                    tracker.TrackItem(item,
                        trackedAs: itemName,
                        confidence: result.Confidence);
                }
            });

            AddCommand("Track death", GetTrackDeathRule(), (tracker, result) =>
            {
                var death = itemService.FindOrDefault("Death");
                if (death == null)
                {
                    Logger.LogError("Tried to track death, but could not find an item named 'Death'.");
                    tracker.Say(x => x.Error);
                    return;
                }

                tracker.TrackItem(death, confidence: result.Confidence, tryClear: false);
            });

            AddCommand("Track available items in an area", GetTrackEverythingRule(), (tracker, result) =>
            {
                if (result.Semantics.ContainsKey(RoomKey))
                {
                    var room = GetRoomFromResult(tracker, result);
                    tracker.ClearArea(room,
                        trackItems: true,
                        includeUnavailable: false,
                        confidence: result.Confidence);
                }
                else if (result.Semantics.ContainsKey(RegionKey))
                {
                    var region = GetRegionFromResult(tracker, result);
                    tracker.ClearArea(region,
                        trackItems: true,
                        includeUnavailable: false,
                        confidence: result.Confidence);
                }
            });

            AddCommand("Track all items in an area (including out-of-logic)", GetTrackEverythingIncludingOutOfLogicRule(), (tracker, result) =>
            {
                if (result.Semantics.ContainsKey(RoomKey))
                {
                    var room = GetRoomFromResult(tracker, result);
                    tracker.ClearArea(room,
                        trackItems: true,
                        includeUnavailable: true,
                        confidence: result.Confidence);
                }
                else if (result.Semantics.ContainsKey(RegionKey))
                {
                    var region = GetRegionFromResult(tracker, result);
                    tracker.ClearArea(region,
                        trackItems: true,
                        includeUnavailable: true,
                        confidence: result.Confidence);
                }
            });

            AddCommand("Untrack an item", GetUntrackItemRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out _);
                tracker.UntrackItem(item, result.Confidence);
            });

            AddCommand("Set item count", GetSetItemCountRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out _);
                var count = (int)result.Semantics[ItemCountKey].Value;
                tracker.TrackItemAmount(item, count, result.Confidence);
            });
        }

        private GrammarBuilder GetTrackDeathRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker, ")
                .Append("I died");
        }

        private GrammarBuilder GetTrackItemRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
            var itemNames = GetItemNames(x => x.Name[0] != "Content");
            var locationNames = GetLocationNames();
            var roomNames = GetRoomNames();

            var trackItemNormal = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames);

            var trackItemDungeon = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from")
                .Append(DungeonKey, dungeonNames);

            var trackItemLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from", "in the", "from the")
                .Append(LocationKey, locationNames);

            var trackItemRoom = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from", "in the", "from the")
                .Append(RoomKey, roomNames);

            return GrammarBuilder.Combine(
                trackItemNormal, trackItemDungeon, trackItemLocation, trackItemRoom);
        }

        private GrammarBuilder GetTrackEverythingRule()
        {
            var roomNames = GetRoomNames();
            var regionNames = GetRegionNames();

            var trackAllInRoom = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .OneOf("everything", "available items")
                .OneOf("in", "from", "in the", "from the")
                .Append(RoomKey, roomNames);

            var trackAllInRegion = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .OneOf("everything", "available items")
                .OneOf("in", "from")
                .Append(RegionKey, regionNames);

            return GrammarBuilder.Combine(trackAllInRoom, trackAllInRegion);
        }

        private GrammarBuilder GetTrackEverythingIncludingOutOfLogicRule()
        {
            var roomNames = GetRoomNames();
            var regionNames = GetRegionNames();

            var trackAllInRoom = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("force", "sudo")
                .OneOf("track", "add")
                .OneOf("everything", "all items")
                .OneOf("in", "from", "in the", "from the")
                .Append(RoomKey, roomNames);

            var trackAllInRegion = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("force", "sudo")
                .OneOf("track", "add")
                .OneOf("everything", "all items")
                .OneOf("in", "from")
                .Append(RegionKey, regionNames);

            var cheatedRoom = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("sequence break", "I sequence broke", "I cheated my way to")
                .Append(RoomKey, roomNames);

            var cheatedRegion = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("sequence break", "I sequence broke", "I cheated my way to")
                .Append(RegionKey, regionNames);

            return GrammarBuilder.Combine(trackAllInRoom, trackAllInRegion,
                cheatedRoom, cheatedRegion);
        }

        private GrammarBuilder GetUntrackItemRule()
        {
            var itemNames = GetItemNames();

            var untrackItem = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("untrack", "remove")
                .Optional("a", "an", "the")
                .Append(ItemNameKey, itemNames);

            var toggleItemOff = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .Append("toggle")
                .Append(ItemNameKey, itemNames)
                .Append("off");

            return GrammarBuilder.Combine(untrackItem, toggleItemOff);
        }

        private GrammarBuilder GetSetItemCountRule()
        {
            var itemNames = GetPluralItemNames();
            var numbers = new Choices();
            for (var i = 0; i <= 200; i++)
                numbers.Add(new SemanticResultValue(i.ToString(), i));

            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("I have", "I've got", "I possess", "I am in the possession of")
                .Append(ItemCountKey, numbers)
                .Append(ItemNameKey, itemNames);
        }
    }
}
