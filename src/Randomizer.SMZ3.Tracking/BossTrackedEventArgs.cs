using System;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides data for events that occur when clearing a location.
    /// </summary>
    public class BossTrackedEventArgs : TrackerEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="BossTrackedEventArgs"/> class.
        /// </summary>
        /// <param name="boss">The boss that was tracked.</param>
        /// <param name="confidence">The speech recognition confidence.</param>
        /// <param name="autoTracked">If the location was automatically tracked</param>
        public BossTrackedEventArgs(Boss? boss, float? confidence, bool autoTracked)
            : base(confidence, autoTracked)
        {
            Boss = boss;
        }

        /// <summary>
        /// Gets the boss that was tracked.
        /// </summary>
        public Boss? Boss { get; }
    }
}
