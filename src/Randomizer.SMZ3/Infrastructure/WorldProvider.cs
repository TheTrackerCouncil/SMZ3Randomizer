using System;

using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Infrastructure
{
    /// <summary>
    /// Provides access to the current <see cref="SMZ3.World"/>.
    /// </summary>
    public class WorldAccessor : IWorldAccessor
    {
        /// <inheritdoc/>
        public World World { get; set; }
    }
}
