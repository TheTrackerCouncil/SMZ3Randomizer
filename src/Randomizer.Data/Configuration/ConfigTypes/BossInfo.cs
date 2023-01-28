using System;
using Randomizer.Shared;
using Randomizer.Shared.Enums;

namespace Randomizer.Data.Configuration.ConfigTypes
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
        /// Initializes a new instance of the <see cref="BossInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the boss.</param>
        public BossInfo(string name)
        {
            Name = new SchrodingersString(name);
        }

        /// <summary>
        /// The identifier for merging configs
        /// </summary>
        [MergeKey]
        public string Boss { get; set; } = "";

        /// <summary>
        /// Gets the name of the boss.
        /// </summary>
        public SchrodingersString Name { get; set; } = new();

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

        public BossType Type { get; set; }

        /// <summary>
        /// Memory offset for detecting if this boss was defeated
        /// </summary>
        public int? MemoryAddress { get; set; }

        /// <summary>
        /// Bit to check to determine if this boss was defeated
        /// </summary>
        public int? MemoryFlag { get; set; }

        /// <summary>
        /// Returns a string representation of the boss.
        /// </summary>
        /// <returns>A string representing this boss.</returns>
        public override string? ToString() => Name[0];
    }
}
