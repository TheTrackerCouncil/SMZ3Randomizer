using System;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for turning on Go Mode.
    /// </summary>
    public class GoModeModule : TrackerModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoModeModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="logger">Used to log information.</param>
        public GoModeModule(Tracker tracker, IItemService itemService, ILogger<GoModeModule> logger)
            : base(tracker, itemService, logger)
        {
            AddCommand("Toggle Go Mode", "Hey tracker, track Go Mode.", (tracker, result) =>
            {
                tracker.ToggleGoMode(result.Confidence);
            });
        }
    }
}
