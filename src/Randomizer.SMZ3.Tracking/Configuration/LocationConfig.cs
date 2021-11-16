using System;
using System.Collections.Generic;
using System.Linq;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Provides configurable information about locations in SMZ3.
    /// </summary>
    public class LocationConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationConfig"/>
        /// class.
        /// </summary>
        /// <param name="regions">The region information.</param>
        /// <param name="rooms">The room information.</param>
        /// <param name="locations">The location information.</param>
        public LocationConfig(IReadOnlyCollection<RegionInfo> regions,
            IReadOnlyCollection<RoomInfo> rooms,
            IReadOnlyCollection<LocationInfo> locations)
        {
            Regions = regions;
            Rooms = rooms;
            Locations = locations;
        }

        /// <summary>
        /// Gets a collection of extra information about regions.
        /// </summary>
        public IReadOnlyCollection<RegionInfo> Regions { get; }

        /// <summary>
        /// Gets a collection of extra information about rooms.
        /// </summary>
        public IReadOnlyCollection<RoomInfo> Rooms { get; }

        /// <summary>
        /// Gets a collection of extra information about locations.
        /// </summary>
        public IReadOnlyCollection<LocationInfo> Locations { get; }

        /// <summary>
        /// Returns extra information for the specified region.
        /// </summary>
        /// <param name="region">The region to get extra information for.</param>
        /// <returns>
        /// A new <see cref="RegionInfo"/> for the specified region.
        /// </returns>
        public RegionInfo Region(Region region)
            => Regions.Single(x => x.TypeName == region.GetType().FullName);

        /// <summary>
        /// Returns extra information for the specified room.
        /// </summary>
        /// <param name="room">The room to get extra information for.</param>
        /// <returns>
        /// A new <see cref="RoomInfo"/> for the specified room.
        /// </returns>
        public RoomInfo Room(Room room)
            => Rooms.Single(x => x.TypeName == room.GetType().FullName);

        /// <summary>
        /// Returns extra information for the specified location.
        /// </summary>
        /// <param name="location">
        /// The location to get extra information for.
        /// </param>
        /// <returns>
        /// A new <see cref="LocationInfo"/> for the specified room.
        /// </returns>
        public LocationInfo Location(Location location)
            => Locations.Single(x => x.Id == location.Id);
    }
}
