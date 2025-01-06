using System;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands for tracking bosses.
/// </summary>
public class BossTrackingModule : TrackerModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BossTrackingModule"/>
    /// class.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    public BossTrackingModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<BossTrackingModule> logger)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {

    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetMarkBossAsDefeatedRule()
    {
        var bossNames = GetBossNames();
        var beatBoss = new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional(GrammarBuilder.ForceCommandIdentifiers)
            .OneOf("track", "I beat", "I defeated", "I beat off", "I killed")
            .Append(BossKey, bossNames);

        var markBoss = new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional(GrammarBuilder.ForceCommandIdentifiers)
            .Append("mark")
            .Append(BossKey, bossNames)
            .Append("as")
            .OneOf("beaten", "beaten off", "dead", "fucking dead", "defeated");

        var bossIsDead = new GrammarBuilder()
            .Append("Hey tracker,")
            .Append(BossKey, bossNames)
            .OneOf("is dead", "is fucking dead");

        return GrammarBuilder.Combine(beatBoss, markBoss, bossIsDead);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetMarkBossAsNotDefeatedRule()
    {
        var bossNames = GetBossNames();

        var markBoss = new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional(GrammarBuilder.ForceCommandIdentifiers)
            .Append("mark")
            .Append(BossKey, bossNames)
            .Append("as")
            .OneOf("alive", "not defeated", "undefeated");

        var untrack = new GrammarBuilder()
            .Append("Hey tracker,")
            .Optional(GrammarBuilder.ForceCommandIdentifiers)
            .Append("untrack")
            .Append(BossKey, bossNames);

        var bossIsAlive = new GrammarBuilder()
            .Append("Hey tracker,")
            .Append(BossKey, bossNames)
            .OneOf("is alive", "is still alive");

        return GrammarBuilder.Combine(markBoss, untrack, bossIsAlive);
    }

    [SupportedOSPlatform("windows")]
    private GrammarBuilder GetBossDefeatedWithContentRule()
    {
        var bossNames = GetBossNames();
        var beatBoss = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("I beat", "I defeated", "I beat off", "I killed")
            .Append(BossKey, bossNames)
            .OneOf("Without getting hit", "Without taking damage", "And didn't get hit", "And didn't take damage", "In one cycle");

        var oneCycled = new GrammarBuilder()
            .Append("Hey tracker,")
            .OneOf("I one cycled", "I quick killed")
            .Append(BossKey, bossNames);

        return GrammarBuilder.Combine(beatBoss, oneCycled);
    }

    [SupportedOSPlatform("windows")]
    public override void AddCommands()
    {
        AddCommand("Mark boss as defeated", GetMarkBossAsDefeatedRule(), (result) =>
        {
            var boss = GetBossFromResult(TrackerBase, result) ??
                       throw new Exception($"Could not find boss or dungeon in command: '{result.Text}'");

            var admittedGuilt = result.Text.ContainsAny("killed", "beat", "defeated", "dead")
                                && !result.Text.ContainsAny("beat off", "beaten off");

            var force = result.Text.ContainsAny(GrammarBuilder.ForceCommandIdentifiers);

            if (boss.Region != null)
            {
                // Track boss with associated dungeon or region
                TrackerBase.BossTracker.MarkBossAsDefeated(boss.Region, result.Confidence, false, admittedGuilt, force);
                return;
            }

            // Track standalone boss
            TrackerBase.BossTracker.MarkBossAsDefeated(boss, admittedGuilt, result.Confidence, force);
        });

        AddCommand("Mark boss as alive", GetMarkBossAsNotDefeatedRule(), (result) =>
        {
            var boss = GetBossFromResult(TrackerBase, result) ?? throw new Exception($"Could not find boss or dungeon in command: '{result.Text}'");

            var force = result.Text.ContainsAny(GrammarBuilder.ForceCommandIdentifiers);

            if (boss.Region != null)
            {
                // Untrack boss with associated dungeon or region
                TrackerBase.BossTracker.MarkBossAsNotDefeated(boss.Region, result.Confidence, force);
                return;
            }

            // Untrack standalone boss
            TrackerBase.BossTracker.MarkBossAsNotDefeated(boss, result.Confidence, force);
        });

        AddCommand("Mark boss as defeated with content", GetBossDefeatedWithContentRule(), (result) =>
        {
            var contentItemData = WorldQueryService.FirstOrDefault("Content");

            var dungeon = GetBossDungeonFromResult(TrackerBase, result) as IHasBoss;
            if (dungeon != null)
            {
                if (contentItemData != null)
                {
                    TrackerBase.Say(x => x.DungeonBossClearedAddContent);
                    TrackerBase.ItemTracker.TrackItem(contentItemData, force: true);
                }

                // Track boss with associated dungeon
                TrackerBase.BossTracker.MarkBossAsDefeated(dungeon, result.Confidence, force: true);
                return;
            }

            var boss = GetBossFromResult(TrackerBase, result);
            if (boss != null)
            {
                if (contentItemData != null)
                {
                    TrackerBase.Say(x => x.DungeonBossClearedAddContent);
                    TrackerBase.ItemTracker.TrackItem(contentItemData, force: true);
                }

                // Track standalone boss
                var admittedGuilt = result.Text.ContainsAny("killed", "beat", "defeated", "dead")
                                    && !result.Text.ContainsAny("beat off", "beaten off");
                TrackerBase.BossTracker.MarkBossAsDefeated(boss, admittedGuilt, result.Confidence, force: true);
                return;
            }

            throw new Exception($"Could not find boss or dungeon in command: '{result.Text}'");
        });
    }
}
