using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class PalaceOfDarkness : Z3Region, IHasReward
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5B8
        };

        public PalaceOfDarkness(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyPD, ItemType.BigKeyPD, ItemType.MapPD, ItemType.CompassPD };

            ShooterRoom = new Location(this, 256 + 121, 0x1EA5B, LocationType.Regular,
                name: "Shooter Room",
                vanillaItem: ItemType.KeyPD);

            BigKeyChest = new Location(this, 256 + 122, 0x1EA37, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyPD,
                access: items => items.KeyPD >= (BigKeyChest.ItemIs(ItemType.KeyPD, World) ? 1 :
                    (items.Hammer && items.Bow && items.Lamp) || config.Keysanity ? 6 : 5))
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyPD, World) && items.KeyPD >= 5);

            StalfosBasement = new Location(this, 256 + 123, 0x1EA49, LocationType.Regular,
                name: "Stalfos Basement",
                vanillaItem: ItemType.KeyPD,
                access: items => items.KeyPD >= 1 || (items.Bow && items.Hammer));

            ArenaBridge = new Location(this, 256 + 124, 0x1EA3D, LocationType.Regular,
                name: "The Arena - Bridge",
                vanillaItem: ItemType.KeyPD,
                access: items => items.KeyPD >= 1 || (items.Bow && items.Hammer));

            ArenaLedge = new Location(this, 256 + 125, 0x1EA3A, LocationType.Regular,
                name: "The Arena - Ledge",
                vanillaItem: ItemType.KeyPD,
                access: items => items.Bow);

            MapChest = new Location(this, 256 + 126, 0x1EA52, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapPD,
                access: items => items.Bow);

            CompassChest = new Location(this, 256 + 127, 0x1EA43, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassPD,
                access: items => items.KeyPD >= ((items.Hammer && items.Bow && items.Lamp) || config.Keysanity ? 4 : 3));

            HarmlessHellway = new Location(this, 256 + 128, 0x1EA46, LocationType.Regular,
                name: "Harmless Hellway",
                vanillaItem: ItemType.FiveRupees,
                access: items => items.KeyPD >= (HarmlessHellway.ItemIs(ItemType.KeyPD, World) ?
                    (items.Hammer && items.Bow && items.Lamp) || config.Keysanity ? 4 : 3 :
                    (items.Hammer && items.Bow && items.Lamp) || config.Keysanity ? 6 : 5))
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyPD, World) && items.KeyPD >= 5);

            BigChest = new Location(this, 256 + 133, 0x1EA40, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Hammer,
                access: items => items.BigKeyPD && items.Lamp && items.KeyPD >= ((items.Hammer && items.Bow) || config.Keysanity ? 6 : 5));

            HelmasaurKingReward = new Location(this, 256 + 134, 0x308153, LocationType.Regular,
                name: "Helmasaur King",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.Lamp && items.Hammer && items.Bow && items.BigKeyPD && items.KeyPD >= 6);

            DarkMaze = new(this);
            DarkBasement = new(this);
        }

        public override string Name => "Palace of Darkness";

        public override string Area => "Dark Palace";

        public Reward Reward { get; set; } = Reward.None;

        public Location ShooterRoom { get; }

        public Location BigKeyChest { get; }

        public Location StalfosBasement { get; }

        public Location ArenaBridge { get; }

        public Location ArenaLedge { get; }

        public Location MapChest { get; }

        public Location CompassChest { get; }

        public Location HarmlessHellway { get; }

        public Location BigChest { get; }

        public Location HelmasaurKingReward { get; }

        public DarkMazeRoom DarkMaze { get; }

        public DarkBasementRoom DarkBasement { get; }

        public override bool CanEnter(Progression items)
        {
            return items.MoonPearl && World.DarkWorldNorthEast.CanEnter(items);
        }

        public bool CanComplete(Progression items)
        {
            return HelmasaurKingReward.IsAvailable(items);
        }

        public class DarkMazeRoom : Room
        {
            public DarkMazeRoom(Region region)
                : base(region, "Dark Maze")
            {
                Top = new Location(this, 256 + 131, 0x1EA55, LocationType.Regular,
                    name: "Top",
                    vanillaItem: ItemType.ThreeBombs,
                    access: items => items.Lamp && items.KeyPD >= ((items.Hammer && items.Bow) || Config.Keysanity ? 6 : 5));

                Bottom = new Location(this, 256 + 132, 0x1EA58, LocationType.Regular,
                    name: "Bottom",
                    vanillaItem: ItemType.KeyPD,
                    access: items => items.Lamp && items.KeyPD >= ((items.Hammer && items.Bow) || Config.Keysanity ? 6 : 5));
            }

            public Location Top { get; }

            public Location Bottom { get; }
        }

        public class DarkBasementRoom : Room
        {
            public DarkBasementRoom(Region region)
                : base(region, "Dark Basement")
            {
                Left = new Location(this, 256 + 129, 0x1EA4C, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TenArrows,
                    access: items => Logic.CanPassFireRodDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.Keysanity ? 4 : 3));

                Right = new Location(this, 256 + 130, 0x1EA4F, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.KeyPD,
                    access: items => Logic.CanPassFireRodDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.Keysanity ? 4 : 3));
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
