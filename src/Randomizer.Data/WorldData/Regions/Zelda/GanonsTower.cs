using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using Randomizer.Shared.Enums;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class GanonsTower : Z3Region, IDungeon
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5C9
        };

        public GanonsTower(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            RegionItems = new[] { ItemType.KeyGT, ItemType.BigKeyGT, ItemType.MapGT, ItemType.CompassGT };

            BobsTorch = new Location(this, 256 + 189, 0x308161, LocationType.Regular,
                name: "Bob's Torch",
                vanillaItem: ItemType.KeyGT,
                access: items => items.Boots,
                memoryAddress: 0x8C,
                memoryFlag: 0xA,
                metadata: metadata,
                trackerState: trackerState);

            MapChest = new Location(this, 256 + 194, 0x1EAD3, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapGT,
                access: items => items.Hammer && (items.Hookshot || items.Boots) && items.KeyGT >=
                    (new[] { ItemType.BigKeyGT, ItemType.KeyGT }.Any(type => MapChest != null && MapChest.ItemIs(type, World)) ? 3 : 4),
                memoryAddress: 0x8B,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyGT, World) && items.KeyGT >= 3);

            FiresnakeRoom = new Location(this, 256 + 195, 0x1EAD0, LocationType.Regular,
                name: "Firesnake Room",
                vanillaItem: ItemType.KeyGT,
                access: items => FiresnakeRoom != null && RandomizerRoom != null && items.Hammer && items.Hookshot && items.KeyGT >= (new[] {
                        RandomizerRoom.TopRight,
                        RandomizerRoom.TopLeft,
                        RandomizerRoom.BottomLeft,
                        RandomizerRoom.BottomRight
                    }.Any(l => l.ItemIs(ItemType.BigKeyGT, World))
                    || FiresnakeRoom.ItemIs(ItemType.KeyGT, World) ? 2 : 3),
                memoryAddress: 0x7D,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            TileRoom = new Location(this, 256 + 202, 0x1EAE2, LocationType.Regular,
                name: "Tile Room",
                vanillaItem: ItemType.KeyGT,
                access: items => items.Somaria,
                memoryAddress: 0x8D,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            BobsChest = new Location(this, 256 + 207, 0x1EADF, LocationType.Regular,
                name: "Bob's Chest",
                vanillaItem: ItemType.TenArrows,
                access: items => items.KeyGT >= 3 && (
                    (items.Hammer && items.Hookshot) ||
                    (items.Somaria && items.FireRod)),
                memoryAddress: 0x8C,
                memoryFlag: 0x7,
                metadata: metadata,
                trackerState: trackerState);

            BigChest = new Location(this, 256 + 208, 0x1EAD6, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveTunic,
                access: items => items.BigKeyGT && items.KeyGT >= 3 && (
                    (items.Hammer && items.Hookshot) ||
                    (items.Somaria && items.FireRod)),
                memoryAddress: 0x8C,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World));

            PreMoldormChest = new Location(this, 256 + 214, 0x1EB03, LocationType.Regular,
                name: "Pre-Moldorm Chest",
                vanillaItem: ItemType.KeyGT,
                access: TowerAscend,
                memoryAddress: 0x3D,
                memoryFlag: 0x6,
                metadata: metadata,
                trackerState: trackerState)
                .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World));

            MoldormChest = new Location(this, 256 + 215, 0x1EB06, LocationType.Regular,
                name: "Moldorm Chest",
                vanillaItem: ItemType.TwentyRupees,
                access: items => items.BigKeyGT && items.KeyGT >= 4 &&
                    items.Bow && Logic.CanLightTorches(items) &&
                    CanBeatMoldorm(items) && items.Hookshot,
                memoryAddress: 0x4D,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Allow((item, items) => new[] { ItemType.KeyGT, ItemType.BigKeyGT }.All(type => item.IsNot(type, World)));

            DMsRoom = new DMsRoomRoom(this, metadata, trackerState);
            RandomizerRoom = new RandomizerRoomRoom(this, metadata, trackerState);
            HopeRoom = new HopeRoomRoom(this, metadata, trackerState);
            CompassRoom = new CompassRoomRoom(this, metadata, trackerState);
            BigKeyRoom = new BigKeyRoomRoom(this, metadata, trackerState);
            MiniHelmasaurRoom = new MiniHelmasaurRoomRoom(this, metadata, trackerState);
            StartingRooms = new List<int> { 0xC };
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Ganon's Tower");
            DungeonMetadata = metadata?.Dungeon(GetType()) ?? new DungeonInfo("Ganon's Tower", "GT", "Ganon");
            DungeonState = trackerState?.DungeonStates.First(x => x.WorldId == world.Id && x.Name == GetType().Name) ?? new TrackerDungeonState();
        }

        public override string Name => "Ganon's Tower";

        public int SongIndex { get; init; } = 11;

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public Location BobsTorch { get; }

        public Location MapChest { get; }

        public Location FiresnakeRoom { get; }

        public Location TileRoom { get; }

        public Location BobsChest { get; }

        public Location BigChest { get; }

        public Location PreMoldormChest { get; }

        public Location MoldormChest { get; }

        public DMsRoomRoom DMsRoom { get; }

        public RandomizerRoomRoom RandomizerRoom { get; }

        public HopeRoomRoom HopeRoom { get; }

        public CompassRoomRoom CompassRoom { get; }

        public BigKeyRoomRoom BigKeyRoom { get; }

        public MiniHelmasaurRoomRoom MiniHelmasaurRoom { get; }

        public Region ParentRegion => World.DarkWorldDeathMountainWest;

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            var smBosses = new[] { BossType.Kraid, BossType.Phantoon, BossType.Draygon, BossType.Ridley };
            return items.MoonPearl && World.DarkWorldDeathMountainEast.CanEnter(items, requireRewards) && items.CrystalCount >= Config.GanonsTowerCrystalCount && World.CanDefeatBossCount(items, smBosses) >= Config.TourianBossCount;
        }

        public override bool CanFill(Item item, Progression items)
        {
            if (Config.MultiWorld)
            {
                if (item.World != World || item.Progression)
                {
                    return false;
                }

                if (Config.ZeldaKeysanity
                    && !((item.Type == ItemType.BigKeyGT || item.Type == ItemType.KeyGT) && item.World == World)
                    && (item.IsKey || item.IsBigKey || item.IsKeycard))
                {
                    return false;
                }
            }

            return base.CanFill(item, items);
        }

        private bool TowerAscend(Progression items)
        {
            return items.BigKeyGT && items.KeyGT >= 3 && items.Bow && Logic.CanLightTorches(items);
        }

        private static bool CanBeatMoldorm(Progression items)
        {
            return items.Sword || items.Hammer;
        }

        public class DMsRoomRoom : Room
        {
            public DMsRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "DMs Room", metadata)
            {
                // "bombs, arrows, and Rupees" - but what?
                TopLeft = new Location(this, 256 + 190, 0x1EAB8, LocationType.Regular,
                    name: "Top Left",
                    access: items => items.Hammer && items.Hookshot,
                    memoryAddress: 0x7B,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
                TopRight = new Location(this, 256 + 191, 0x1EABB, LocationType.Regular,
                    name: "Top Right",
                    access: items => items.Hammer && items.Hookshot,
                    memoryAddress: 0x7B,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);
                BottomLeft = new Location(this, 256 + 192, 0x1EABE, LocationType.Regular,
                    name: "Bottom Left",
                    access: items => items.Hammer && items.Hookshot,
                    memoryAddress: 0x7B,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState);
                BottomRight = new Location(this, 256 + 193, 0x1EAC1, LocationType.Regular,
                    name: "Bottom Right",
                    access: items => items.Hammer && items.Hookshot,
                    memoryAddress: 0x7B,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location TopLeft { get; }

            public Location TopRight { get; }

            public Location BottomLeft { get; }

            public Location BottomRight { get; }
        }

        public class RandomizerRoomRoom : Room
        {
            public RandomizerRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Randomizer Room", metadata) // The room with all the floor tiles
            {
                TopLeft = new Location(this, 256 + 196, 0x1EAC4, LocationType.Regular,
                    name: "Top Left",
                    access: items => LeftSide(items, new[] { TopRight, BottomLeft, BottomRight }),
                    memoryAddress: 0x7C,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
                TopRight = new Location(this, 256 + 197, 0x1EAC7, LocationType.Regular,
                    name: "Top Right",
                    access: items => LeftSide(items, new[] { TopLeft, BottomLeft, BottomRight }),
                    memoryAddress: 0x7C,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);
                BottomLeft = new Location(this, 256 + 198, 0x1EACA, LocationType.Regular,
                    name: "Bottom Left",
                    access: items => LeftSide(items, new[] { TopRight, TopLeft, BottomRight }),
                    memoryAddress: 0x7C,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState);
                BottomRight = new Location(this, 256 + 199, 0x1EACD, LocationType.Regular,
                    name: "Bottom Right",
                    access: items => LeftSide(items, new[] { TopRight, TopLeft, BottomLeft }),
                    memoryAddress: 0x7C,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location TopLeft { get; }

            public Location TopRight { get; }

            public Location BottomLeft { get; }

            public Location BottomRight { get; }

            private bool LeftSide(Progression items, IList<Location?> locations)
            {
                return items.Hammer && items.Hookshot && items.KeyGT >= (locations.Any(l => l != null && l.ItemIs(ItemType.BigKeyGT, World)) ? 3 : 4);
            }
        }

        public class HopeRoomRoom : Room
        {
            public HopeRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Hope Room", metadata, "Right Side First Room")
            {
                Left = new Location(this, 256 + 200, 0x1EAD9, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TenArrows,
                    memoryAddress: 0x8C,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);

                Right = new Location(this, 256 + 201, 0x1EADC, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.ThreeBombs,
                    memoryAddress: 0x8C,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Left { get; }

            public Location Right { get; }
        }

        public class CompassRoomRoom : Room
        {
            public CompassRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Compass Room", metadata)
            {
                TopLeft = new Location(this, 256 + 203, 0x1EAE5, LocationType.Regular,
                    name: "Top Left",
                    vanillaItem: ItemType.CompassGT,
                    access: items => RightSide(items, new[] { TopRight, BottomLeft, BottomRight }),
                    memoryAddress: 0x9D,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);

                TopRight = new Location(this, 256 + 204, 0x1EAE8, LocationType.Regular,
                    name: "Top Right",
                    access: items => RightSide(items, new[] { TopLeft, BottomLeft, BottomRight }),
                    memoryAddress: 0x9D,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);

                BottomLeft = new Location(this, 256 + 205, 0x1EAEB, LocationType.Regular,
                    name: "Bottom Left",
                    access: items => RightSide(items, new[] { TopRight, TopLeft, BottomRight }),
                    memoryAddress: 0x9D,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState);

                BottomRight = new Location(this, 256 + 206, 0x1EAEE, LocationType.Regular,
                    name: "Bottom Right",
                    access: items => RightSide(items, new[] { TopRight, TopLeft, BottomLeft }),
                    memoryAddress: 0x9D,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location TopLeft { get; }

            public Location TopRight { get; }

            public Location BottomLeft { get; }

            public Location BottomRight { get; }

            private bool RightSide(Progression items, IList<Location?> locations)
            {
                return items.Somaria && items.FireRod && items.KeyGT >= (locations.Any(l => l != null && l.ItemIs(ItemType.BigKeyGT, World)) ? 3 : 4);
            }
        }

        public class BigKeyRoomRoom : Room
        {
            public BigKeyRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Big Key Room", metadata)
            {
                Bottom = new Location(this, 256 + 209, 0x1EAF1, LocationType.Regular,
                    name: "Bottom Big Key Chest",
                    vanillaItem: ItemType.BigKeyGT,
                    access: BigKeyRoom,
                    memoryAddress: 0x1C,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);

                Left = new Location(this, 256 + 210, 0x1EAF4, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TenArrows,
                    access: BigKeyRoom,
                    memoryAddress: 0x1C,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);

                Right = new Location(this, 256 + 211, 0x1EAF7, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.ThreeBombs,
                    access: BigKeyRoom,
                    memoryAddress: 0x1C,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Bottom { get; }

            public Location Left { get; }

            public Location Right { get; }

            private bool BigKeyRoom(Progression items)
            {
                return items.KeyGT >= 3 && CanBeatArmos(items)
                    && ((items.Hammer && items.Hookshot) || (items.FireRod && items.Somaria));
            }

            private bool CanBeatArmos(Progression items)
            {
                return items.Sword || items.Hammer || items.Bow ||
                    (Logic.CanExtendMagic(items) && (items.Somaria || items.Byrna)) ||
                    (Logic.CanExtendMagic(items, 4) && (items.FireRod || items.IceRod));
            }
        }

        public class MiniHelmasaurRoomRoom : Room
        {
            public MiniHelmasaurRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Mini Helmasaur Room", metadata)
            {
                var tower = (GanonsTower)region;
                Left = new Location(this, 256 + 212, 0x1EAFD, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.ThreeBombs,
                    access: tower.TowerAscend,
                    memoryAddress: 0x3D,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World));

                Right = new Location(this, 256 + 213, 0x1EB00, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.ThreeBombs,
                    access: tower.TowerAscend,
                    memoryAddress: 0x3D,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World));
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
