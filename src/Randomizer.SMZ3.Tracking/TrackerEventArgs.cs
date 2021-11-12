using System;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Contains event data for tracking events.
    /// </summary>
    public class TrackerEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerEventArgs"/>
        /// class.
        /// </summary>
        /// <param name="confidence">The speech recognition confidence.</param>
        public TrackerEventArgs(float? confidence)
        {
            Confidence = confidence;
        }

        /// <summary>
        /// Gets the speech recognition confidence as a value between 0.0 and
        /// 1.0, or <c>null</c> if the event was not initiated by speech
        /// recognition.
        /// </summary>
        public float? Confidence { get; }
    }
}
