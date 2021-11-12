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
                vanillaItem: ItemType.CompassTR);

            ChainChomps = new Location(this, 256 + 180, 0x1EA16, LocationType.Regular,
                name: "Chain Chomps",
                vanillaItem: ItemType.KeyTR,
                access: items => items.KeyTR >= 1);

            BigKeyChest = new Location(this, 256 + 181, 0x1EA25, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyTR,
                access: items => items.KeyTR >=
                    (!Config.Keysanity || BigKeyChest.ItemIs(ItemType.BigKeyTR, World) ? 2 :
                        BigKeyChest.ItemIs(ItemType.KeyTR, World) ? 3 : 4))
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyTR, World) && items.KeyTR >= 3);

            BigChest = new Location(this, 256 + 182, 0x1EA19, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveShield,
                access: items => items.BigKeyTR && items.KeyTR >= 2)
                .Allow((item, items) => item.IsNot(ItemType.BigKeyTR, World));

            CrystarollerRoom = new Location(this, 256 + 183, 0x1EA34, LocationType.Regular,
                name: "Crystaroller Room",
                vanillaItem: ItemType.KeyTR,
                access: items => items.BigKeyTR && items.KeyTR >= 2);

            TrinexxReward = new Location(this, 256 + 188, 0x308159, LocationType.Regular,
                name: "Trinexx",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyTR && items.KeyTR >= 4 && items.Lamp && CanBeatBoss(items));

            RollerRoom = new(this);
            LaserBridge = new(this);
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

        public override bool CanEnter(Progression items)
        {
            return Medallion switch
            {
                ItemType.Bombos => items.Bombos,
                ItemType.Ether => items.Ether,
                _ => items.Quake
            } && items.Sword &&
                items.MoonPearl && items.CanLiftHeavy() && items.Hammer && items.Somaria &&
                World.LightWorldDeathMountainEast.CanEnter(items);
        }

        public bool CanComplete(Progression items)
        {
            return TrinexxReward.IsAvailable(items);
        }

        private bool CanBeatBoss(Progression items)
        {
            return items.Firerod && items.Icerod;
        }

        public class RollerRoomRoom : Room
        {
            public RollerRoomRoom(Region region)
                : base(region, "Roller Room")
            {
                Left = new Location(this, 256 + 178, 0x1EA1C, LocationType.Regular,
                    "Left",
                    ItemType.MapTR,
                    items => items.Firerod);
                Right = new Location(this, 256 + 179, 0x1EA1F, LocationType.Regular,
                    "Right",
                    ItemType.KeyTR,
                    items => items.Firerod);
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
                    access: CanAccess);

                TopLeft = new Location(this, 256 + 185, 0x1EA2B, LocationType.Regular,
                    name: "Top Left",
                    vanillaItem: ItemType.FiveRupees,
                    access: CanAccess);

                BottomRight = new Location(this, 256 + 186, 0x1EA2E, LocationType.Regular,
                    name: "Bottom Right",
                    vanillaItem: ItemType.TwentyRupees,
                    access: CanAccess);

                BottomLeft = new Location(this, 256 + 187, 0x1EA31, LocationType.Regular,
                    name: "Bottom Left",
                    vanillaItem: ItemType.KeyTR,
                    access: CanAccess);
            }

            public Location TopRight { get; }

            public Location TopLeft { get; }

            public Location BottomRight { get; }

            public Location BottomLeft { get; }

            private bool CanAccess(Progression items)
            {
                return items.BigKeyTR && items.KeyTR >= 3 && items.Lamp && (items.Cape || items.Byrna || items.CanBlockLasers);
            }
        }
    }
}
