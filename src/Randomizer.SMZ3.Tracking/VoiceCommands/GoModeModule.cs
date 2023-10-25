using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for turning on Go Mode.
    /// </summary>
    public class GoModeModule : TrackerModule
    {
        private ResponseConfig _responseConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoModeModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="itemService">Service to get item information</param>
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger">Used to log information.</param>
        /// <param name="responseConfig"></param>
        public GoModeModule(TrackerBase tracker, IItemService itemService, IWorldService worldService, ILogger<GoModeModule> logger, ResponseConfig responseConfig)
            : base(tracker, itemService, worldService, logger)
        {
            _responseConfig = responseConfig;
        }

        private GrammarBuilder GetGoModeRule(List<string> prompts)
        {
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf(prompts.ToArray());
        }

        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
        public override void AddCommands()
        {
            AddCommand("Toggle Go Mode", GetGoModeRule(_responseConfig.GoModePrompts), (result) =>
            {
                TrackerBase.ToggleGoMode(result.Confidence);
            });
        }
    }
}
