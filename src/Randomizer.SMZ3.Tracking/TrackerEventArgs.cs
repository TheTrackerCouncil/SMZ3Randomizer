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
        /// <param name="autoTracked">If the event was triggered by auto tracker</param>
        public TrackerEventArgs(float? confidence, bool autoTracked = false)
        {
            Confidence = confidence;
            AutoTracked = autoTracked;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerEventArgs"/>
        /// class.
        /// </summary>
        /// <param name="confidence">The speech recognition confidence.</param>
        /// <param name="phrase">The phrase that was recognized.</param>
        public TrackerEventArgs(float? confidence, string? phrase)
        {
            Confidence = confidence;
            Phrase = phrase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerEventArgs"/>
        /// class.
        /// </summary>
        /// <param name="autoTracked">If the event was triggered by auto tracker</param>
        public TrackerEventArgs(bool autoTracked)
        {
            AutoTracked = autoTracked;
        }

        /// <summary>
        /// Gets the speech recognition confidence as a value between 0.0 and
        /// 1.0, or <c>null</c> if the event was not initiated by speech
        /// recognition.
        /// </summary>
        public float? Confidence { get; }

        /// <summary>
        /// Gets the phrase Tracker recognized, or <c>null</c>.
        /// </summary>
        public string? Phrase { get; }

        /// <summary>
        /// If the event was triggered by auto tracker
        /// </summary>
        public bool AutoTracked { get; init; }
    }
}
