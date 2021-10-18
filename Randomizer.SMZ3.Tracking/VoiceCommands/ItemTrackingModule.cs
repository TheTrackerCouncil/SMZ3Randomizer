using System;

using Microsoft.EntityFrameworkCore.Metadata.Conventions;

using Randomizer.Shared.Models;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class ItemTrackingModule : TrackerModule
    {
        public ItemTrackingModule(Tracker tracker)
            : base(tracker)
        {
            AddCommand("TrackItemRule", GetTrackItemRule(), (tracker, result) =>
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
        }

        private GrammarBuilder GetTrackItemRule()
        {
            var dungeonNames = GetDungeonNames();
            var itemNames = GetItemNames();
            var locationNames = GetLocationNames();
            var roomNames = GetRoomNames();

            var trackItemNormal = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("track", "please track")
                .Append(ItemNameKey, itemNames);

            var trackItemDungeon = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("track", "please track")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from")
                .Append(DungeonKey, dungeonNames);

            var trackItemLocation = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("track", "please track")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from", "in the", "from the")
                .Append(LocationKey, locationNames);

            var trackItemRoom = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("track", "please track")
                .Append(ItemNameKey, itemNames)
                .OneOf("in", "from", "in the", "from the")
                .Append(RoomKey, roomNames);

            return GrammarBuilder.Combine(
                trackItemNormal, trackItemDungeon, trackItemLocation, trackItemRoom);
        }
    }
}
