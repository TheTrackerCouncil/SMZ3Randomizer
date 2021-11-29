using System;

using Microsoft.Extensions.Logging;

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
        /// <param name="logger">Used to write logging information.</param>
        public BossTrackingModule(Tracker tracker, ILogger<BossTrackingModule> logger)
            : base(tracker, logger)
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
                    tracker.MarkBossAsDefeated(boss, result.Confidence);
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
        }

        private GrammarBuilder GetMarkBossAsDefeatedRule()
        {
            var bossNames = GetBossNames();
            var beatBoss = new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("track", "I beat", "I defeated", "I beat off")
                .Append(BossKey, bossNames);

            var markBoss = new GrammarBuilder()
                .Append("Hey tracker,")
                .Append("mark")
                .Append(BossKey, bossNames)
                .Append("as")
                .OneOf("beaten", "beaten off", "dead", "fucking dead");

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
    }
}
