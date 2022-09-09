using System.Collections.Generic;
using Randomizer.Data.WorldData;

namespace Randomizer.Data.WorldData.Regions
{
    /// <summary>
    /// Defines a named area that contains one or more locations.
    /// </summary>
    public interface IHasLocations
    {
        /// <summary>
        /// Gets the name of the area.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a collection of alternative names for the area.
        /// </summary>
        IReadOnlyCollection<string> AlsoKnownAs { get; }

        /// <summary>
        /// Gets all locations in the area.
        /// </summary>
        IEnumerable<Location> Locations { get; }
    }
}
