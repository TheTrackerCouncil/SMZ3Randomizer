using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;

public class GanonsTower : Z3Region, IHasTreasure, IHasBoss
{
    public GanonsTower(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = [ItemType.KeyGT, ItemType.BigKeyGT, ItemType.MapGT, ItemType.CompassGT];

        BobsTorch = new Location(this, LocationId.GanonsTowerBobsTorch, 0x308161, LocationType.Regular,
            name: "Bob's Torch",
            vanillaItem: ItemType.KeyGT,
            access: items => items.Boots,
            memoryAddress: 0x8C,
            memoryFlag: 0xA,
            metadata: metadata,
            trackerState: trackerState);

        MapChest = new Location(this, LocationId.GanonsTowerMapChest, 0x1EAD3, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapGT,
                access: items => items.Hammer && (items.Hookshot || items.Boots) && items.KeyGT >=
                    (new[] { ItemType.BigKeyGT, ItemType.KeyGT }.Any(type => MapChest != null && MapChest.ItemIs(type, World)) ? 3 : 4),
                memoryAddress: 0x8B,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .AlwaysAllow((item, items) => item.Is(ItemType.KeyGT, World) && items.KeyGT >= 3);

        FiresnakeRoom = new Location(this, LocationId.GanonsTowerFiresnakeRoom, 0x1EAD0, LocationType.Regular,
            name: "Firesnake Room",
            vanillaItem: ItemType.KeyGT,
            access: items => FiresnakeRoom != null && RandomizerRoom != null && items.Hammer && items.Hookshot && items.KeyGT >= (new[] {
                    LocationId.GanonsTowerRandomizerRoomTopRight,
                    LocationId.GanonsTowerRandomizerRoomTopLeft,
                    LocationId.GanonsTowerRandomizerRoomBottomLeft,
                    LocationId.GanonsTowerRandomizerRoomBottomRight
                }.Any(l => world.FindLocation(l).ItemIs(ItemType.BigKeyGT, World))
                || FiresnakeRoom.ItemIs(ItemType.KeyGT, World) ? 2 : 3),
            memoryAddress: 0x7D,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        TileRoom = new Location(this, LocationId.GanonsTowerTileRoom, 0x1EAE2, LocationType.Regular,
            name: "Tile Room",
            vanillaItem: ItemType.KeyGT,
            access: items => items.Somaria,
            memoryAddress: 0x8D,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BobsChest = new Location(this, LocationId.GanonsTowerBobsChest, 0x1EADF, LocationType.Regular,
            name: "Bob's Chest",
            vanillaItem: ItemType.TenArrows,
            access: items => items.KeyGT >= 3 && (
                (items.Hammer && items.Hookshot) ||
                (items.Somaria && items.FireRod)),
            memoryAddress: 0x8C,
            memoryFlag: 0x7,
            metadata: metadata,
            trackerState: trackerState);

        BigChest = new Location(this, LocationId.GanonsTowerBigChest, 0x1EAD6, LocationType.Regular,
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

        PreMoldormChest = new Location(this, LocationId.GanonsTowerPreMoldormChest, 0x1EB03, LocationType.Regular,
                name: "Pre-Moldorm Chest",
                vanillaItem: ItemType.KeyGT,
                access: TowerAscend,
                memoryAddress: 0x3D,
                memoryFlag: 0x6,
                metadata: metadata,
                trackerState: trackerState)
            .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World));

        MoldormChest = new Location(this, LocationId.GanonsTowerMoldormChest, 0x1EB06, LocationType.Regular,
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
        MapName = "Dark World";

        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Ganon's Tower";

    public LocationId? BossLocationId => null;

    public BossType DefaultBossType => BossType.Ganon;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public event EventHandler? UpdatedTreasure;

    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public Boss Boss { get; set; } = null!;

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

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        var smBosses = new[] { BossType.Kraid, BossType.Phantoon, BossType.Draygon, BossType.Ridley };
        var canEnterDDMEast = World.DarkWorldDeathMountainEast.CanEnter(items, requireRewards);
        var haveEnoughCrystals = items.CrystalCount >= Config.GanonsTowerCrystalCount;
        var gtOpenBeforeGanon = Config.GanonsTowerCrystalCount < Config.GanonCrystalCount;
        var canBeatMetroid = World.CanDefeatBossCount(items, smBosses) >= Config.TourianBossCount;
        return items.MoonPearl && canEnterDDMEast && haveEnoughCrystals && (gtOpenBeforeGanon || canBeatMetroid);
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

    public bool CanBeatBoss(Progression items) => MoldormChest.IsAvailable(items) && items.MasterSword &&
                                                  items.Contains(ItemType.SilverArrows);

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
            Locations = new List<Location>
            {
                new Location(this, LocationId.GanonsTowerDMsRoomTopLeft, 0x1EAB8, LocationType.Regular,
                    name: "Top Left",
                    access: items => items.Hammer && items.Hookshot,
                    memoryAddress: 0x7B,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerDMsRoomTopRight, 0x1EABB, LocationType.Regular,
                    name: "Top Right",
                    access: items => items.Hammer && items.Hookshot,
                    memoryAddress: 0x7B,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerDMsRoomBottomLeft, 0x1EABE, LocationType.Regular,
                    name: "Bottom Left",
                    access: items => items.Hammer && items.Hookshot,
                    memoryAddress: 0x7B,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerDMsRoomBottomRight, 0x1EAC1, LocationType.Regular,
                    name: "Bottom Right",
                    access: items => items.Hammer && items.Hookshot,
                    memoryAddress: 0x7B,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class RandomizerRoomRoom : Room
    {
        public RandomizerRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Randomizer Room", metadata) // The room with all the floor tiles
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.GanonsTowerRandomizerRoomTopLeft, 0x1EAC4, LocationType.Regular,
                    name: "Top Left",
                    access: items => LeftSide(items, LocationId.GanonsTowerRandomizerRoomTopLeft),
                    memoryAddress: 0x7C,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerRandomizerRoomTopRight, 0x1EAC7, LocationType.Regular,
                    name: "Top Right",
                    access: items => LeftSide(items, LocationId.GanonsTowerRandomizerRoomTopRight),
                    memoryAddress: 0x7C,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerRandomizerRoomBottomLeft, 0x1EACA, LocationType.Regular,
                    name: "Bottom Left",
                    access: items => LeftSide(items, LocationId.GanonsTowerRandomizerRoomBottomLeft),
                    memoryAddress: 0x7C,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerRandomizerRoomBottomRight, 0x1EACD, LocationType.Regular,
                    name: "Bottom Right",
                    access: items => LeftSide(items, LocationId.GanonsTowerRandomizerRoomBottomRight),
                    memoryAddress: 0x7C,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }

        private bool LeftSide(Progression items, LocationId except)
        {
            return items.Hammer && items.Hookshot && items.KeyGT >= (Locations.Any(l => l.Id != except && l.ItemIs(ItemType.BigKeyGT, World)) ? 3 : 4);
        }
    }

    public class HopeRoomRoom : Room
    {
        public HopeRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Hope Room", metadata, "Right Side First Room")
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.GanonsTowerHopeRoomLeft, 0x1EAD9, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TenArrows,
                    memoryAddress: 0x8C,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerHopeRoomRight, 0x1EADC, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.ThreeBombs,
                    memoryAddress: 0x8C,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class CompassRoomRoom : Room
    {
        public CompassRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Compass Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.GanonsTowerCompassRoomTopLeft, 0x1EAE5, LocationType.Regular,
                    name: "Top Left",
                    vanillaItem: ItemType.CompassGT,
                    access: items => RightSide(items, LocationId.GanonsTowerCompassRoomTopLeft),
                    memoryAddress: 0x9D,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerCompassRoomTopRight, 0x1EAE8, LocationType.Regular,
                    name: "Top Right",
                    access: items => RightSide(items, LocationId.GanonsTowerCompassRoomTopRight),
                    memoryAddress: 0x9D,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerCompassRoomBottomLeft, 0x1EAEB, LocationType.Regular,
                    name: "Bottom Left",
                    access: items => RightSide(items, LocationId.GanonsTowerCompassRoomBottomLeft),
                    memoryAddress: 0x9D,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerCompassRoomBottomRight, 0x1EAEE, LocationType.Regular,
                    name: "Bottom Right",
                    access: items => RightSide(items, LocationId.GanonsTowerCompassRoomBottomRight),
                    memoryAddress: 0x9D,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }

        private bool RightSide(Progression items, LocationId except)
        {
            return items.Somaria && items.FireRod && items.KeyGT >= (Locations.Any(l => l.Id != except && l.ItemIs(ItemType.BigKeyGT, World)) ? 3 : 4);
        }
    }

    public class BigKeyRoomRoom : Room
    {
        public BigKeyRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Big Key Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.GanonsTowerBigKeyChest, 0x1EAF1, LocationType.Regular,
                    name: "Bottom Big Key Chest",
                    vanillaItem: ItemType.BigKeyGT,
                    access: BigKeyRoom,
                    memoryAddress: 0x1C,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerBigKeyRoomLeft, 0x1EAF4, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TenArrows,
                    access: BigKeyRoom,
                    memoryAddress: 0x1C,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.GanonsTowerBigKeyRoomRight, 0x1EAF7, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.ThreeBombs,
                    access: BigKeyRoom,
                    memoryAddress: 0x1C,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }

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

            Locations = new List<Location>
            {
                new Location(this, LocationId.GanonsTowerMiniHelmasaurRoomLeft, 0x1EAFD, LocationType.Regular,
                        name: "Left",
                        vanillaItem: ItemType.ThreeBombs,
                        access: tower.TowerAscend,
                        memoryAddress: 0x3D,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World)),
                new Location(this, LocationId.GanonsTowerMiniHelmasaurRoomRight, 0x1EB00, LocationType.Regular,
                        name: "Right",
                        vanillaItem: ItemType.ThreeBombs,
                        access: tower.TowerAscend,
                        memoryAddress: 0x3D,
                        memoryFlag: 0x5,
                        metadata: metadata,
                        trackerState: trackerState)
                    .Allow((item, items) => item.IsNot(ItemType.BigKeyGT, World))
            };
        }
    }
}
