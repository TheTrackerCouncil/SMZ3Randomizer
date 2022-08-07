using System;
using System.Collections.Generic;

using Randomizer.Shared;

namespace Randomizer.SMZ3.Generation
{
    /// <summary>
    /// Represents the configuration for a plandomizer world.
    /// </summary>
    public class PlandoConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether Keysanity should be enabled.
        /// </summary>
        public bool Keysanity { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that contains the names of locations and
        /// the types of items they should be filled with.
        /// </summary>
        public Dictionary<string, ItemType> Items { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that contains the names of regions and the
        /// boss rewards they should be filled with.
        /// </summary>
        public Dictionary<string, RewardType> Rewards { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that contains the names of regions and the
        /// medallions they require.
        /// </summary>
        public Dictionary<string, Medallion> Medallions { get; set; }

        /// <summary>
        /// Gets or sets the logic options that apply to the plando.
        /// </summary>
        public LogicConfig Logic { get; set; }
    }
}
