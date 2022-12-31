using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class EasternPalace : Z3Region, IHasReward, IDungeon
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D59A
        };

        public EasternPalace(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            RegionItems = new[] { ItemType.BigKeyEP, ItemType.MapEP, ItemType.CompassEP };

            CannonballChest = new Location(this, 256 + 103, 0x1E9B3, LocationType.Regular,
                name: "Cannonball Chest",
                vanillaItem: ItemType.OneHundredRupees,
                memoryAddress: 0xB9,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            MapChest = new Location(this, 256 + 104, 0x1E9F5, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapEP,
                memoryAddress: 0xAA,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            CompassChest = new Location(this, 256 + 105, 0x1E977, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassEP,
                memoryAddress: 0xA8,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            BigChest = new Location(this, 256 + 106, 0x1E97D, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Bow,
                access: items => items.BigKeyEP,
                memoryAddress: 0xA9,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            BigKeyChest = new Location(this, 256 + 107, 0x1E9B9, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyEP,
                access: items => Logic.CanPassSwordOnlyDarkRooms(items),
                memoryAddress: 0xB8,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            ArmosKnightsRewards = new Location(this, 256 + 108, 0x308150, LocationType.Regular,
                name: "Armos Knights",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyEP && items.Bow && Logic.CanPassFireRodDarkRooms(items),
                memoryAddress: 0xC8,
                memoryFlag: 0xB,
                metadata: metadata,
                trackerState: trackerState);

            MemoryAddress = 0xC8;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 201 };
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Eastern Palace");
            DungeonMetadata = metadata?.Dungeon(GetType()) ?? new DungeonInfo("Eastern Palace", "EP", "Armos Knights");
            DungeonState = trackerState?.DungeonStates.First(x => x.WorldId == world.Id && x.Name == GetType().Name) ?? new TrackerDungeonState();
            Reward = new Reward(DungeonState.Reward ?? RewardType.None, world, this, metadata, DungeonState);
        }

        public override string Name => "Eastern Palace";

        public Reward Reward { get; set; }

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public int SongIndex { get; init; } = 0;

        public Region ParentRegion => World.LightWorldNorthEast;

        public Location CannonballChest { get; }

        public Location MapChest { get; }

        public Location CompassChest { get; }

        public Location BigChest { get; }

        public Location BigKeyChest { get; }

        public Location ArmosKnightsRewards { get; }

        public bool CanComplete(Progression items)
            => ArmosKnightsRewards.IsAvailable(items);
    }
}
