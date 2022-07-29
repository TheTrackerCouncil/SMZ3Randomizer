﻿using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class TurtleRock : Z3Region, IHasReward, INeedsMedallion
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5C7,
            0x02D5A7,
            0x02D5AA,
            0x02D5AB
        };
        public TurtleRock(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyTR, ItemType.BigKeyTR, ItemType.MapTR, ItemType.CompassTR };

            CompassChest = new Location(this, 256 + 177, 0x1EA22, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassTR,
                memoryAddress: 0xD6,
                memoryFlag: 0x4);

            ChainChomps = new Location(this, 256 + 180, 0x1EA16, LocationType.Regular,
                name: "Chain Chomps",
                vanillaItem: ItemType.KeyTR,
                access: items => items.KeyTR >= 1,
                memoryAddress: 0xB6,
                memoryFlag: 0x4);

            BigKeyChest = new Location(this, 256 + 181, 0x1EA25, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyTR,
                access: items => items.KeyTR >=
                    (!Config.Keysanity || BigKeyChest.ItemIs(ItemType.BigKeyTR, World) ? 2 :
                        BigKeyChest.ItemIs(ItemType.KeyTR, World) ? 3 : 4),
                memoryAddress: 0x14,
                memoryFlag: 0x4)
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyTR, World) && items.KeyTR >= 3);

            BigChest = new Location(this, 256 + 182, 0x1EA19, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveShield,
                access: items => items.BigKeyTR && items.KeyTR >= 2,
                memoryAddress: 0x24,
                memoryFlag: 0x4)
                .Allow((item, items) => item.IsNot(ItemType.BigKeyTR, World));

            CrystarollerRoom = new Location(this, 256 + 183, 0x1EA34, LocationType.Regular,
                name: "Crystaroller Room",
                vanillaItem: ItemType.KeyTR,
                access: items => items.BigKeyTR && items.KeyTR >= 2,
                memoryAddress: 0x4,
                memoryFlag: 0x4);

            TrinexxReward = new Location(this, 256 + 188, 0x308159, LocationType.Regular,
                name: "Trinexx",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyTR && items.KeyTR >= 4 && Logic.CanPassSwordOnlyDarkRooms(items) && CanBeatBoss(items),
                memoryAddress: 0xA4,
                memoryFlag: 0xB);

            DungeonReward = new Location(this, -1, -1, LocationType.ZeldaReward,
                name: "Turtle Rock Reward",
                vanillaItem: ItemType.CrystalBlue,
                access: items => CanComplete(items),
                memoryAddress: 0xA4,
                memoryFlag: 0xB);

            RollerRoom = new RollerRoomRoom(this);
            LaserBridge = new LaserBridgeRoom(this);

            MemoryAddress = 0xA4;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 35, 36, 213, 214 };
        }

        public override string Name => "Turtle Rock";

        public Reward Reward { get; set; } = Reward.None;

        public ItemType Medallion { get; set; }

        public Location CompassChest { get; }

        public Location ChainChomps { get; }

        public Location BigKeyChest { get; }

        public Location BigChest { get; }

        public Location CrystarollerRoom { get; }

        public Location TrinexxReward { get; }

        public RollerRoomRoom RollerRoom { get; }

        public LaserBridgeRoom LaserBridge { get; }

        public Location DungeonReward { get; }

        public Location RewardLocation => DungeonReward;

        public override bool CanEnter(Progression items)
        {
            return items.Contains(Medallion) && items.Sword && items.MoonPearl &&
                Logic.CanLiftHeavy(items) && items.Hammer && items.Somaria &&
                World.LightWorldDeathMountainEast.CanEnter(items);
        }

        public bool CanComplete(Progression items)
        {
            return TrinexxReward.IsAvailable(items);
        }

        private bool CanBeatBoss(Progression items)
        {
            return items.FireRod && items.IceRod;
        }

        public class RollerRoomRoom : Room
        {
            public RollerRoomRoom(Region region)
                : base(region, "Roller Room")
            {
                Left = new Location(this, 256 + 178, 0x1EA1C, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.MapTR,
                    access: items => items.FireRod,
                    memoryAddress: 0xB7,
                    memoryFlag: 0x4);
                Right = new Location(this, 256 + 179, 0x1EA1F, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.KeyTR,
                    access: items => items.FireRod,
                    memoryAddress: 0xB7,
                    memoryFlag: 0x5);
            }

            public Location Left { get; }

            public Location Right { get; }
        }

        public class LaserBridgeRoom : Room
        {
            public LaserBridgeRoom(Region region)
                : base(region, "Eye Bridge", "Laser Bridge")
            {
                TopRight = new Location(this, 256 + 184, 0x1EA28, LocationType.Regular,
                    name: "Top Right",
                    vanillaItem: ItemType.FiveRupees,
                    access: CanAccess,
                    memoryAddress: 0xD5,
                    memoryFlag: 0x4);

                TopLeft = new Location(this, 256 + 185, 0x1EA2B, LocationType.Regular,
                    name: "Top Left",
                    vanillaItem: ItemType.FiveRupees,
                    access: CanAccess,
                    memoryAddress: 0xD5,
                    memoryFlag: 0x5);

                BottomRight = new Location(this, 256 + 186, 0x1EA2E, LocationType.Regular,
                    name: "Bottom Right",
                    vanillaItem: ItemType.TwentyRupees,
                    access: CanAccess,
                    memoryAddress: 0xD5,
                    memoryFlag: 0x6);

                BottomLeft = new Location(this, 256 + 187, 0x1EA31, LocationType.Regular,
                    name: "Bottom Left",
                    vanillaItem: ItemType.KeyTR,
                    access: CanAccess,
                    memoryAddress: 0xD5,
                    memoryFlag: 0x7);
            }

            public Location TopRight { get; }

            public Location TopLeft { get; }

            public Location BottomRight { get; }

            public Location BottomLeft { get; }

            private bool CanAccess(Progression items)
            {
                return items.BigKeyTR && items.KeyTR >= 3 && Logic.CanPassSwordOnlyDarkRooms(items) && (items.Cape || items.Byrna || items.CanBlockLasers);
            }
        }
    }
}
