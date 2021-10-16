using System;
using System.Collections.Generic;

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
        /// Gets the phrases to respond with when tracking a specific item.
        /// </summary>
        public Dictionary<string, SchrodingersString> TrackedSpecificItem { get; init; } = new();

        /// <summary>
        /// Gets the phrases to respond with when Shaktool becomes available.
        /// </summary>
        public SchrodingersString? ShaktoolAvailable { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when Peg World becomes available.
        /// </summary>
        public SchrodingersString? PegWorldAvailable { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when Peg World mode is toggled on.
        /// </summary>
        public SchrodingersString? PegWorldModeOn { get; init; }

        public SchrodingersString? PegWorldModePegged { get; init; }

        public SchrodingersString? PegWorldModeDone { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when marking the reward at a
        /// dungeon.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon. <c>{1}</c>
        /// is a placeholder for the name of the reward that was marked.
        /// </remarks>
        public SchrodingersString DungeonRewardMarked { get; init; }
            = new SchrodingersString("Marked {0} as {1}.");

        /// <summary>
        /// Gets the phrases to respond with when clearing a dungeon.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon that was
        /// cleared. <c>{1}</c> is a placeholder for the boss of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonCleared { get; init; }
            = new SchrodingersString("Cleared {0}.", "Marked {1} as defeated.");

        /// <summary>
        /// Gets the phrases to respond with when marking the medallion
        /// requirement of a dungeon.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the medallion.
        /// <c>{1}</c> is a placeholder for the name of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonRequirementMarked { get; init; }
            = new SchrodingersString("Marked {0} as required for {1}");

        /// <summary>
        /// Gets the phrases to respond with when marking the wrong dungeon with
        /// a required medallion.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the dungeon.
        /// </remarks>
        public SchrodingersString DungeonRequirementInvalid { get; init; }
            = new SchrodingersString("{0} doesn't need medallions");

        /// <summary>
        /// Gets the phrases to respond with when marking a dungeon with a
        /// requirement that doesn't match the seed logic.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the name of the medallion in the seed.
        /// <c>{1}</c> is a placeholder for the name of the dungeon.
        /// <c>{2}</c> is a placeholder for the name of the medallion that was tracked.
        /// </remarks>
        public SchrodingersString? DungeonRequirementMismatch { get; init; }

        /// <summary>
        /// Gets a dictionary that contains the phrases to respond with when no
        /// voice commands have been issued after a certain period of time, as
        /// expressed in the dictionary keys.
        /// </summary>
        public Dictionary<string, SchrodingersString> Idle { get; init; } = new();
    }
}
