using System;
using System.Collections.Generic;
using Randomizer.Data.WorldData;
using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Infrastructure
{
    /// <summary>
    /// Provides access to the current <see cref="SMZ3.World"/>.
    /// </summary>
    public class WorldAccessor : IWorldAccessor
    {
        /// <summary>
        /// Gets or sets the current <see cref="SMZ3.World"/>, if one is
        /// available.
        /// </summary>
        public World World { get; set; }

        /// <summary>
        /// List of all of the worlds
        /// </summary>
        public List<World> Worlds { get; set; }
    }
}
