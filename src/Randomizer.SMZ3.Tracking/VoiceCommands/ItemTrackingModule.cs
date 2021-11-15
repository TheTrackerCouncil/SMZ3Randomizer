using System;

using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for tracking items.
    /// </summary>
    public class ItemTrackingModule : TrackerModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemTrackingModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to log information.</param>
        public ItemTrackingModule(Tracker tracker, ILogger<ItemTrackingModule> logger) : base(tracker, logger)
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
                    tracker.TrackItem(item, location,
                        trackedAs: itemName,
                        confidence: result.Confidence);
                }
                else
                {
                    tracker.TrackItem(item,
                        trackedAs: itemName,
                        confidence: result.Confidence);
                }
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

            AddCommand("Untrack an item", GetUntrackItemRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out _);
                tracker.UntrackItem(item, result.Confidence);
            });
        }

        private GrammarBuilder GetTrackItemRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
            var itemNames = GetItemNames();
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
                .OneOf("everything", "all items", "available items")
                .OneOf("in", "from", "in the", "from the")
                .Append(RoomKey, roomNames);

            var trackAllInRegion = new GrammarBuilder()
                .Append("Hey tracker,")
                .Optional("please", "would you kindly")
                .OneOf("track", "add")
                .OneOf("everything", "all items", "available items")
                .OneOf("in", "from")
                .Append(RegionKey, regionNames);

            return GrammarBuilder.Combine(trackAllInRoom, trackAllInRegion);
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
    }
}
