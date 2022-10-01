using System.Collections.Generic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class DesertPalace : Z3Region, IHasReward, IDungeon
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D59B,
            0x02D59C,
            0x02D59D,
            0x02D59E
        };

        public DesertPalace(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyDP, ItemType.BigKeyDP, ItemType.MapDP, ItemType.CompassDP };

            BigChest = new Location(this, 256 + 109, 0x1E98F, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveGlove,
                access: items => items.BigKeyDP,
                memoryAddress: 0x73,
                memoryFlag: 0x4);

            TorchItem = new Location(this, 256 + 110, 0x308160, LocationType.Regular,
                name: "Torch",
                vanillaItem: ItemType.KeyDP,
                access: items => items.Boots,
                memoryAddress: 0x73,
                memoryFlag: 0xA);

            MapChest = new Location(this, 256 + 111, 0x1E9B6, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapDP,
                memoryAddress: 0x74,
                memoryFlag: 0x4);

            BigKeyChest = new Location(this, 256 + 112, 0x1E9C2, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyDP,
                access: items => items.KeyDP,
                memoryAddress: 0x75,
                memoryFlag: 0x4);

            CompassChest = new Location(this, 256 + 113, 0x1E9CB, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassDP,
                access: items => items.KeyDP,
                memoryAddress: 0x85,
                memoryFlag: 0x4);

            LanmolasReward = new Location(this, 256 + 114, 0x308151, LocationType.Regular,
                name: "Lanmolas",
                vanillaItem: ItemType.HeartContainer,
                access: items => (
                    Logic.CanLiftLight(items) ||
                    (Logic.CanAccessMiseryMirePortal(items) && items.Mirror)
                ) && items.BigKeyDP && items.KeyDP && Logic.CanLightTorches(items) && CanBeatBoss(items),
                memoryAddress: 0x33, 
                memoryFlag: 0xB);

            MemoryAddress = 0x33;
            MemoryFlag = 0xB;
            StartingRooms = new List<int>() { 99, 131, 132, 133 };
        }

        public override string Name => "Desert Palace";

        public override List<string> AlsoKnownAs { get; }
            = new List<string>() { "Dessert Palace" };

        public Reward Reward { get; set; }

        public RewardType RewardType { get; set; } = RewardType.None;

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public Region ParentRegion => World.LightWorldSouth;

        public Location BigChest { get; }

        public Location TorchItem { get; }

        public Location MapChest { get; }

        public Location BigKeyChest { get; }

        public Location CompassChest { get; }

        public Location LanmolasReward { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.Book ||
                items.Mirror && Logic.CanLiftHeavy(items) && items.Flute ||
                Logic.CanAccessMiseryMirePortal(items) && items.Mirror;
        }

        public bool CanComplete(Progression items)
        {
            return LanmolasReward.IsAvailable(items);
        }

        private bool CanBeatBoss(Progression items)
        {
            return items.Sword || items.Hammer || items.Bow ||
                items.FireRod || items.IceRod ||
                items.Byrna || items.Somaria;
        }
    }
}
