using System;

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
                var dungeon = result.Semantics.ContainsKey(DungeonKey)
                    ? GetDungeonFromResult(tracker, result)
                    : null;

                tracker.TrackItem(item,
                    trackedAs: itemName,
                    dungeon: dungeon,
                    confidence: result.Confidence);
            });
        }

        private GrammarBuilder GetTrackItemRule()
        {
            var dungeonNames = GetDungeonNames();
            var itemNames = GetItemNames();

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

            return GrammarBuilder.Combine(
                trackItemNormal, trackItemDungeon);
        }
    }
}
