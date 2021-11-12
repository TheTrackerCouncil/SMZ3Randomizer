using System;

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
        /// <param name="trackedAs">
        /// The name of the item that was tracked.
        /// </param>
        /// <param name="confidence">The speech recognition confidence.</param>
        public ItemTrackedEventArgs(string? trackedAs, float? confidence)
            : base(confidence)
        {
            TrackedAs = trackedAs;
        }

        /// <summary>
        /// Gets the name of the item as it was tracked.
        /// </summary>
        public string? TrackedAs { get; }
    }
}
