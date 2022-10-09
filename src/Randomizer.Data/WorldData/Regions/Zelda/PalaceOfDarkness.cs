using System.Collections.Generic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class PalaceOfDarkness : Z3Region, IHasReward, IDungeon
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5B8
        };

        public PalaceOfDarkness(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyPD, ItemType.BigKeyPD, ItemType.MapPD, ItemType.CompassPD };

            ShooterRoom = new Location(this, 256 + 121, 0x1EA5B, LocationType.Regular,
                name: "Shooter Room",
                vanillaItem: ItemType.KeyPD,
                memoryAddress: 0x9,
                memoryFlag: 0x4);

            BigKeyChest = new Location(this, 256 + 122, 0x1EA37, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyPD,
                access: items => items.KeyPD >= (BigKeyChest.ItemIs(ItemType.KeyPD, World) ? 1 :
                    (items.Hammer && items.Bow && Logic.CanPassSwordOnlyDarkRooms(items)) || config.ZeldaKeysanity ? 6 : 5),
                memoryAddress: 0x3A,
                memoryFlag: 0x4)
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyPD, World) && items.KeyPD >= 5);

            StalfosBasement = new Location(this, 256 + 123, 0x1EA49, LocationType.Regular,
                name: "Stalfos Basement",
                vanillaItem: ItemType.KeyPD,
                access: items => items.KeyPD >= 1 || (items.Bow && items.Hammer),
                memoryAddress: 0xA,
                memoryFlag: 0x4);

            ArenaBridge = new Location(this, 256 + 124, 0x1EA3D, LocationType.Regular,
                name: "The Arena - Bridge",
                vanillaItem: ItemType.KeyPD,
                access: items => items.KeyPD >= 1 || (items.Bow && items.Hammer),
                memoryAddress: 0x2A,
                memoryFlag: 0x5);

            ArenaLedge = new Location(this, 256 + 125, 0x1EA3A, LocationType.Regular,
                name: "The Arena - Ledge",
                vanillaItem: ItemType.KeyPD,
                access: items => items.Bow,
                memoryAddress: 0x2A,
                memoryFlag: 0x4);

            MapChest = new Location(this, 256 + 126, 0x1EA52, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapPD,
                access: items => items.Bow,
                memoryAddress: 0x2B,
                memoryFlag: 0x4);

            CompassChest = new Location(this, 256 + 127, 0x1EA43, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassPD,
                access: items => items.KeyPD >= ((items.Hammer && items.Bow && Logic.CanPassSwordOnlyDarkRooms(items)) || config.ZeldaKeysanity ? 4 : 3),
                memoryAddress: 0x1A,
                memoryFlag: 0x5);

            HarmlessHellway = new Location(this, 256 + 128, 0x1EA46, LocationType.Regular,
                name: "Harmless Hellway",
                vanillaItem: ItemType.FiveRupees,
                access: items => items.KeyPD >= (HarmlessHellway.ItemIs(ItemType.KeyPD, World) ?
                    (items.Hammer && items.Bow && Logic.CanPassSwordOnlyDarkRooms(items)) || config.ZeldaKeysanity ? 4 : 3 :
                    (items.Hammer && items.Bow && Logic.CanPassSwordOnlyDarkRooms(items)) || config.ZeldaKeysanity ? 6 : 5),
                memoryAddress: 0x1A,
                memoryFlag: 0x6)
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyPD, World) && items.KeyPD >= 5);

            BigChest = new Location(this, 256 + 133, 0x1EA40, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Hammer,
                access: items => items.BigKeyPD && Logic.CanPassSwordOnlyDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || config.ZeldaKeysanity ? 6 : 5),
                memoryAddress: 0x1A,
                memoryFlag: 0x4);

            HelmasaurKingReward = new Location(this, 256 + 134, 0x308153, LocationType.Regular,
                name: "Helmasaur King",
                vanillaItem: ItemType.HeartContainer,
                access: items => Logic.CanPassSwordOnlyDarkRooms(items) && items.Hammer && items.Bow && items.BigKeyPD && items.KeyPD >= 6,
                memoryAddress: 0x5A,
                memoryFlag: 0xB);

            DarkMaze = new DarkMazeRoom(this);
            DarkBasement = new DarkBasementRoom(this);

            MemoryAddress = 0x5A;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 74 };
        }

        public override string Name => "Palace of Darkness";

        public override string Area => "Dark Palace";

        public Reward Reward { get; set; }
        public RewardType RewardType { get; set; } = RewardType.None;

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public Region ParentRegion => World.DarkWorldNorthEast;

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

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.MoonPearl && World.DarkWorldNorthEast.CanEnter(items, requireRewards);
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
                    access: items => Logic.CanPassSwordOnlyDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.ZeldaKeysanity ? 6 : 5),
                    memoryAddress: 0x19,
                    memoryFlag: 0x4);

                Bottom = new Location(this, 256 + 132, 0x1EA58, LocationType.Regular,
                    name: "Bottom",
                    vanillaItem: ItemType.KeyPD,
                    access: items => Logic.CanPassSwordOnlyDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.ZeldaKeysanity ? 6 : 5),
                    memoryAddress: 0x19,
                    memoryFlag: 0x5);
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
                    access: items => Logic.CanPassFireRodDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.ZeldaKeysanity ? 4 : 3),
                    memoryAddress: 0x6A,
                    memoryFlag: 0x4);

                Right = new Location(this, 256 + 130, 0x1EA4F, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.KeyPD,
                    access: items => Logic.CanPassFireRodDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.ZeldaKeysanity ? 4 : 3),
                    memoryAddress: 0x6A,
                    memoryFlag: 0x5);
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
