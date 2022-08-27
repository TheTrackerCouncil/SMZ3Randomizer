using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class SwampPalace : Z3Region, IHasReward
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5B7
        };

        public SwampPalace(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeySP, ItemType.BigKeySP, ItemType.MapSP, ItemType.CompassSP };

            Entrance = new Location(this, 256 + 135, 0x1EA9D, LocationType.Regular,
                name: "Entrance",
                vanillaItem: ItemType.KeySP,
                memoryAddress: 0x28,
                memoryFlag: 0x4)
                .Allow((item, items) => Config.ZeldaKeysanity || item.Is(ItemType.KeySP, World));

            MapChest = new Location(this, 256 + 136, 0x1E986, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapSP,
                access: items => items.KeySP,
                memoryAddress: 0x37,
                memoryFlag: 0x4);

            BigChest = new Location(this, 256 + 137, 0x1E989, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Hookshot,
                access: items => items.BigKeySP && items.KeySP && items.Hammer,
                memoryAddress: 0x36,
                memoryFlag: 0x4)
                .AlwaysAllow((item, items) => item.Is(ItemType.BigKeySP, World));

            CompassChest = new Location(this, 256 + 138, 0x1EAA0, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassSP,
                access: items => items.KeySP && items.Hammer,
                memoryAddress: 0x46,
                memoryFlag: 0x4);

            WestChest = new Location(this, 256 + 139, 0x1EAA3, LocationType.Regular,
                name: "West Chest",
                vanillaItem: ItemType.TwentyRupees,
                access: items => items.KeySP && items.Hammer,
                memoryAddress: 0x34,
                memoryFlag: 0x4);

            BigKeyChest = new Location(this, 256 + 140, 0x1EAA6, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeySP,
                access: items => items.KeySP && items.Hammer,
                memoryAddress: 0x35,
                memoryFlag: 0x4);

            WaterfallRoom = new Location(this, 256 + 143, 0x1EAAF, LocationType.Regular,
                name: "Waterfall Room",
                vanillaItem: ItemType.TwentyRupees,
                access: items => items.KeySP && items.Hammer && items.Hookshot,
                memoryAddress: 0x66,
                memoryFlag: 0x4);

            ArrghusReward = new Location(this, 256 + 144, 0x308154, LocationType.Regular,
                name: "Arrghus",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.KeySP && items.Hammer && items.Hookshot,
                memoryAddress: 0x6,
                memoryFlag: 0xB);

            FloodedRoom = new FloodedRoomRoom(this);

            MemoryAddress = 0x6;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 40 };
        }

        public override string Name => "Swamp Palace";

        public RewardType Reward { get; set; } = RewardType.None;

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
            public FloodedRoomRoom(Region region)
                : base(region, "Flooded Room")
            {
                Left = new Location(this, 256 + 141, 0x1EAA9, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TwentyRupees,
                    access: items => items.KeySP && items.Hammer && items.Hookshot,
                    memoryAddress: 0x76,
                    memoryFlag: 0x4);
                Right = new Location(this, 256 + 142, 0x1EAAC, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.TwentyRupees,
                    access: items => items.KeySP && items.Hammer && items.Hookshot,
                    memoryAddress: 0x76,
                    memoryFlag: 0x5);
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
