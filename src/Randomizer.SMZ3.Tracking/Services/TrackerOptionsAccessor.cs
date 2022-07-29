using System;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Provides access to the currently active tracking preferences.
    /// </summary>
    public class TrackerOptionsAccessor
    {
        /// <summary>
        /// Gets or sets the currently active tracking preferences, or <see
        /// langword="null"/> if the options have not yet been activated.
        /// </summary>
        public TrackerOptions? Options { get; set; }
    }
}
