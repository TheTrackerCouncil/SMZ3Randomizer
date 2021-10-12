using System;

namespace Randomizer.SMZ3.Tracking.Vocabulary
{
    public class ResponseConfig
    {
        public PhraseSet? StartedTracking { get; init; }

        public PhraseSet? StoppedTracking { get; init; }

        public PhraseSet TrackedItem { get; init; }
            = new PhraseSet("Toggled {0} on");

        /// <summary>
        /// Gets the phrases to respond with when tracking a progressive item
        /// using a specific stage name.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </remarks>
        public PhraseSet TrackedItemByStage { get; init; }
            = new PhraseSet("Marked {0} as {1}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that can be
        /// tracked multiple times.
        /// </summary>
        public PhraseSet TrackedItemMultiple { get; init; }
             = new PhraseSet("Added {0}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a progressive item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </remarks>
        public PhraseSet TrackedProgressiveItem { get; init; }
            = new PhraseSet("Upgraded {0} by one step.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a lower tier of an
        /// item than you already have, e.g. tracking Master Sword when you have
        /// the Tempered Sword.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </remarks>
        public PhraseSet? TrackedOlderProgressiveItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that is
        /// already at the max tier.
        /// </summary>
        public PhraseSet? TrackedTooManyOfAnItem { get; init; }

        public PhraseSet? TrackedAlreadyTrackedItem { get; init; }

        public PhraseSet? ShaktoolAvailable { get; init; }

        public PhraseSet? PegWorldAvailable { get; init; }
    }
}
