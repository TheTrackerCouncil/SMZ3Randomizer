using System;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides Tracker preferences.
    /// </summary>
    public record TrackerOptions
    {
        /// <summary>
        /// Gets or sets the minimum confidence level for Tracker to react to
        /// voice commands.
        /// </summary>
        public float MinimumConfidence { get; set; } = 0.75f;

        /// <summary>
        /// Gets or sets the minimum confidence level for Tracker to use sassy
        /// responses or responses that could potentially give away information.
        /// </summary>
        public float MinimumSassConfidence { get; set; } = 0.90f;
    }
}
