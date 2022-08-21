using System;
using System.Linq;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for Peg World mode.
    /// </summary>
    public class PegWorldModeModule : TrackerModule, IOptionalModule
    {
        public static readonly int TotalPegs = 22;

        /// <summary>
        /// Initializes a new instance of the <see cref="PegWorldModeModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to log information.</param>
        public PegWorldModeModule(Tracker tracker, IItemService itemService, ILogger<PegWorldModeModule> logger)
            : base(tracker, itemService, logger)
        {
            AddCommand("Toggle Peg World mode on", new[] {
                "Hey tracker, toggle Peg World Mode on",
                "Hey tracker, we're going to Peg World!",
                "Hey tracker, let's go to Peg World!"
            }, (tracker, result) =>
            {
                tracker.StartPegWorldMode(result.Confidence);
            });

            AddCommand("Toggle Peg World mode off", new[] {
                "Hey tracker, toggle Peg World Mode off",
                "Hey tracker, I don't want to be at Peg World anymore",
                "Hey tracker, I want to go on something more thrilling than Peg World",
                "Hey tracker, please release me from Peg World",
                "Hey tracker, release me from Peg World"
            }, (tracker, result) =>
            {
                tracker.StopPegWorldMode(result.Confidence);
            });

            AddCommand("Track Peg World peg", new[] {
                "Hey tracker, track Peg.",
                "Hey tracker, peg."
            }, (tracker, result) =>
            {
                if (tracker.PegsPegged < TotalPegs)
                {
                    tracker.Peg(result.Confidence);
                }
            });
        }
    }
}
