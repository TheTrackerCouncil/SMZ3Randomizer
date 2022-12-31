using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class SwampPalace : Z3Region, IHasReward, IDungeon
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5B7
        };

        public SwampPalace(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            RegionItems = new[] { ItemType.KeySP, ItemType.BigKeySP, ItemType.MapSP, ItemType.CompassSP };

            Entrance = new Location(this, 256 + 135, 0x1EA9D, LocationType.Regular,
                name: "Entrance",
                vanillaItem: ItemType.KeySP,
                memoryAddress: 0x28,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Allow((item, items) => Config.ZeldaKeysanity || item.Is(ItemType.KeySP, World));

            MapChest = new Location(this, 256 + 136, 0x1E986, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapSP,
                access: items => items.KeySP,
                memoryAddress: 0x37,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            BigChest = new Location(this, 256 + 137, 0x1E989, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Hookshot,
                access: items => items.BigKeySP && items.KeySP && items.Hammer,
                memoryAddress: 0x36,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .AlwaysAllow((item, items) => item.Is(ItemType.BigKeySP, World));

            CompassChest = new Location(this, 256 + 138, 0x1EAA0, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassSP,
                access: items => items.KeySP && items.Hammer,
                memoryAddress: 0x46,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            WestChest = new Location(this, 256 + 139, 0x1EAA3, LocationType.Regular,
                name: "West Chest",
                vanillaItem: ItemType.TwentyRupees,
                access: items => items.KeySP && items.Hammer,
                memoryAddress: 0x34,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            BigKeyChest = new Location(this, 256 + 140, 0x1EAA6, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeySP,
                access: items => items.KeySP && items.Hammer,
                memoryAddress: 0x35,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            WaterfallRoom = new Location(this, 256 + 143, 0x1EAAF, LocationType.Regular,
                name: "Waterfall Room",
                vanillaItem: ItemType.TwentyRupees,
                access: items => items.KeySP && items.Hammer && items.Hookshot,
                memoryAddress: 0x66,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            ArrghusReward = new Location(this, 256 + 144, 0x308154, LocationType.Regular,
                name: "Arrghus",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.KeySP && items.Hammer && items.Hookshot,
                memoryAddress: 0x6,
                memoryFlag: 0xB,
                metadata: metadata,
                trackerState: trackerState);

            FloodedRoom = new FloodedRoomRoom(this, metadata, trackerState);

            MemoryAddress = 0x6;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 40 };
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Swamp Palace");
            DungeonMetadata = metadata?.Dungeon(GetType()) ?? new DungeonInfo("Swamp Palace", "SP", "Arrghus");
            DungeonState = trackerState?.DungeonStates.First(x => x.WorldId == world.Id && x.Name == GetType().Name) ?? new TrackerDungeonState();
            Reward = new Reward(DungeonState.Reward ?? RewardType.None, world, this, metadata, DungeonState);
        }

        public override string Name => "Swamp Palace";

        public int SongIndex { get; init; } = 3;

        public Reward Reward { get; set; }

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public Region ParentRegion => World.DarkWorldSouth;

        public Location Entrance { get; }

        public Location MapChest { get; }

        public Location BigChest { get; }

        public Location CompassChest { get; }

        public Location WestChest { get; }

        public Location BigKeyChest { get; }

        public Location WaterfallRoom { get; }

        public Location ArrghusReward { get; }

        public FloodedRoomRoom FloodedRoom { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.MoonPearl && items.Mirror && items.Flippers && World.DarkWorldSouth.CanEnter(items, requireRewards);
        }

        public bool CanComplete(Progression items)
        {
            return ArrghusReward.IsAvailable(items);
        }

        public class FloodedRoomRoom : Room
        {
            public FloodedRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Flooded Room", metadata)
            {
                Left = new Location(this, 256 + 141, 0x1EAA9, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TwentyRupees,
                    access: items => items.KeySP && items.Hammer && items.Hookshot,
                    memoryAddress: 0x76,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
                Right = new Location(this, 256 + 142, 0x1EAAC, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.TwentyRupees,
                    access: items => items.KeySP && items.Hammer && items.Hookshot,
                    memoryAddress: 0x76,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
