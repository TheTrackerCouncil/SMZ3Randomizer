using System;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Contains event data for item tracking events.
    /// </summary>
    public class ItemTrackedEventArgs : TrackerEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemTrackedEventArgs"/>
        /// class.
        /// </summary>
        /// <param name="item">The item that was tracked or untracked</param>
        /// <param name="trackedAs">
        /// The name of the item that was tracked.
        /// </param>
        /// <param name="confidence">The speech recognition confidence.</param>
        /// <param name="autoTracked">If the item was auto tracked</param>
        public ItemTrackedEventArgs(Item? item, string? trackedAs, float? confidence, bool autoTracked)
            : base(confidence, autoTracked)
        {
            Item = item;
            TrackedAs = trackedAs;
        }

        /// <summary>
        /// Gets the name of the item as it was tracked.
        /// </summary>
        public string? TrackedAs { get; }

        /// <summary>
        /// The item that was tracked or untracked
        /// </summary>
        public Item? Item { get; }
    }
}
