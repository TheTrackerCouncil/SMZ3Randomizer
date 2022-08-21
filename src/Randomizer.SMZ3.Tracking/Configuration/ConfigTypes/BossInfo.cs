using System;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
{
    /// <summary>
    /// Represents a boss whose defeat can be tracked.
    /// </summary>
    /// <remarks>
    /// This class is typically only used for tracking bosses not already
    /// represented by <see cref="DungeonInfo"/>, e.g. Metroid bosses.
    /// </remarks>
    public class BossInfo : IMergeable<BossInfo>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BossInfo() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BossInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the boss.</param>
        public BossInfo(SchrodingersString name)
        {
            Name = name;
        }

        /// <summary>
        /// The identifier for merging configs
        /// </summary>
        [MergeKey]
        public string Boss { get; set; }

        /// <summary>
        /// Gets the name of the boss.
        /// </summary>
        public SchrodingersString Name { get; set; }

        /// <summary>
        /// Gets the phrases to respond with when the boss has been tracked (but
        /// not necessarily killed).
        /// </summary>
        public SchrodingersString? WhenTracked { get; set; }

        /// <summary>
        /// Gets the phrases to respond with when the boss has been defeated.
        /// </summary>
        public SchrodingersString? WhenDefeated { get; set; }

        /// <summary>
        /// Gets or sets the path to the image to be displayed on the tracker to
        /// represent the boss.
        /// </summary>
        public string? Image { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether the boss has been defeated.
        /// </summary>
        public bool Defeated { get; set; }

        /// <summary>
        /// Gets or sets the reward for when the boss is defeated
        /// </summary>
        public RewardType Reward { get; set; }

        /// <summary>
        /// Returns a string representation of the boss.
        /// </summary>
        /// <returns>A string representing this boss.</returns>
        public override string? ToString() => Name[0];
    }
}
