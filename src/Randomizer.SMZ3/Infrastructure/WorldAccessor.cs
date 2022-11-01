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
        private World? _world;
        private List<World>? _worlds;

        /// <summary>
        /// Gets or sets the current <see cref="SMZ3.World"/>, if one is
        /// available.
        /// </summary>
        public World World
        {
            get => _world ?? throw new InvalidOperationException("World Accessor does not have world set");
            set
            {
                _world = value;
            }
        }

        /// <summary>
        /// List of all of the worlds
        /// </summary>
        public List<World> Worlds
        {
            get => _worlds ?? throw new InvalidOperationException("World Accessor does not have worlds set");
            set
            {
                _worlds = value;
            }
        }
    }
}
