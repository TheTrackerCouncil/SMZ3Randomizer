using System;

namespace Randomizer.SMZ3.Tracking.Vocabulary
{
    public class ResponseConfig
    {
        /// <summary>
        /// Gets the phrases to respond with when tracker starts.
        /// </summary>
        public SchrodingersString? StartedTracking { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracker stops.
        /// </summary>
        public SchrodingersString? StoppedTracking { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking a simple single-stage
        /// item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item.
        /// </remarks>
        public SchrodingersString TrackedItem { get; init; }
            = new SchrodingersString("Toggled {0} on");

        /// <summary>
        /// Gets the phrases to respond with when tracking a progressive item
        /// using a specific stage name.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </remarks>
        public SchrodingersString TrackedItemByStage { get; init; }
            = new SchrodingersString("Marked {0} as {1}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that can be
        /// tracked multiple times.
        /// </summary>
        public SchrodingersString TrackedItemMultiple { get; init; }
             = new SchrodingersString("Added {0}.");

        /// <summary>
        /// Gets the phrases to respond with when tracking a progressive item.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the base item name (e.g. Sword),
        /// <c>{1}</c> is a placeholder for the current stage of the progressive
        /// item (e.g. Master Sword).
        /// </remarks>
        public SchrodingersString TrackedProgressiveItem { get; init; }
            = new SchrodingersString("Upgraded {0} by one step.");

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
        public SchrodingersString? TrackedOlderProgressiveItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking an item that is
        /// already at the max tier.
        /// </summary>
        public SchrodingersString? TrackedTooManyOfAnItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when tracking a single-stage item
        /// that is already tracked.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the item.
        /// </remarks>
        public SchrodingersString? TrackedAlreadyTrackedItem { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when Shaktool becomes available.
        /// </summary>
        public SchrodingersString? ShaktoolAvailable { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when Peg World becomes available.
        /// </summary>
        public SchrodingersString? PegWorldAvailable { get; init; }
    }
}
