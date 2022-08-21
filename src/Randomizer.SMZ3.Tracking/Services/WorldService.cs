using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.SMZ3.Tracking.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Service for retrieving information about the current state of
    /// the world
    /// </summary>
    public class WorldService : IWorldService
    {
        protected IReadOnlyCollection<RegionInfo> _regions;
        protected IReadOnlyCollection<DungeonInfo> _dungeons;
        protected IReadOnlyCollection<RoomInfo> _rooms;
        protected IReadOnlyCollection<LocationInfo> _locations;
        protected IReadOnlyCollection<BossInfo> _bosses;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="regions">Config with additional region information</param>
        /// <param name="dungeons">Config with additional dungeon information</param>
        /// <param name="rooms">Config with additional room information</param>
        /// <param name="locations">Config with additional location information</param>
        /// <param name="bosses">Config with additional boss information</param>
        public WorldService(RegionConfig regions, DungeonConfig dungeons, RoomConfig rooms, LocationConfig locations, BossConfig bosses)
        {
            _regions = regions;
            _dungeons = dungeons;
            _rooms = rooms;
            _locations = locations;
            _bosses = bosses;
        }

        /// <summary>
        /// Collection of all additional region information
        /// </summary>
        public IReadOnlyCollection<RegionInfo> Regions => _regions;

        /// <summary>
        /// Collection of all additional dungeon information
        /// </summary>
        public IReadOnlyCollection<DungeonInfo> Dungeons => _dungeons;

        /// <summary>
        /// Collection of all additional room information
        /// </summary>
        public IReadOnlyCollection<RoomInfo> Rooms => _rooms;

        /// <summary>
        /// Collection of all additional location information
        /// </summary>
        public IReadOnlyCollection<LocationInfo> Locations => _locations;

        /// <summary>
        /// Collection of all additional boss information
        /// </summary>
        public IReadOnlyCollection<BossInfo> Bosses => _bosses;

        /// <summary>
        /// Returns extra information for the specified region.
        /// </summary>
        /// <param name="name">
        /// The name or fully qualified type name of the region.
        /// </param>
        /// <returns>
        /// A new <see cref="RegionInfo"/> for the specified region.
        /// </returns>
        public RegionInfo Region(string name)
            => Regions.Single(x => x.Type.FullName == name || x.Region == name);

        /// <summary>
        /// Returns extra information for the specified region.
        /// </summary>
        /// <param name="type">
        /// The Randomizer.SMZ3 type matching the region.
        /// </param>
        /// <returns>
        /// A new <see cref="RegionInfo"/> for the specified region.
        /// </returns>
        public RegionInfo Region(Type type)
            => Regions.Single(x => x.Type == type);

        /// <summary>
        /// Returns extra information for the specified region.
        /// </summary>
        /// <param name="region">The region to get extra information for.</param>
        /// <returns>
        /// A new <see cref="RegionInfo"/> for the specified region.
        /// </returns>
        public RegionInfo Region(Region region)
            => Region(region.GetType());

        /// <summary>
        /// Returns extra information for the specified region.
        /// </summary>
        /// <typeparam name="TRegion">
        /// The type of region to get extra information for.
        /// </typeparam>
        /// <returns>
        /// A new <see cref="RegionInfo"/> for the specified region.
        /// </returns>
        public RegionInfo Region<TRegion>() where TRegion : Region
            => Region(typeof(TRegion));

        /// <summary>
        /// Returns extra information for the specified dungeon.
        /// </summary>
        /// <param name="name">
        /// The name or fully qualified type name of the dungeon region.
        /// </param>
        /// <returns>
        /// A new <see cref="DungeonInfo"/> for the specified dungeon region, or
        /// <c>null</c> if <paramref name="typeName"/> is not a valid dungeon.
        /// </returns>
        public DungeonInfo? Dungeon(string name)
            => Dungeons.SingleOrDefault(x => x.Type.FullName == name || x.Dungeon == name);

        /// <summary>
        /// Returns extra information for the specified dungeon.
        /// </summary>
        /// <param name="type">
        /// The type of dungeon to be looked up
        /// </param>
        /// <returns>
        /// A new <see cref="DungeonInfo"/> for the specified dungeon region, or
        /// <c>null</c> if <paramref name="type"/> is not a valid dungeon.
        /// </returns>
        public DungeonInfo Dungeon(Type type)
            => Dungeons.Single(x => type == x.Type);

        /// <summary>
        /// Returns extra information for the specified dungeon.
        /// </summary>
        /// <param name="region">
        /// The dungeon region to get extra information for.
        /// </param>
        /// <returns>
        /// A new <see cref="DungeonInfo"/> for the specified dungeon region.
        /// </returns>
        public DungeonInfo Dungeon(Region region)
            => Dungeon(region.GetType());

        /// <summary>
        /// Returns extra information for the specified dungeon.
        /// </summary>
        /// <typeparam name="TRegion">
        /// The type of region that represents the dungeon to get extra
        /// information for.
        /// </typeparam>
        /// <returns>
        /// A new <see cref="DungeonInfo"/> for the specified dungeon region.
        /// </returns>
        public DungeonInfo Dungeon<TRegion>() where TRegion : Region
            => Dungeon(typeof(TRegion));

        /// <summary>
        /// Returns extra information for the specified room.
        /// </summary>
        /// <param name="name">
        /// The name or fully qualified type name of the room.
        /// </param>
        /// <returns>
        /// A new <see cref="RoomInfo"/> for the specified room.
        /// </returns>
        public RoomInfo Room(string name)
            => Rooms.Single(x => x.Type.FullName == name || x.Room == name);

        /// <summary>
        /// Returns extra information for the specified room.
        /// </summary>
        /// <param name="type">
        /// The type of the room.
        /// </param>
        /// <returns>
        /// A new <see cref="RoomInfo"/> for the specified room.
        /// </returns>
        public RoomInfo Room(Type type)
            => Rooms.Single(x => x.Type == type);

        /// <summary>
        /// Returns extra information for the specified room.
        /// </summary>
        /// <param name="room">The room to get extra information for.</param>
        /// <returns>
        /// A new <see cref="RoomInfo"/> for the specified room.
        /// </returns>
        public RoomInfo Room(Room room)
            => Room(room.GetType());

        /// <summary>
        /// Returns extra information for the specified room.
        /// </summary>
        /// <typeparam name="TRoom">
        /// The type of room to get extra information for.
        /// </typeparam>
        /// <returns>
        /// A new <see cref="RoomInfo"/> for the specified room.
        /// </returns>
        public RoomInfo Room<TRoom>() where TRoom : Room
            => Room(typeof(TRoom));

        /// <summary>
        /// Returns extra information for the specified location.
        /// </summary>
        /// <param name="id">The numeric ID of the location.</param>
        /// <returns>
        /// A new <see cref="LocationInfo"/> for the specified room.
        /// </returns>
        public LocationInfo Location(int id)
            => Locations.Single(x => x.LocationNumber == id);

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
            => Locations.Single(x => x.LocationNumber == location.Id);

        /// <summary>
        /// Returns information about a specified boss
        /// </summary>
        /// <param name="name">The name of the boss</param>
        /// <returns>The <see cref="BossInfo"/> for the specified boss.</returns>
        public BossInfo Boss(string name)
            => Bosses.Single(x => x.Boss == name);
    }
}
