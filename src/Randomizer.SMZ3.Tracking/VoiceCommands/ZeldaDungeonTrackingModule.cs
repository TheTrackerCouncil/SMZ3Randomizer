using System;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;
using Randomizer.Shared;

using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Services;

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
        private const string TreasureCountKey = "NumberOfTreasures";

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ZeldaDungeonTrackingModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to log information.</param>
        public ZeldaDungeonTrackingModule(Tracker tracker, IItemService itemService, ILogger<ZeldaDungeonTrackingModule> logger)
            : base(tracker, itemService, logger)
        {
            AddCommand("Mark dungeon pendant/crystal", GetMarkDungeonRewardRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                var reward = (RewardItem)result.Semantics[RewardKey].Value;
                tracker.SetDungeonReward(dungeon, reward, result.Confidence);
            });

            AddCommand("Mark remaining dungeons", GetMarkRemainingDungeonRewardsRule(), (tracker, result) =>
            {
                tracker.SetUnmarkedDungeonReward(RewardItem.Crystal, result.Confidence);
            });

            AddCommand("Mark dungeon as cleared", GetClearDungeonRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                tracker.MarkDungeonAsCleared(dungeon, result.Confidence);
            });

            AddCommand("Mark dungeon medallion", GetMarkDungeonRequirementRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                var medallion = (Medallion)result.Semantics[MedallionKey].Value;
                tracker.SetDungeonRequirement(dungeon, medallion, result.Confidence);
            });

            AddCommand("Clear dungeon treasure", GetTreasureTrackingRule(), (tracker, result) =>
            {
                var count = result.Semantics.ContainsKey(TreasureCountKey)
                    ? (int)result.Semantics[TreasureCountKey].Value
                    : 1;
                var dungeon = GetDungeonFromResult(tracker, result);
                tracker.TrackDungeonTreasure(dungeon, result.Confidence, amount: count);
                dungeon.HasManuallyClearedTreasure = true;
            });
        }

        private GrammarBuilder GetMarkDungeonRewardRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: false);
            var rewardNames = new Choices();
            foreach (var reward in Enum.GetValues<RewardItem>())
            {
                foreach (var name in ItemService?.GetOrDefault(reward)?.Name ?? new Configuration.ConfigTypes.SchrodingersString())
                    rewardNames.Add(new SemanticResultValue(name, (int)reward));
            }

            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("mark", "set")
                .Append(DungeonKey, dungeonNames)
                .Append("as")
                .Append(RewardKey, rewardNames);
        }

        private GrammarBuilder GetMarkRemainingDungeonRewardsRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("mark", "set")
                .OneOf("remaining dungeons", "other dungeons", "unmarked dungeons")
                .Append("as")
                .OneOf("crystal", "blue crystal");
        }

        private GrammarBuilder GetClearDungeonRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);

            var markDungeon = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("mark")
                .Append(DungeonKey, dungeonNames)
                .Append("as")
                .OneOf("cleared", "beaten");

            return GrammarBuilder.Combine(markDungeon);
        }

        private GrammarBuilder GetMarkDungeonRequirementRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: false);
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
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);

            var medallions = new Choices();
            foreach (var medallion in Enum.GetValues<Medallion>())
                medallions.Add(new SemanticResultValue(medallion.ToString(), (int)medallion));

            var treasureCount = new Choices();
            for (var i = 2; i <= 20; i++)
                treasureCount.Add(new SemanticResultValue(i.ToString(), i));

            var clearOne = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("clear")
                .OneOf("one item", "an item", "one treasure", "a treasure")
                .OneOf("in", "from")
                .Append(DungeonKey, dungeonNames);

            var clearMultiple = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("clear")
                .Append(TreasureCountKey, treasureCount)
                .OneOf("items", "treasures")
                .OneOf("in", "from")
                .Append(DungeonKey, dungeonNames);

            return GrammarBuilder.Combine(clearOne, clearMultiple);
        }
    }
}
