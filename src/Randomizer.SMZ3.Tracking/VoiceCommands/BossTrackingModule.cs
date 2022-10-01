using System;
using System.Linq;
using Microsoft.Extensions.Logging;

using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
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
        /// <param name="itemService">Service to get item information</param>
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger">Used to write logging information.</param>
        public BossTrackingModule(Tracker tracker, IItemService itemService, IWorldService worldService, ILogger<BossTrackingModule> logger)
            : base(tracker, itemService, worldService, logger)
        {
            AddCommand("Mark boss as defeated", GetMarkBossAsDefeatedRule(), (tracker, result) =>
            {
                var dungeon = GetBossDungeonFromResult(tracker, result);
                if (dungeon != null)
                {
                    // Track boss with associated dungeon
                    tracker.MarkDungeonAsCleared(dungeon, result.Confidence);
                    return;
                }

                var boss = GetBossFromResult(tracker, result);
                if (boss != null)
                {
                    // Track standalone boss
                    var admittedGuilt = result.Text.ContainsAny("killed", "beat", "defeated", "dead")
                        && !result.Text.ContainsAny("beat off", "beaten off");
                    tracker.MarkBossAsDefeated(boss, admittedGuilt, result.Confidence);
                    return;
                }

                throw new Exception($"Could not find boss or dungeon in command: '{result.Text}'");
            });

            AddCommand("Mark boss as alive", GetMarkBossAsNotDefeatedRule(), (tracker, result) =>
            {
                var dungeon = GetBossDungeonFromResult(tracker, result);
                if (dungeon != null)
                {
                    // Untrack boss with associated dungeon
                    tracker.MarkDungeonAsIncomplete(dungeon, result.Confidence);
                    return;
                }

                var boss = GetBossFromResult(tracker, result);
                if (boss != null)
                {
                    // Untrack standalone boss
                    tracker.MarkBossAsNotDefeated(boss, result.Confidence);
                    return;
                }

                throw new Exception($"Could not find boss or dungeon in command: '{result.Text}'");
            });

            AddCommand("Mark boss as defeated with content", GetBossDefeatedWithContentRule(), (tracker, result) =>
            {
                var contentItemData = itemService.FirstOrDefault("Content");

                var dungeon = GetBossDungeonFromResult(tracker, result);
                if (dungeon != null)
                {
                    if (contentItemData != null)
                    {
                        Tracker.Say(x => x.DungeonBossClearedAddContent);
                        Tracker.TrackItem(contentItemData);
                    }

                    // Track boss with associated dungeon
                    tracker.MarkDungeonAsCleared(dungeon, result.Confidence);
                    return;
                }

                var boss = GetBossFromResult(tracker, result);
                if (boss != null)
                {
                    if (contentItemData != null)
                    {
                        Tracker.Say(x => x.DungeonBossClearedAddContent);
                        Tracker.TrackItem(contentItemData);
                    }

                    // Track standalone boss
                    var admittedGuilt = result.Text.ContainsAny("killed", "beat", "defeated", "dead")
                        && !result.Text.ContainsAny("beat off", "beaten off");
                    tracker.MarkBossAsDefeated(boss, admittedGuilt, result.Confidence);
                    return;
                }

                throw new Exception($"Could not find boss or dungeon in command: '{result.Text}'");
            });
        }

        private GrammarBuilder GetMarkBossAsDefeatedRule()
        {
            var bossNames = GetBossNames();
            var beatBoss = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("track", "I beat", "I defeated", "I beat off", "I killed")
                .Append(BossKey, bossNames);

            var markBoss = new GrammarBuilder()
                .Append("Hey tracker,")
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

        private GrammarBuilder GetMarkBossAsNotDefeatedRule()
        {
            var bossNames = GetBossNames();

            var markBoss = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("mark")
                .Append(BossKey, bossNames)
                .Append("as")
                .OneOf("alive", "not defeated", "undefeated");

            var bossIsAlive = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append(BossKey, bossNames)
                .OneOf("is alive", "is still alive");

            return GrammarBuilder.Combine(markBoss, bossIsAlive);
        }

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
    }
}
