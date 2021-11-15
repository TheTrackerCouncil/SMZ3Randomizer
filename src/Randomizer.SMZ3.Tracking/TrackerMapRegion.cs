using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Represents a region on the map
    /// </summary>
    public class TrackerMapRegion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMapRegion"/>
        /// class with the specified region name and locations.
        /// </summary>
        /// <param name="name">
        /// The name of the region. Must match a <see cref="Region.Name"/>.
        /// </param>
        /// <param name="rooms">The rooms and locations in this region.</param>
        public TrackerMapRegion(string name, IReadOnlyCollection<TrackerMapLocation> rooms)
        {
            Name = name;
            Rooms = rooms;
        }

        /// <summary>
        /// Gets the name of the region. This should match a <see
        /// cref="Region.Name"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a collection of map locations that belong to this region.
        /// </summary>
        public IReadOnlyCollection<TrackerMapLocation> Rooms { get; }
    }
}
