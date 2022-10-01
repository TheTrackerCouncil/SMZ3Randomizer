using System;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for marking and clearing locations.
    /// </summary>
    public class LocationTrackingModule : TrackerModule
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="LocationTrackingModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="itemService">Service to get item information</param>
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger">Used to log information.</param>
        public LocationTrackingModule(Tracker tracker, IItemService itemService, IWorldService worldService, ILogger<LocationTrackingModule> logger)
            : base(tracker, itemService, worldService, logger)
        {
            AddCommand("Mark item at specific location", GetMarkItemAtLocationRule(), (tracker, result) =>
            {
                var item = GetItemFromResult(tracker, result, out _);
                var location = GetLocationFromResult(tracker, result);
                tracker.MarkLocation(location, item, result.Confidence);
            });

            AddCommand("Clear specific item location", GetClearLocationRule(), (tracker, result) =>
            {
                var location = GetLocationFromResult(tracker, result);
                tracker.Clear(location, result.Confidence);
            });

            AddCommand("Clear available items in an area", GetClearAreaRule(), (tracker, result) =>
            {
                if (result.Semantics.ContainsKey(RoomKey))
                {
                    var room = GetRoomFromResult(tracker, result);
                    tracker.ClearArea(room,
                        trackItems: false,
                        includeUnavailable: false,
                        confidence: result.Confidence);
                }
                else if (result.Semantics.ContainsKey(DungeonKey))
                {
                    var dungeon = GetDungeonFromResult(tracker, result);
                    tracker.ClearDungeon(dungeon, result.Confidence);
                }
                else if (result.Semantics.ContainsKey(RegionKey))
                {
                    var region = GetRegionFromResult(tracker, result);
                    tracker.ClearArea(region,
                        trackItems:false,
                        includeUnavailable: false,
                        confidence: result.Confidence);
                }
            });
        }

        private GrammarBuilder GetMarkItemAtLocationRule()
        {
            var itemNames = GetItemNames();
            var locationNames = GetLocationNames();

            var itemIsAtLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(ItemNameKey, itemNames)
                .OneOf("is at", "are at")
                .Append(LocationKey, locationNames);

            var theItemIsAtLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("a", "an", "the")
                .Append(ItemNameKey, itemNames)
                .OneOf("is at", "are at")
                .Append(LocationKey, locationNames);

            var thereIsItemAtLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("there are", "there is a", "there is an")
                .Append(ItemNameKey, itemNames)
                .Append("at")
                .Append(LocationKey, locationNames);

            var locationHasItem = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(LocationKey, locationNames)
                .OneOf("has", "has a", "has an", "has the")
                .Append(ItemNameKey, itemNames);

            var markAtLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("mark")
                .Append(ItemNameKey, itemNames)
                .Append("at")
                .Append(LocationKey, locationNames);

            return GrammarBuilder.Combine(
                itemIsAtLocation, theItemIsAtLocation, thereIsItemAtLocation,
                locationHasItem, markAtLocation);
        }

        private GrammarBuilder GetClearLocationRule()
        {
            var locationNames = GetLocationNames();

            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("clear", "ignore")
                .Append(LocationKey, locationNames);
        }

        private GrammarBuilder GetClearAreaRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
            var roomNames = GetRoomNames();
            var regionNames = GetRegionNames(excludeDungeons: true);

            var clearDungeon = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("clear", "please clear")
                .Append(DungeonKey, dungeonNames);

            var clearRoom = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("clear", "please clear")
                .Append(RoomKey, roomNames);

            var clearRegion = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("clear", "please clear")
                .Append(RegionKey, regionNames);

            return GrammarBuilder.Combine(clearDungeon, clearRoom, clearRegion);
        }
    }
}
