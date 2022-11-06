using System;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands for undoing things.
    /// </summary>
    public class UndoModule : TrackerModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UndoModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="itemService">Service to get item information</param>
        /// <param name="worldService">Service to get world information</param>
        /// <param name="logger">Used to log information.</param>
        public UndoModule(Tracker tracker, IItemService itemService, IWorldService worldService, ILogger<UndoModule> logger)
            : base(tracker, itemService, worldService, logger)
        {
            AddCommand("Undo last operation", GetUndoRule(), (tracker, result) =>
            {
                tracker.Undo(result.Confidence);
            });
        }

        private GrammarBuilder GetUndoRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("undo that", "control Z", "that's not what I said", "take backsies");
        }
    }
}
