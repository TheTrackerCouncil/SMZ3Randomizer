using System;
using System.Collections.Generic;

namespace Randomizer.Data.Configuration.ConfigTypes
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
        public TrackerMapRegion(string name, IReadOnlyCollection<TrackerMapLocation> rooms, IReadOnlyCollection<TrackerMapSMDoor> doors, string typeName, int? bossX, int? bossY)
        {
            Name = name;
            Rooms = rooms;
            Doors = doors;
            TypeName = typeName;
            BossX = bossX;
            BossY = bossY;
        }

        /// <summary>
        /// Gets the name of the region. This should match a <see
        /// cref="Region.Name"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Full class type name of the region
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// The location of the boss, if there is one
        /// </summary>
        public int? BossX { get; } = null;

        /// <summary>
        /// The location of the boss, if there is one
        /// </summary>
        public int? BossY { get; } = null;

        /// <summary>
        /// Gets a collection of map locations that belong to this region.
        /// </summary>
        public IReadOnlyCollection<TrackerMapLocation>? Rooms { get; }

        /// <summary>
        /// List of Super Metroid doors for Keysanity
        /// </summary>
        public IReadOnlyCollection<TrackerMapSMDoor>? Doors { get; }
    }
}
