using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda.LightWorld
{
    public class LightWorldNorthWest : Z3Region
    {
        public const int SphereOne = -14;

        public LightWorldNorthWest(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            MasterSwordPedestal = new Location(this, 256 + 14, 0x589B0, LocationType.Pedestal,
                name: "Master Sword Pedestal",
                alsoKnownAs: new[] { "Ped" },
                vanillaItem: ItemType.ProgressiveSword,
                access: items => items.AllPendants,
                relevanceRequirement: items => World.CanAquireAll(items, RewardType.PendantGreen, RewardType.PendantBlue, RewardType.PendantRed),
                memoryAddress: 0x80,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            Mushroom = new Location(this, 256 + 15, 0x308013, LocationType.Regular,
                name: "Mushroom",
                vanillaItem: ItemType.Mushroom,
                memoryAddress: 0x191,
                memoryFlag: 0x10,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            LostWoodsHideout = new Location(this, 256 + 16, 0x308000, LocationType.Regular,
                name: "Lost Woods Hideout",
                vanillaItem: ItemType.HeartPiece,
                memoryAddress: 0xE1,
                memoryFlag: 0x9,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            LumberjackTree = new Location(this, 256 + 17, 0x308001, LocationType.Regular,
                name: "Lumberjack Tree",
                vanillaItem: ItemType.HeartPiece,
                access: items => Logic.CheckAgahnim(items, World, requireRewards: true) && items.Boots,
                relevanceRequirement: items => Logic.CheckAgahnim(items, World, requireRewards: false) && items.Boots,
                memoryAddress: 0xE2,
                memoryFlag: 0x9,
                metadata: metadata,
                trackerState: trackerState);

            PegasusRocks = new Location(this, 256 + 18, 0x1EB3F, LocationType.Regular,
                name: "Pegasus Rocks",
                alsoKnownAs: new[] { "Bonk Rocks" },
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Boots,
                memoryAddress: 0x124,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            GraveyardLedge = new Location(this, 256 + 19, 0x308004, LocationType.Regular,
                name: "Graveyard Ledge",
                vanillaItem: ItemType.HeartPiece,
                access: items => items.Mirror && items.MoonPearl && World.DarkWorldNorthWest.CanEnter(items, true),
                relevanceRequirement: items => items.Mirror && items.MoonPearl && World.DarkWorldNorthWest.CanEnter(items, false),
                memoryAddress: 0x11B,
                memoryFlag: 0x9,
                metadata: metadata,
                trackerState: trackerState);

            KingsTomb = new Location(this, 256 + 20, 0x1E97A, LocationType.Regular,
                name: "King's Tomb",
                vanillaItem: ItemType.Cape,
                access: items => items.Boots && (
                    Logic.CanLiftHeavy(items) ||
                    (items.Mirror && items.MoonPearl && World.DarkWorldNorthWest.CanEnter(items, true))),
                relevanceRequirement: items => items.Boots && (
                    Logic.CanLiftHeavy(items) ||
                    (items.Mirror && items.MoonPearl && World.DarkWorldNorthWest.CanEnter(items, false))),
                memoryAddress: 0x113,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            BottleMerchant = new Location(this, 256 + 31, 0x5EB18, LocationType.Regular,
                name: "Bottle Merchant",
                vanillaItem: ItemType.Bottle,
                memoryAddress: 0x149,
                memoryFlag: 0x2,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            ChickenHouse = new Location(this, 256 + 250, 0x1E9E9, LocationType.Regular,
                name: "Chicken House",
                alsoKnownAs: new[] { "Chicken Lady's House" },
                vanillaItem: ItemType.TenArrows,
                memoryAddress: 0x108,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            SickKid = new Location(this, 256 + 33, 0x6B9CF, LocationType.Regular,
                name: "Sick Kid",
                alsoKnownAs: new[] { "Bug Catching Kid's House" },
                vanillaItem: ItemType.Bugnet,
                access: items => items.Bottle,
                memoryAddress: 0x190,
                memoryFlag: 0x4,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            TavernBackRoom = new Location(this, 256 + 34, 0x1E9CE, LocationType.Regular,
                name: "Kakariko Tavern",
                alsoKnownAs: new[] { "Inn back room" },
                vanillaItem: ItemType.Bottle,
                memoryAddress: 0x103,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
                .Weighted(SphereOne);

            Blacksmith = new Location(this, 256 + 76, 0x30802A, LocationType.Regular,
                name: "Blacksmith",
                vanillaItem: ItemType.ProgressiveSword,
                access: items => World.DarkWorldNorthWest.CanEnter(items, true) && Logic.CanLiftHeavy(items),
                relevanceRequirement: items => World.DarkWorldNorthWest.CanEnter(items, false) && Logic.CanLiftHeavy(items),
                memoryAddress: 0x191,
                memoryFlag: 0x4,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            MagicBat = new Location(this, 256 + 35, 0x308015, LocationType.Regular,
                name: "Magic Bat",
                vanillaItem: ItemType.HalfMagic,
                access: items => items.Powder
                         && (items.Hammer
                             || (items.MoonPearl && items.Mirror && Logic.CanLiftHeavy(items))),
                memoryAddress: 0x191,
                memoryFlag: 0x80,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

            KakarikoWell = new KakarikoWellArea(this, metadata, trackerState);
            BlindsHideout = new BlindsHideoutRoom(this, metadata, trackerState);
            StartingRooms = new List<int>() { 0, 2, 10, 16, 17, 18, 19, 20, 24, 26, 34, 128};
            IsOverworld = true;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Light World North West");
        }

        public override string Name => "Light World North West";

        public override string Area => "Light World";

        public Location MasterSwordPedestal { get; }

        public Location Mushroom { get; }

        public Location LostWoodsHideout { get; }

        public Location LumberjackTree { get; }

        public Location PegasusRocks { get; }

        public Location GraveyardLedge { get; }

        public Location KingsTomb { get; }

        public Location BottleMerchant { get; }

        public Location ChickenHouse { get; }

        public Location SickKid { get; }

        public Location TavernBackRoom { get; }

        public Location Blacksmith { get; }

        public Location MagicBat { get; }

        public KakarikoWellArea KakarikoWell { get; }

        public BlindsHideoutRoom BlindsHideout { get; }

        public class KakarikoWellArea : Room
        {
            public KakarikoWellArea(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Kakariko Well", metadata)
            {
                BackCave = new Location(this, 256 + 21, 0x1EA8E, LocationType.Regular,
                    name: "Top",
                    alsoKnownAs: new[] { "Back cave" },
                    vanillaItem: ItemType.HeartPiece,
                    memoryAddress: 0x2F,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);

                Left = new Location(this, 256 + 22, 0x1EA91, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x2F,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);

                Middle = new Location(this, 256 + 23, 0x1EA94, LocationType.Regular,
                    name: "Middle",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x2F,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);

                Right = new Location(this, 256 + 24, 0x1EA97, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.TwentyRupees,
                    memoryAddress: 0x2F,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);

                Bottom = new Location(this, 256 + 25, 0x1EA9A, LocationType.Regular,
                    name: "Bottom",
                    vanillaItem: ItemType.ThreeBombs,
                    memoryAddress: 0x2F,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);
            }

            public Location BackCave { get; }

            public Location Left { get; }

            public Location Middle { get; }

            public Location Right { get; }

            public Location Bottom { get; }
        }

        public class BlindsHideoutRoom : Room
        {
            public BlindsHideoutRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Blind's Hideout", metadata)
            {
                BackRoom = new Location(this, 256 + 26, 0x1EB0F, LocationType.Regular,
                    name: "Top",
                    memoryAddress: 0x11D,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);
                FarLeft = new Location(this, 256 + 27, 0x1EB18, LocationType.Regular,
                    name: "Far Left",
                    memoryAddress: 0x11D,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);
                Left = new Location(this, 256 + 28, 0x1EB12, LocationType.Regular,
                    name: "Left",
                    memoryAddress: 0x11D,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);
                Right = new Location(this, 256 + 29, 0x1EB15, LocationType.Regular,
                    name: "Right",
                    memoryAddress: 0x11D,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);
                FarRight = new Location(this, 256 + 30, 0x1EB1B, LocationType.Regular,
                    name: "Far Right",
                    memoryAddress: 0x11D,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState)
                    .Weighted(SphereOne);
            }

            public Location BackRoom { get; }

            public Location FarLeft { get; }

            public Location Left { get; }

            public Location Right { get; }

            public Location FarRight { get; }
        }

    }
}
