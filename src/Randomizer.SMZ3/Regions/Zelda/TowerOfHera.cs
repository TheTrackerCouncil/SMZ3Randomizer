using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class TowerOfHera : Z3Region, IHasReward
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5C5,
            0x02907A,
            0x028B8C
        };

        public TowerOfHera(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyTH, ItemType.BigKeyTH, ItemType.MapTH, ItemType.CompassTH };

            BasementCage = new Location(this, 256 + 115, 0x308162, LocationType.HeraStandingKey,
                name: "Basement Cage",
                vanillaItem: ItemType.KeyTH,
                memoryAddress: 0x87,
                memoryFlag: 0xA);

            MapChest = new Location(this, 256 + 116, 0x1E9AD, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapTH,
                memoryAddress: 0x77,
                memoryFlag: 0x4);

            BigKeyChest = new Location(this, 256 + 117, 0x1E9E6, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyTH,
                access: items => items.KeyTH && Logic.CanLightTorches(items),
                memoryAddress: 0x87,
                memoryFlag: 0x4)
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyTH, World));

            CompassChest = new Location(this, 256 + 118, 0x1E9FB, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassTH,
                access: items => items.BigKeyTH,
                memoryAddress: 0x27,
                memoryFlag: 0x5);

            BigChest = new Location(this, 256 + 119, 0x1E9F8, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.MoonPearl,
                access: items => items.BigKeyTH,
                memoryAddress: 0x27,
                memoryFlag: 0x4);

            MoldormReward = new Location(this, 256 + 120, 0x308152, LocationType.Regular,
                name: "Moldorm",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyTH && CanBeatBoss(items),
                memoryAddress: 0x7,
                memoryFlag: 0xB);

            MemoryAddress = 0x7;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 0x77 };
        }

        public override string Name => "Tower of Hera";

        public Reward Reward { get; set; } = Reward.None;

        public Location BasementCage { get; }

        public Location MapChest { get; }

        public Location BigKeyChest { get; }

        public Location CompassChest { get; }

        public Location BigChest { get; }

        public Location MoldormReward { get; }

        public override bool CanEnter(Progression items)
        {
            return (items.Mirror || (items.Hookshot && items.Hammer))
                   && World.LightWorldDeathMountainWest.CanEnter(items);
        }

        public bool CanComplete(Progression items)
        {
            return MoldormReward.IsAvailable(items);
        }

        private bool CanBeatBoss(Progression items)
        {
            return items.Sword || items.Hammer;
        }
    }
}
