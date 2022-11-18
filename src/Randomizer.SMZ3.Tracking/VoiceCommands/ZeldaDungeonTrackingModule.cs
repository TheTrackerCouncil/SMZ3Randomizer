using System;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;
using Randomizer.Shared;

using Randomizer.Data.Configuration;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.Data.Configuration.ConfigTypes;
using System.Collections.Generic;
using System.Linq;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for marking dungeons and tracking dungeon
    /// progress.
    /// </summary>
    public class ZeldaDungeonTrackingModule : TrackerModule
    {
        private const string RewardKey = "RewardName";
        private const string TreasureCountKey = "NumberOfTreasures";

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ZeldaDungeonTrackingModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="itemService">Service to get item information</param>
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger">Used to log information.</param>
        public ZeldaDungeonTrackingModule(Tracker tracker, IItemService itemService, IWorldService worldService, ILogger<ZeldaDungeonTrackingModule> logger)
            : base(tracker, itemService, worldService, logger)
        {
            AddCommand("Mark dungeon pendant/crystal", GetMarkDungeonRewardRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                var reward = (RewardType)result.Semantics[RewardKey].Value;
                tracker.SetDungeonReward(dungeon, reward, result.Confidence);
            });

            AddCommand("Mark remaining dungeons", GetMarkRemainingDungeonRewardsRule(), (tracker, result) =>
            {
                tracker.SetUnmarkedDungeonReward(RewardType.CrystalBlue, result.Confidence);
            });

            AddCommand("Mark dungeon as cleared", GetClearDungeonRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                tracker.MarkDungeonAsCleared(dungeon, result.Confidence);
            });

            AddCommand("Mark dungeon medallion", GetMarkDungeonRequirementRule(), (tracker, result) =>
            {
                var dungeon = GetDungeonFromResult(tracker, result);
                var medallion = GetItemFromResult(tracker, result, out string itemName);
                tracker.SetDungeonRequirement(dungeon, medallion.Type, result.Confidence);
            });

            AddCommand("Clear dungeon treasure", GetTreasureTrackingRule(), (tracker, result) =>
            {
                var count = result.Semantics.ContainsKey(TreasureCountKey)
                    ? (int)result.Semantics[TreasureCountKey].Value
                    : 1;
                var dungeon = GetDungeonFromResult(tracker, result);
                tracker.TrackDungeonTreasure(dungeon, result.Confidence, amount: count);
                dungeon.DungeonState.HasManuallyClearedTreasure = true;
            });
        }

        private GrammarBuilder GetMarkDungeonRewardRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: false);
            var rewardNames = new Choices();
            foreach (var reward in Enum.GetValues<RewardType>())
            {
                foreach (var name in ItemService?.FirstOrDefault(reward)?.Metadata.Name ?? new SchrodingersString())
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
            var medallions = GetMedallionNames();

            var dungeonFirst = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(DungeonKey, dungeonNames)
                .OneOf("requires", "needs")
                .Append(ItemNameKey, medallions);

            var itemFirst = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(ItemNameKey, medallions)
                .OneOf("is required for", "is needed for")
                .Append(DungeonKey, dungeonNames);

            var markDungeon = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("mark")
                .Append(DungeonKey, dungeonNames)
                .OneOf("as", "as requiring", "as needing")
                .Append(ItemNameKey, medallions);

            var markItem = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("mark")
                .Append(ItemNameKey, medallions)
                .OneOf("as", "as required for", "as needed for")
                .Append(DungeonKey, dungeonNames);

            return GrammarBuilder.Combine(
                dungeonFirst, itemFirst,
                markDungeon, markItem);
        }

        private GrammarBuilder GetTreasureTrackingRule()
        {
            var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: true);

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
