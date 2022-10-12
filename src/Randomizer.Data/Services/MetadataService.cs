using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Microsoft.Extensions.Logging;
using Randomizer.Shared.Enums;
using Randomizer.Shared;
using Randomizer.Data.Configuration;

namespace Randomizer.Data.Services
{
    /// <summary>
    /// Service for retrieving additional metadata information
    /// about objects and locations within the world
    /// </summary>
    public class MetadataService : IMetadataService
    {
        protected IReadOnlyCollection<RegionInfo> _regions;
        protected IReadOnlyCollection<DungeonInfo> _dungeons;
        protected IReadOnlyCollection<RoomInfo> _rooms;
        protected IReadOnlyCollection<LocationInfo> _locations;
        protected IReadOnlyCollection<BossInfo> _bosses;
        protected IReadOnlyCollection<ItemData> _items;
        protected IReadOnlyCollection<RewardInfo> _rewards;
        private readonly ILogger<MetadataService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configs">All configs</param>
        /// <param name="logger"></param>
        public MetadataService(Configs configs, ILogger<MetadataService> logger)
        {
            _regions = configs.Regions;
            _dungeons = configs.Dungeons;
            _rooms = configs.Rooms;
            _locations = configs.Locations;
            _bosses = configs.Bosses;
            _items = configs.Items;
            _rewards = configs.Rewards;
            _logger = logger;
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
        /// Collection of all additional boss information
        /// </summary>
        public IReadOnlyCollection<ItemData> Items => _items;

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
            => Regions.Single(x => x.Type?.FullName == name || x.Region == name);

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
        /// <c>null</c> if <paramref name="name"/> is not a valid dungeon.
        /// </returns>
        public DungeonInfo? Dungeon(string name)
            => Dungeons.SingleOrDefault(x => x.Type?.FullName == name || x.Dungeon == name);

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
        /// Returns extra information for the specified dungeon.
        /// </summary>
        /// <param name="dungeon">
        /// The dungeon to get extra information for.
        /// </param>
        /// <returns>
        /// A new <see cref="DungeonInfo"/> for the specified dungeon region.
        /// </returns>
        public DungeonInfo Dungeon(IDungeon dungeon)
            => Dungeon(dungeon.GetType());

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
            => Rooms.Single(x => x.Type?.FullName == name || x.Room == name);

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

        /// <summary>
        /// Returns information about a specified boss
        /// </summary>
        /// <param name="boss">The type of the boss</param>
        /// <returns>The <see cref="BossInfo"/> for the specified boss.</returns>
        public BossInfo Boss(BossType boss)
            => Bosses.Single(x => x.Type == boss);

        /// <summary>
        /// Returns information about a specified item
        /// </summary>
        /// <param name="type">The type of the item</param>
        /// <returns></returns>
        public ItemData? Item(ItemType type)
            => Items.FirstOrDefault(x => x.InternalItemType == type);

        /// <summary>
        /// Applies various metadata to the world, such as LocationData and ItemData
        /// </summary>
        /// <param name="world">The world to apply metadata to</param>
        public void LoadWorldMetadata(World world)
        {
            world.Locations.ToList().ForEach(loc => loc.Metadata = Location(loc.Id));
            world.Dungeons.ToList().ForEach(d => d.DungeonMetadata = Dungeon(d));
            world.Rewards.ToList().ForEach(r => r.Metadata = _rewards.First(md => md.RewardType == r.Type));
            world.Regions.ToList().ForEach(r => r.Metadata = _regions.First(md => md.Type == r.GetType()));
            world.Rooms.ToList().ForEach(r => r.Metadata = _rooms.First(md => md.Type == r.GetType()));

            foreach (var (metadata, worldItems) in _items.Select(x => (item: x, worldItems: world.AllItems.Where(w => w.Is(x.InternalItemType, x.Item)))))
            {
                if (worldItems.Any())
                    worldItems.ToList().ForEach(x => x.Metadata = metadata);
                else
                {
                    _logger.LogInformation($"No data found {metadata.Item}");
                    var item = new Item(metadata.InternalItemType, world, metadata.Item)
                    {
                        State = new()
                        {
                            ItemName = metadata.Item,
                            Type = metadata.InternalItemType,
                            TrackerState = world.State
                        },
                        Metadata = metadata
                    };
                    world.State.ItemStates.Add(item.State);
                    world.TrackerItems.Add(item);
                }
            }

            foreach (var (metadata, worldBoss) in _bosses.Select(md => (boss: md, worldBoss: world.AllBosses.FirstOrDefault(w => w.Is(md.Type, md.Boss)))))
            {
                if (worldBoss != null)
                    worldBoss.Metadata = metadata;
                else
                {
                    _logger.LogInformation($"No data found {metadata.Boss}");
                    var boss = new Boss(metadata.Type, world, metadata.Boss)
                    {
                        State = new()
                        {
                            BossName = metadata.Boss,
                            Type = metadata.Type,
                            TrackerState = world.State
                        },
                        Metadata = metadata
                    };
                    world.State.BossStates.Add(boss.State);
                    world.TrackerBosses.Add(boss);
                }
            }
        }
    }
}
