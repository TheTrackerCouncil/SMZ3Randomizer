using System.Collections.Generic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class ThievesTown : Z3Region, IHasReward, IDungeon
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5C6
        };

        public ThievesTown(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyTT, ItemType.BigKeyTT, ItemType.MapTT, ItemType.CompassTT };

            MapChest = new Location(this, 256 + 153, 0x1EA01, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapTT,
                memoryAddress: 0xDB,
                memoryFlag: 0x4);

            AmbushChest = new Location(this, 256 + 154, 0x1EA0A, LocationType.Regular,
                name: "Ambush Chest",
                vanillaItem: ItemType.TwentyRupees,
                memoryAddress: 0xCB,
                memoryFlag: 0x4);

            CompassChest = new Location(this, 256 + 155, 0x1EA07, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassTT,
                memoryAddress: 0xDC,
                memoryFlag: 0x4);

            BigKeyChest = new Location(this, 256 + 156, 0x1EA04, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyTT,
                memoryAddress: 0xDB,
                memoryFlag: 0x5);

            Attic = new Location(this, 256 + 157, 0x1EA0D, LocationType.Regular,
                name: "Attic", // ??? Vanilla item ???
                access: items => items.BigKeyTT && items.KeyTT,
                memoryAddress: 0x65,
                memoryFlag: 0x4);

            BlindsCell = new Location(this, 256 + 158, 0x1EA13, LocationType.Regular,
                name: "Blind's Cell",
                vanillaItem: ItemType.KeyTT,
                access: items => items.BigKeyTT,
                memoryAddress: 0x45,
                memoryFlag: 0x4);

            BigChest = new Location(this, 256 + 159, 0x1EA10, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveGlove,
                access: items => items.BigKeyTT && items.Hammer &&
                    (BigChest.ItemIs(ItemType.KeyTT, World) || items.KeyTT),
                memoryAddress: 0x44,
                memoryFlag: 0x4)
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyTT, World) && items.Hammer);

            BlindReward = new Location(this, 256 + 160, 0x308156, LocationType.Regular,
                name: "Blind",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyTT && items.KeyTT && CanBeatBoss(items),
                memoryAddress: 0xAC,
                memoryFlag: 0xB);

            MemoryAddress = 0xAC;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 219 };
        }

        public override string Name => "Thieves' Town";

        public Reward Reward { get; set; }
        public RewardType RewardType { get; set; } = RewardType.None;

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public Region ParentRegion => World.DarkWorldNorthWest;

        public Location MapChest { get; }

        public Location AmbushChest { get; }

        public Location CompassChest { get; }

        public Location BigKeyChest { get; }

        public Location Attic { get; }

        public Location BlindsCell { get; }

        public Location BigChest { get; }

        public Location BlindReward { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.MoonPearl && World.DarkWorldNorthWest.CanEnter(items, requireRewards);
        }

        public bool CanComplete(Progression items)
        {
            return BlindReward.IsAvailable(items);
        }

        private bool CanBeatBoss(Progression items)
        {
            return items.Sword || items.Hammer ||
                items.Somaria || items.Byrna;
        }
    }
}
