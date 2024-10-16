using System;
using System.Runtime.Versioning;
using System.Speech.Recognition;

using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands for marking dungeons and tracking dungeon
/// progress.
/// </summary>
public class TreasureTrackingModule : TrackerModule
{
    private const string RewardKey = "RewardName";
    private const string TreasureCountKey = "NumberOfTreasures";

    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="TreasureTrackingModule"/> class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to log information.</param>
    public TreasureTrackingModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<TreasureTrackingModule> logger)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {

    }

    #region Voice Commands
    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetMarkDungeonRewardRule()
    {
        var dungeonNames = GetDungeonNames(includeDungeonsWithoutReward: false);
        var rewardNames = new Choices();
        foreach (var reward in Enum.GetValues<RewardType>())
        {
            foreach (var name in WorldQueryService?.FirstOrDefault(reward)?.Metadata.Name ?? new SchrodingersString())
                rewardNames.Add(new SemanticResultValue(name, (int)reward));
        }

        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("mark", "set")
            .Append(DungeonKey, dungeonNames)
            .Append("as")
            .Append(RewardKey, rewardNames);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetMarkRemainingDungeonRewardsRule()
    {
        return new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("mark", "set")
            .OneOf("remaining dungeons", "other dungeons", "unmarked dungeons")
            .Append("as")
            .OneOf("crystal", "blue crystal");
    }

    [SupportedOSPlatform("windows")]
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

    [SupportedOSPlatform("windows")]
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

    [SupportedOSPlatform("windows")]
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

    [SupportedOSPlatform("windows")]
    public override void AddCommands()
    {
        AddCommand("Mark dungeon pendant/crystal", GetMarkDungeonRewardRule(), (result) =>
        {
            var dungeon = (IHasReward)GetDungeonFromResult(TrackerBase, result);
            var reward = (RewardType)result.Semantics[RewardKey].Value;
            TrackerBase.RewardTracker.SetAreaReward(dungeon, reward, result.Confidence);
        });

        AddCommand("Mark remaining dungeons", GetMarkRemainingDungeonRewardsRule(), (result) =>
        {
            TrackerBase.RewardTracker.SetUnmarkedRewards(RewardType.CrystalBlue, result.Confidence);
        });

        AddCommand("Mark dungeon as cleared", GetClearDungeonRule(), (result) =>
        {
            if (GetDungeonFromResult(TrackerBase, result) is IHasBoss dungeon)
            {
                TrackerBase.BossTracker.MarkRegionBossAsDefeated(dungeon, result.Confidence);
            }
        });

        AddCommand("Mark dungeon medallion", GetMarkDungeonRequirementRule(), (result) =>
        {
            if (GetDungeonFromResult(TrackerBase, result) is IHasPrerequisite dungeon)
            {
                var medallion = GetItemFromResult(TrackerBase, result, out _);
                TrackerBase.PrerequisiteTracker.SetDungeonRequirement(dungeon, medallion.Type, result.Confidence);
            }
        });

        AddCommand("Clear dungeon treasure", GetTreasureTrackingRule(), (result) =>
        {
            var count = result.Semantics.ContainsKey(TreasureCountKey)
                ? (int)result.Semantics[TreasureCountKey].Value
                : 1;
            var dungeon = GetDungeonFromResult(TrackerBase, result);
            TrackerBase.TreasureTracker.TrackDungeonTreasure(dungeon, result.Confidence, amount: count);
        });
    }
    #endregion
}
