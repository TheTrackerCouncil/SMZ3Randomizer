using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld;

public class LightWorldNorthWest : Z3Region
{
    public const int SphereOne = -14;

    public LightWorldNorthWest(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        MasterSwordPedestal = new Location(this, LocationId.MasterSwordPedestal, 0x589B0, LocationType.Pedestal,
                name: "Master Sword Pedestal",
                vanillaItem: ItemType.ProgressiveSword,
                access: items => items.AllPendants,
                relevanceRequirement: items => World.CanAquireAll(items, RewardType.PendantGreen, RewardType.PendantBlue, RewardType.PendantRed),
                memoryAddress: 0x80,
                memoryFlag: 0x40,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState);

        Mushroom = new Location(this, LocationId.Mushroom, 0x308013, LocationType.Regular,
                name: "Mushroom",
                vanillaItem: ItemType.Mushroom,
                memoryAddress: 0x191,
                memoryFlag: 0x10,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState)
            .Weighted(SphereOne);

        LostWoodsHideout = new Location(this, LocationId.LostWoodsHideout, 0x308000, LocationType.Regular,
                name: "Lost Woods Hideout",
                vanillaItem: ItemType.HeartPiece,
                memoryAddress: 0xE1,
                memoryFlag: 0x9,
                metadata: metadata,
                trackerState: trackerState)
            .Weighted(SphereOne);

        LumberjackTree = new Location(this, LocationId.LumberjackTree, 0x308001, LocationType.Regular,
                name: "Lumberjack Tree",
                vanillaItem: ItemType.HeartPiece,
                access: items => Logic.CheckAgahnim(items, World, requireRewards: true) && items.Boots,
                relevanceRequirement: items => Logic.CheckAgahnim(items, World, requireRewards: false) && items.Boots,
                memoryAddress: 0xE2,
                memoryFlag: 0x9,
                metadata: metadata,
                trackerState: trackerState);

        PegasusRocks = new Location(this, LocationId.PegasusRocks, 0x1EB3F, LocationType.Regular,
            name: "Pegasus Rocks",
            vanillaItem: ItemType.HeartPiece,
            access: items => items.Boots,
            memoryAddress: 0x124,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        GraveyardLedge = new Location(this, LocationId.GraveyardLedge, 0x308004, LocationType.Regular,
            name: "Graveyard Ledge",
            vanillaItem: ItemType.HeartPiece,
            access: items => items.Mirror && World.Logic.CanNavigateDarkWorld(items) && World.DarkWorldNorthWest.CanEnter(items, true),
            relevanceRequirement: items => items.Mirror && World.Logic.CanNavigateDarkWorld(items) && World.DarkWorldNorthWest.CanEnter(items, false),
            memoryAddress: 0x11B,
            memoryFlag: 0x9,
            metadata: metadata,
            trackerState: trackerState);

        KingsTomb = new Location(this, LocationId.KingsTomb, 0x1E97A, LocationType.Regular,
            name: "King's Tomb",
            vanillaItem: ItemType.Cape,
            access: items => items.Boots && (
                Logic.CanLiftHeavy(items) ||
                (items.Mirror && World.Logic.CanNavigateDarkWorld(items) && World.DarkWorldNorthWest.CanEnter(items, true))),
            relevanceRequirement: items => items.Boots && (
                Logic.CanLiftHeavy(items) ||
                (items.Mirror && World.Logic.CanNavigateDarkWorld(items) && World.DarkWorldNorthWest.CanEnter(items, false))),
            memoryAddress: 0x113,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BottleMerchant = new Location(this, LocationId.BottleMerchant, 0x5EB18, LocationType.Regular,
                name: "Bottle Merchant",
                vanillaItem: ItemType.Bottle,
                memoryAddress: 0x149,
                memoryFlag: 0x2,
                memoryType: LocationMemoryType.ZeldaMisc,
                metadata: metadata,
                trackerState: trackerState)
            .Weighted(SphereOne);

        ChickenHouse = new Location(this, LocationId.ChickenHouse, 0x1E9E9, LocationType.Regular,
                name: "Chicken House",
                vanillaItem: ItemType.TenArrows,
                memoryAddress: 0x108,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .Weighted(SphereOne);

        SickKid = new Location(this, LocationId.SickKid, 0x6B9CF, LocationType.Regular,
            name: "Sick Kid",
            vanillaItem: ItemType.Bugnet,
            access: items => items.Bottle,
            memoryAddress: 0x190,
            memoryFlag: 0x4,
            memoryType: LocationMemoryType.ZeldaMisc,
            metadata: metadata,
            trackerState: trackerState);

        TavernBackRoom = new Location(this, LocationId.KakarikoTavern, 0x1E9CE, LocationType.Regular,
                name: "Kakariko Tavern",
                vanillaItem: ItemType.Bottle,
                memoryAddress: 0x103,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .Weighted(SphereOne);

        Blacksmith = new Location(this, LocationId.Blacksmith, 0x30802A, LocationType.Regular,
            name: "Blacksmith",
            vanillaItem: ItemType.ProgressiveSword,
            access: items => World.DarkWorldNorthWest.CanEnter(items, true) && Logic.CanLiftHeavy(items),
            relevanceRequirement: items => World.DarkWorldNorthWest.CanEnter(items, false) && Logic.CanLiftHeavy(items),
            memoryAddress: 0x191,
            memoryFlag: 0x4,
            memoryType: LocationMemoryType.ZeldaMisc,
            metadata: metadata,
            trackerState: trackerState);

        MagicBat = new Location(this, LocationId.MagicBat, 0x308015, LocationType.Regular,
            name: "Magic Bat",
            vanillaItem: ItemType.HalfMagic,
            access: items => items.Powder
                             && (items.Hammer
                                 || (World.Logic.CanNavigateDarkWorld(items) && items.Mirror && Logic.CanLiftHeavy(items))),
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
        MapName = "Light World";
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
            Locations = new List<Location>
            {
                new Location(this, LocationId.KakarikoWellTop, 0x1EA8E, LocationType.Regular,
                        name: "Top",
                        vanillaItem: ItemType.HeartPiece,
                        memoryAddress: 0x2F,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne),
                new Location(this, LocationId.KakarikoWellLeft, 0x1EA91, LocationType.Regular,
                        name: "Left",
                        vanillaItem: ItemType.TwentyRupees,
                        memoryAddress: 0x2F,
                        memoryFlag: 0x5,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne),
                new Location(this, LocationId.KakarikoWellMiddle, 0x1EA94, LocationType.Regular,
                        name: "Middle",
                        vanillaItem: ItemType.TwentyRupees,
                        memoryAddress: 0x2F,
                        memoryFlag: 0x6,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne),
                new Location(this, LocationId.KakarikoWellRight, 0x1EA97, LocationType.Regular,
                        name: "Right",
                        vanillaItem: ItemType.TwentyRupees,
                        memoryAddress: 0x2F,
                        memoryFlag: 0x7,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne),
                new Location(this, LocationId.KakarikoWellBottom, 0x1EA9A, LocationType.Regular,
                        name: "Bottom",
                        vanillaItem: ItemType.ThreeBombs,
                        memoryAddress: 0x2F,
                        memoryFlag: 0x8,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne)
            };
        }
    }

    public class BlindsHideoutRoom : Room
    {
        public BlindsHideoutRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Blind's Hideout", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.BlindsHideoutTop, 0x1EB0F, LocationType.Regular,
                        name: "Top",
                        memoryAddress: 0x11D,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne),
                new Location(this, LocationId.BlindsHideoutFarLeft, 0x1EB18, LocationType.Regular,
                        name: "Far Left",
                        memoryAddress: 0x11D,
                        memoryFlag: 0x7,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne),
                new Location(this, LocationId.BlindsHideoutLeft, 0x1EB12, LocationType.Regular,
                        name: "Left",
                        memoryAddress: 0x11D,
                        memoryFlag: 0x5,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne),
                new Location(this, LocationId.BlindsHideoutRight, 0x1EB15, LocationType.Regular,
                        name: "Right",
                        memoryAddress: 0x11D,
                        memoryFlag: 0x6,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne),
                new Location(this, LocationId.BlindsHideoutFarRight, 0x1EB1B, LocationType.Regular,
                        name: "Far Right",
                        memoryAddress: 0x11D,
                        memoryFlag: 0x8,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Weighted(SphereOne)
            };
        }
    }
}
