using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda.LightWorld
{
    public class LightWorldSouth : Z3Region
    {
        private const int SphereOne = -10;

        public LightWorldSouth(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            MazeRace = new Location(this, 256 + 45, 0x308142, LocationType.Regular,
                name: "Maze Race",
                vanillaItem: ItemType.HeartPiece,
                memoryAddress: 0x28,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            Library = new Location(this, 256 + 240, 0x308012, LocationType.Regular,
                name: "Library",
                vanillaItem: ItemType.Book,
                access: items => items.Boots,
                memoryAddress: 0x190,
                memoryFlag: 0x80,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            ForestClearingDigSpot = new Location(this, 256 + 241, 0x30814A, LocationType.Regular,
                name: "Flute Spot",
                alsoKnownAs: new[] { "Forest Clearing - Digging Spot" },
                vanillaItem: ItemType.Flute,
                access: items => items.Shovel,
                memoryAddress: 0x2A,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            Cave45 = new Location(this, 256 + 242, 0x308003, LocationType.Regular,
                name: "South of Grove",
                alsoKnownAs: new[] { "Cave #45" },
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Mirror && World.DarkWorldSouth.CanEnter(items, true),
                relevanceRequirement: items => items.Mirror && World.DarkWorldSouth.CanEnter(items, false),
                memoryAddress: 0x11B,
                memoryFlag: 0xA,
                metadata: metadata,
                trackerState: trackerState);

            LinksHouse = new Location(this, 256 + 243, 0x1E9BC, LocationType.Regular,
                name: "Link's House",
                vanillaItem: ItemType.Lamp,
                memoryAddress: 0x104,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            Aginah = new Location(this, 256 + 244, 0x1E9F2, LocationType.Regular,
                name: "Aginah's Cave",
                alsoKnownAs: new[] { "Aggina's Cave" },
                vanillaItem: ItemType.HeartPiece,
                memoryAddress: 0x10A,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            DesertLedge = new Location(this, 256 + 252, 0x308143, LocationType.Regular,
                name: "Desert Ledge",
                vanillaItem: ItemType.HeartPiece,
                access: items => World.DesertPalace.CanEnter(items, true),
                memoryAddress: 0x30,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            CheckerboardCave = new Location(this, 256 + 253, 0x308005, LocationType.Regular,
                name: "Checkerboard Cave",
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Mirror && (
                    (items.Flute && Logic.CanLiftHeavy(items)) ||
                        Logic.CanAccessMiseryMirePortal(items)
                    ) && Logic.CanLiftLight(items),
                memoryAddress: 0x126,
                memoryFlag: 0x9,
                metadata: metadata,
                trackerState: trackerState);

            BombosTablet = new Location(this, 256 + 58, 0x308017, LocationType.Bombos,
                name: "Bombos Tablet",
                vanillaItem: ItemType.Bombos,
                access: items => items.Book && items.MasterSword && items.Mirror && World.DarkWorldSouth.CanEnter(items, true),
                relevanceRequirement: items => items.Book && items.MasterSword && items.Mirror && World.DarkWorldSouth.CanEnter(items, false),
                memoryAddress: 0x191,
                memoryFlag: 0x2,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            LakeHyliaIsland = new Location(this, 256 + 61, 0x308144, LocationType.Regular,
                name: "Lake Hylia Island",
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Flippers && items.MoonPearl && items.Mirror && (
                    World.DarkWorldSouth.CanEnter(items, true) ||
                    World.DarkWorldNorthEast.CanEnter(items, true)),
                relevanceRequirement: items => items.Flippers && items.MoonPearl && items.Mirror && (
                    World.DarkWorldSouth.CanEnter(items, false) ||
                    World.DarkWorldNorthEast.CanEnter(items, false)),
                memoryAddress: 0x35,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            UnderTheBridge = new Location(this, 256 + 62, 0x6BE7D, LocationType.Regular,
                name: "Hobo",
                alsoKnownAs: new[] { "Under the bridge" },
                vanillaItem: ItemType.Bottle,
                access: items => items.Flippers || Logic.CanHyruleSouthFakeFlippers(items, false),
                memoryAddress: 0x149,
                memoryFlag: 0x1,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            IceCave = new Location(this, 256 + 63, 0x1EB4E, LocationType.Regular,
                name: "Ice Rod Cave",
                alsoKnownAs: new[] { "Ice Cave" },
                vanillaItem: ItemType.Icerod,
                memoryAddress: 0x120,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            MiniMoldormCave = new MiniMoldormCaveRoom(this, metadata, trackerState);
            SwampRuins = new SwampRuinsRoom(this, metadata, trackerState);

            StartingRooms = new List<int>() { 40, 41, 42, 43, 44, 45, 48, 50, 51, 52, 53, 55, 58, 59, 60, 63 };
            IsOverworld = true;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Light World South");
        }

        public override string Name => "Light World South";
        public override string Area => "Light World";

        public Location MazeRace { get; }

        public Location Library { get; }

        public Location ForestClearingDigSpot { get; }

        public Location Cave45 { get; }

        public Location LinksHouse { get; }

        public Location Aginah { get; }

        public Location DesertLedge { get; }

        public Location CheckerboardCave { get; }

        public Location BombosTablet { get; }

        public Location LakeHyliaIsland { get; }

        public Location UnderTheBridge { get; }

        public Location IceCave { get; }

        public MiniMoldormCaveRoom MiniMoldormCave { get; }

        public SwampRuinsRoom SwampRuins { get; }

        public class MiniMoldormCaveRoom : Room
        {
            public MiniMoldormCaveRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Mini Moldorm Cave", metadata)
            {
                FarLeftChest = new Location(this, 256 + 51, 0x1EB42, LocationType.Regular,
                    name: "Far Left",
                    vanillaItem: ItemType.ThreeBombs,
                    memoryAddress: 0x123,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);

                LeftChest = new Location(this, 256 + 52, 0x1EB45, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x123,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);

                Npc = new Location(this, 256 + 53, 0x308010, LocationType.Regular,
                    name: "NPC",
                    vanillaItem: ItemType.ThreeHundredRupees,
                    memoryAddress: 0x123,
                    memoryFlag: 0xA,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);

                RightChest = new Location(this, 256 + 54, 0x1EB48, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x123,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);

                FarRightChest = new Location(this, 256 + 251, 0x1EB4B, LocationType.Regular,
                    name: "Far Right",
                    vanillaItem: ItemType.TenArrows,
                    memoryAddress: 0x123,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);
            }

            public Location FarLeftChest { get; }

            public Location LeftChest { get; }

            public Location RightChest { get; }

            public Location FarRightChest { get; }

            public Location Npc { get; }
        }

        public class SwampRuinsRoom : Room
        {
            public SwampRuinsRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Swamp Ruins", metadata)
            {
                FloodgateChest = new Location(this, 256 + 59, 0x1E98C, LocationType.Regular,
                   name: "Floodgate Chest",
                   vanillaItem: ItemType.ThreeBombs,
                   memoryAddress: 0x10B,
                   memoryFlag: 0x4,
                   metadata: metadata,
                   trackerState: trackerState)
                   .Weighted(SphereOne);

                SunkedTreasure = new Location(this, 256 + 60, 0x308145, LocationType.Regular,
                    name: "Sunken Treasure",
                    vanillaItem: ItemType.HeartPiece,
                    memoryAddress: 0x3B,
                    memoryFlag: 0x40,
                    memoryType: LocationMemoryType.ZeldaMisc,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);
            }

            public Location FloodgateChest { get; }

            public Location SunkedTreasure { get; }

        }
    }

}
