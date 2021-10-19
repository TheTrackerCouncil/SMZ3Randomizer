using System;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for marking dungeons and tracking dungeon
    /// progress.
    /// </summary>
    public class ZeldaDungeonTrackingModule : TrackerModule
    {
        private const string RewardKey = "RewardName";
        private const string MedallionKey = "MedallionName";

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ZeldaDungeonTrackingModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        public ZeldaDungeonTrackingModule(Tracker tracker) : base(tracker)
        {
            AddCommand("Mark dungeon pendant/crystal", GetMarkDungeonRewardRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                var reward = (RewardItem)result.Semantics[RewardKey].Value;
                tracker.SetDungeonReward(dungeon, reward, result.Confidence);
            });

            AddCommand("Clear dungeon", GetClearDungeonRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                tracker.ClearDungeon(dungeon, result.Confidence);
            });

            AddCommand("Mark dungeon medallion", GetMarkDungeonRequirementRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                var medallion = (Medallion)result.Semantics[MedallionKey].Value;
                tracker.SetDungeonRequirement(dungeon, medallion, result.Confidence);
            });

            AddCommand("Track dungeon treasure", GetTreasureTrackingRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                tracker.TrackDungeonTreasure(dungeon, result.Confidence);
            });
        }

        private GrammarBuilder GetMarkDungeonRewardRule()
        {
            var dungeonNames = GetDungeonNames();
            var rewardNames = new Choices();
            foreach (var reward in Enum.GetValues<RewardItem>())
            {
                foreach (var name in reward.GetName())
                    rewardNames.Add(new SemanticResultValue(name, (int)reward));
            }

            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("mark", "set")
                .Append(DungeonKey, dungeonNames)
                .Append("as")
                .Append(RewardKey, rewardNames);
        }

        private GrammarBuilder GetClearDungeonRule()
        {
            var dungeonNames = GetDungeonNames();
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("clear")
                .Append(DungeonKey, dungeonNames);
        }

        private GrammarBuilder GetMarkDungeonRequirementRule()
        {
            var dungeonNames = GetDungeonNames();
            var medallions = new Choices();
            foreach (var medallion in Enum.GetValues<Medallion>())
                medallions.Add(new SemanticResultValue(medallion.ToString(), (int)medallion));

            var dungeonFirst = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(DungeonKey, dungeonNames)
                .OneOf("requires", "needs")
                .Append(MedallionKey, medallions);

            var itemFirst = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(MedallionKey, medallions)
                .OneOf("is required for", "is needed for")
                .Append(DungeonKey, dungeonNames);

            var markDungeon = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("mark")
                .Append(DungeonKey, dungeonNames)
                .OneOf("as", "as requiring", "as needing")
                .Append(MedallionKey, medallions);

            var markItem = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("mark")
                .Append(MedallionKey, medallions)
                .OneOf("as", "as required for", "as needed for")
                .Append(DungeonKey, dungeonNames);

            return GrammarBuilder.Combine(
                dungeonFirst, itemFirst,
                markDungeon, markItem);
        }

        private GrammarBuilder GetTreasureTrackingRule()
        {
            var dungeonNames = GetDungeonNames();
            var medallions = new Choices();
            foreach (var medallion in Enum.GetValues<Medallion>())
                medallions.Add(new SemanticResultValue(medallion.ToString(), (int)medallion));

            return new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("clear one")
                .OneOf("item", "treasure", "chest", "treasure chest")
                .Append("in")
                .Append(DungeonKey, dungeonNames);
        }
    }
}
