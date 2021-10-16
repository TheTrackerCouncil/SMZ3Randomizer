using System;
using System.Linq;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class ZeldaDungeonTrackingModule : TrackerModule
    {
        private const string RewardKey = "RewardName";
        private const string MedallionKey = "MedallionName";

        public ZeldaDungeonTrackingModule(Tracker tracker) : base(tracker)
        {
            AddCommand("MarkDungeonRewardRule", GetMarkDungeonRewardRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                var reward = (RewardItem)result.Semantics[RewardKey].Value;
                tracker.SetDungeonReward(dungeon, reward, result.Confidence);
            });

            AddCommand("ClearDungeonRule", GetClearDungeonRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                tracker.ClearDungeon(dungeon, result.Confidence);
            });

            AddCommand("MarkDungeonRequirementRule", GetMarkDungeonRequirementRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                var medallion = (Medallion)result.Semantics[MedallionKey].Value;
                tracker.SetDungeonRequirement(dungeon, medallion, result.Confidence);
            });

            AddCommand("TreasureTrackingRule", GetTreasureTrackingRule(), (tracker, result) =>
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
                .OneOf("clear", "mark")
                .OneOf("item", "treasure", "chest", "treasure chest")
                .Append("in")
                .Append(DungeonKey, dungeonNames);
        }

        private Choices GetDungeonNames()
        {
            var dungeonNames = new Choices();
            foreach (var dungeon in Tracker.Dungeons)
            {
                foreach (var name in dungeon.Name)
                    dungeonNames.Add(new SemanticResultValue(name.Text, name.Text));
                dungeonNames.Add(new SemanticResultValue(dungeon.Abbreviation, dungeon.Abbreviation));
            }

            return dungeonNames;
        }
    }
}
