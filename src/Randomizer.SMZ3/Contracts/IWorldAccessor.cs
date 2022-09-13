using System;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Contracts
{
    /// <summary>
    /// Provides access to the current <see cref="SMZ3.World"/>.
    /// </summary>
    public interface IWorldAccessor
    {
        /// <summary>
        /// Gets or sets the current <see cref="SMZ3.World"/>, if one is
        /// available.
        /// </summary>
        World World { get; set; }
    }
}
