using System;

namespace Randomizer.SMZ3
{
    /// <summary>
    /// Defines a method for retrieving the <see cref="World"/> to track.
    /// </summary>
    public interface IWorldAccessor
    {
        /// <summary>
        /// Returns the <see cref="World"/> instance to track.
        /// </summary>
        /// <returns>The <see cref="World"/> instance to track.</returns>
        World GetWorld();
    }
}
