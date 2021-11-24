using System;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides Tracker preferences.
    /// </summary>
    public record TrackerOptions
    {
        /// <summary>
        /// Gets or sets the minimum confidence level for Tracker to recognize
        /// voice commands.
        /// </summary>
        /// <remarks>
        /// Recognized speech below this threshold will not be executed.
        /// </remarks>
        public float MinimumRecognitionConfidence { get; set; } = 0.75f;

        /// <summary>
        /// Gets or sets the minimum confidence level for Tracker to execute to
        /// voice commands.
        /// </summary>
        /// <remarks>
        /// Recognized speech below this threshold will not be executed, but may
        /// prompt Tracker to ask to repeat.
        /// </remarks>
        public float MinimumExecutionConfidence { get; set; } = 0.85f;

        /// <summary>
        /// Gets or sets the minimum confidence level for Tracker to use sassy
        /// responses or responses that could potentially give away information.
        /// </summary>
        public float MinimumSassConfidence { get; set; } = 0.92f;

        /// <summary>
        /// Gets or sets a value indicating whether Tracker can give hints when
        /// asked about an item or location.
        /// </summary>
        public bool HintsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Tracker can give spoilers
        /// when asked about an item or location.
        /// </summary>
        public bool SpoilersEnabled { get; set; }
    }
}
