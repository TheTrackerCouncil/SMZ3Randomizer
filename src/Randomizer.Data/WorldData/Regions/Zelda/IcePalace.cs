using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class IcePalace : Z3Region, IHasReward, IDungeon
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5BF
        };
        public IcePalace(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            RegionItems = new[] { ItemType.KeyIP, ItemType.BigKeyIP, ItemType.MapIP, ItemType.CompassIP };

            CompassChest = new Location(this, 256 + 161, 0x1E9D4, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassIP,
                memoryAddress: 0x2E,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            SpikeRoom = new Location(this, 256 + 162, 0x1E9E0, LocationType.Regular,
                name: "Spike Room",
                vanillaItem: ItemType.KeyIP,
                access: items => BigKeyChest != null && MapChest != null && (items.Hookshot || (items.KeyIP >= 1
                    && CanNotWasteKeysBeforeAccessible(items, MapChest, BigKeyChest))),
                memoryAddress: 0x5F,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            MapChest = new Location(this, 256 + 163, 0x1E9DD, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapIP,
                access: items => BigKeyChest != null && items.Hammer && Logic.CanLiftLight(items) && (
                    items.Hookshot || (items.KeyIP >= 1
                                       && CanNotWasteKeysBeforeAccessible(items, SpikeRoom, BigKeyChest))
                ),
                memoryAddress: 0x3F,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            BigKeyChest = new Location(this, 256 + 164, 0x1E9A4, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyIP,
                access: items => items.Hammer && Logic.CanLiftLight(items) && (
                    items.Hookshot || (items.KeyIP >= 1
                                       && CanNotWasteKeysBeforeAccessible(items, SpikeRoom, MapChest))
                ),
                memoryAddress: 0x1F,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            IcedTRoom = new Location(this, 256 + 165, 0x1E9E3, LocationType.Regular,
                name: "Iced T Room",
                vanillaItem: ItemType.KeyIP,
                memoryAddress: 0xAE,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            FreezorChest = new Location(this, 256 + 166, 0x1E995, LocationType.Regular,
                name: "Freezor Chest",
                vanillaItem: ItemType.ThreeBombs,
                memoryAddress: 0x7E,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            BigChest = new Location(this, 256 + 167, 0x1E9AA, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveTunic,
                access: items => items.BigKeyIP,
                memoryAddress: 0x9E,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            KholdstareReward = new Location(this, 256 + 168, 0x308157, LocationType.Regular,
                name: "Kholdstare",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyIP && items.Hammer && Logic.CanLiftLight(items) &&
                    items.KeyIP >= (items.Somaria ? 1 : 2) && (!Config.LogicConfig.KholdstareNeedsCaneOfSomaria || items.Somaria),
                memoryAddress: 0xDE,
                memoryFlag: 0xB,
                metadata: metadata,
                trackerState: trackerState);

            MemoryAddress = 0xDE;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 14 };
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Ice Palace");
            DungeonMetadata = metadata?.Dungeon(GetType()) ?? new DungeonInfo("Ice Palace", "IP", "Kholdstare");
            DungeonState = trackerState?.DungeonStates.First(x => x.WorldId == world.Id && x.Name == GetType().Name) ?? new TrackerDungeonState();
            Reward = new Reward(DungeonState.Reward ?? RewardType.None, world, this, metadata, DungeonState);
        }

        public override string Name => "Ice Palace";

        public int SongIndex { get; init; } = 7;

        public Reward Reward { get; set; }

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public Region ParentRegion => World.DarkWorldSouth;

        public Location CompassChest { get; }

        public Location SpikeRoom { get; }

        public Location MapChest { get; }

        public Location BigKeyChest { get; }

        public Location IcedTRoom { get; }

        public Location FreezorChest { get; }

        public Location BigChest { get; }

        public Location KholdstareReward { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.MoonPearl && items.Flippers && Logic.CanLiftHeavy(items) && Logic.CanMeltFreezors(items);
        }

        public bool CanComplete(Progression items)
        {
            return KholdstareReward.IsAvailable(items);
        }

        private bool CanNotWasteKeysBeforeAccessible(Progression items, params Location[] locations)
        {
            return !items.BigKeyIP || locations.Any(l => l.ItemIs(ItemType.BigKeyIP, World));
        }
    }
}
