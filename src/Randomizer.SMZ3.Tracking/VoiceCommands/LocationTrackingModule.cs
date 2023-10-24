using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
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
        public LocationTrackingModule(ITracker tracker, IItemService itemService, IWorldService worldService, ILogger<LocationTrackingModule> logger)
            : base(tracker, itemService, worldService, logger)
        {

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

        private GrammarBuilder GetClearAreaIncludingOutOfLogicRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);
            var roomNames = GetRoomNames();
            var regionNames = GetRegionNames(excludeDungeons: true);

            var clearDungeon = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("force", "sudo")
                .OneOf("clear", "please clear")
                .Append(DungeonKey, dungeonNames);

            var clearRoom = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("force", "sudo")
                .OneOf("clear", "please clear")
                .Append(RoomKey, roomNames);

            var clearRegion = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("force", "sudo")
                .OneOf("clear", "please clear")
                .Append(RegionKey, regionNames);

            return GrammarBuilder.Combine(clearDungeon, clearRoom, clearRegion);
        }

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
        public override void AddCommands()
        {
            AddCommand("Mark item at specific location", GetMarkItemAtLocationRule(), (result) =>
            {
                var item = GetItemFromResult(Tracker, result, out _);
                var location = GetLocationFromResult(Tracker, result);
                Tracker.MarkLocation(location, item, result.Confidence);
            });

            AddCommand("Clear specific item location", GetClearLocationRule(), (result) =>
            {
                var location = GetLocationFromResult(Tracker, result);
                Tracker.Clear(location, result.Confidence);
            });

            AddCommand("Clear available items in an area", GetClearAreaRule(), (result) =>
            {
                if (result.Semantics.ContainsKey(RoomKey))
                {
                    var room = GetRoomFromResult(Tracker, result);
                    Tracker.ClearArea(room,
                        trackItems: false,
                        includeUnavailable: false,
                        confidence: result.Confidence);
                }
                else if (result.Semantics.ContainsKey(DungeonKey))
                {
                    var dungeon = GetDungeonFromResult(Tracker, result);
                    Tracker.ClearDungeon(dungeon, result.Confidence);
                }
                else if (result.Semantics.ContainsKey(RegionKey))
                {
                    var region = GetRegionFromResult(Tracker, result);
                    Tracker.ClearArea(region,
                        trackItems:false,
                        includeUnavailable: false,
                        confidence: result.Confidence);
                }
            });

            AddCommand("Clear all items in an area (including out-of-logic)", GetClearAreaIncludingOutOfLogicRule(), (result) =>
            {
                if (result.Semantics.ContainsKey(RoomKey))
                {
                    var room = GetRoomFromResult(Tracker, result);
                    Tracker.ClearArea(room,
                        trackItems: false,
                        includeUnavailable: true,
                        confidence: result.Confidence);
                }
                else if (result.Semantics.ContainsKey(DungeonKey))
                {
                    var dungeon = GetDungeonFromResult(Tracker, result);
                    Tracker.ClearDungeon(dungeon, result.Confidence);
                }
                else if (result.Semantics.ContainsKey(RegionKey))
                {
                    var region = GetRegionFromResult(Tracker, result);
                    Tracker.ClearArea(region,
                        trackItems:false,
                        includeUnavailable: true,
                        confidence: result.Confidence);
                }
            });
        }
    }
}
