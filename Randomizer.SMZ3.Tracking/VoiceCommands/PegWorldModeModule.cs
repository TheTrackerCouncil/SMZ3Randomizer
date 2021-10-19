using System;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for Peg World mode.
    /// </summary>
    public class PegWorldModeModule : TrackerModule, IOptionalModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PegWorldModeModule"/>
        /// class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to log information.</param>
        public PegWorldModeModule(Tracker tracker, ILogger<PegWorldModeModule> logger) : base(tracker, logger)
        {
            AddCommand("Toggle Peg World mode", new[] {
                "Hey tracker, we're going to Peg World!",
                "Hey tracker, let's go to Peg World!"
            }, (tracker, result) =>
            {
                tracker.StartPegWorldMode(result.Confidence);
            });

            AddCommand("Track Peg World peg", new[] {
                "Hey tracker, track Peg.",
                "Hey tracker, peg."
            }, (tracker, result) =>
            {
                var peg = tracker.Pegs.FirstOrDefault(x => !x.Pegged);
                if (peg != null)
                {
                    tracker.Peg(peg, result.Confidence);
                }
            });
        }
    }
}
