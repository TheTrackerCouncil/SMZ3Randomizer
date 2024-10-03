using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;

public class PalaceOfDarkness : Z3Region, IHasReward, IHasTreasure, IHasBoss
{
    public PalaceOfDarkness(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = [ItemType.KeyPD, ItemType.BigKeyPD, ItemType.MapPD, ItemType.CompassPD];

        ShooterRoom = new Location(this, LocationId.PalaceOfDarknessShooterRoom, 0x1EA5B, LocationType.Regular,
            name: "Shooter Room",
            vanillaItem: ItemType.KeyPD,
            memoryAddress: 0x9,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.PalaceOfDarknessBigKeyChest, 0x1EA37, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyPD,
                access: items => BigKeyChest != null && items.KeyPD >= (BigKeyChest.ItemIs(ItemType.KeyPD, World) ? 1 :
                    (items.Hammer && items.Bow && Logic.CanPassSwordOnlyDarkRooms(items)) || config.ZeldaKeysanity ? 6 : 5),
                memoryAddress: 0x3A,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .AlwaysAllow((item, items) => item.Is(ItemType.KeyPD, World) && items.KeyPD >= 5);

        StalfosBasement = new Location(this, LocationId.PalaceOfDarknessStalfosBasement, 0x1EA49, LocationType.Regular,
            name: "Stalfos Basement",
            vanillaItem: ItemType.KeyPD,
            access: items => items.KeyPD >= 1 || (items.Bow && items.Hammer),
            memoryAddress: 0xA,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        ArenaBridge = new Location(this, LocationId.PalaceOfDarknessTheArenaBridge, 0x1EA3D, LocationType.Regular,
            name: "The Arena - Bridge",
            vanillaItem: ItemType.KeyPD,
            access: items => items.KeyPD >= 1 || (items.Bow && items.Hammer),
            memoryAddress: 0x2A,
            memoryFlag: 0x5,
            metadata: metadata,
            trackerState: trackerState);

        ArenaLedge = new Location(this, LocationId.PalaceOfDarknessTheArenaLedge, 0x1EA3A, LocationType.Regular,
            name: "The Arena - Ledge",
            vanillaItem: ItemType.KeyPD,
            access: items => items.Bow,
            memoryAddress: 0x2A,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        MapChest = new Location(this, LocationId.PalaceOfDarknessMapChest, 0x1EA52, LocationType.Regular,
            name: "Map Chest",
            vanillaItem: ItemType.MapPD,
            access: items => items.Bow,
            memoryAddress: 0x2B,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        CompassChest = new Location(this, LocationId.PalaceOfDarknessCompassChest, 0x1EA43, LocationType.Regular,
            name: "Compass Chest",
            vanillaItem: ItemType.CompassPD,
            access: items => items.KeyPD >= ((items.Hammer && items.Bow && Logic.CanPassSwordOnlyDarkRooms(items)) || config.ZeldaKeysanity ? 4 : 3),
            memoryAddress: 0x1A,
            memoryFlag: 0x5,
            metadata: metadata,
            trackerState: trackerState);

        HarmlessHellway = new Location(this, LocationId.PalaceOfDarknessHarmlessHellway, 0x1EA46, LocationType.Regular,
                name: "Harmless Hellway",
                vanillaItem: ItemType.FiveRupees,
                access: items => HarmlessHellway != null && items.KeyPD >= (HarmlessHellway.ItemIs(ItemType.KeyPD, World) ?
                    (items.Hammer && items.Bow && Logic.CanPassSwordOnlyDarkRooms(items)) || config.ZeldaKeysanity ? 4 : 3 :
                    (items.Hammer && items.Bow && Logic.CanPassSwordOnlyDarkRooms(items)) || config.ZeldaKeysanity ? 6 : 5),
                memoryAddress: 0x1A,
                memoryFlag: 0x6,
                metadata: metadata,
                trackerState: trackerState)
            .AlwaysAllow((item, items) => item.Is(ItemType.KeyPD, World) && items.KeyPD >= 5);

        BigChest = new Location(this, LocationId.PalaceOfDarknessBigChest, 0x1EA40, LocationType.Regular,
            name: "Big Chest",
            vanillaItem: ItemType.Hammer,
            access: items => items.BigKeyPD && Logic.CanPassSwordOnlyDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || config.ZeldaKeysanity ? 6 : 5),
            memoryAddress: 0x1A,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        HelmasaurKingReward = new Location(this, LocationId.PalaceOfDarknessHelmasaurKing, 0x308153, LocationType.Regular,
            name: "Helmasaur King",
            vanillaItem: ItemType.HeartContainer,
            access: items => Logic.CanPassSwordOnlyDarkRooms(items) && items.Hammer && items.Bow && items.BigKeyPD && items.KeyPD >= 6,
            memoryAddress: 0x5A,
            memoryFlag: 0xB,
            metadata: metadata,
            trackerState: trackerState);

        DarkMaze = new DarkMazeRoom(this, metadata, trackerState);
        DarkBasement = new DarkBasementRoom(this, metadata, trackerState);

        MemoryAddress = 0x5A;
        MemoryFlag = 0xB;
        StartingRooms = new List<int> { 74 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Palace of Darkness");
        MapName = "Dark World";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Palace of Darkness";

    public RewardType DefaultRewardType => RewardType.CrystalBlue;

    public BossType DefaultBossType => BossType.HelmasaurKing;

    public bool IsShuffledReward => true;

    public override string Area => "Dark Palace";

    public LocationId? BossLocationId => LocationId.PalaceOfDarknessHelmasaurKing;

    public Reward Reward { get; set; } = null!;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public TrackerRewardState RewardState { get; set; } = null!;

    public TrackerBossState BossState { get; set; } = null!;

    public Boss Boss { get; set; } = null!;

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

    public bool CanBeatBoss(Progression items) => HelmasaurKingReward.IsAvailable(items);

    public bool CanRetrieveReward(Progression items) => HelmasaurKingReward.IsAvailable(items);

    public bool CanSeeReward(Progression items) => !World.Config.ZeldaKeysanity || items.Contains(ItemType.MapPD);

    public class DarkMazeRoom : Room
    {
        public DarkMazeRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Dark Maze", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.PalaceOfDarknessDarkMazeTop, 0x1EA55, LocationType.Regular,
                    name: "Top",
                    vanillaItem: ItemType.ThreeBombs,
                    access: items => Logic.CanPassSwordOnlyDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.ZeldaKeysanity ? 6 : 5),
                    memoryAddress: 0x19,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.PalaceOfDarknessDarkMazeBottom, 0x1EA58, LocationType.Regular,
                    name: "Bottom",
                    vanillaItem: ItemType.KeyPD,
                    access: items => Logic.CanPassSwordOnlyDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.ZeldaKeysanity ? 6 : 5),
                    memoryAddress: 0x19,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class DarkBasementRoom : Room
    {
        public DarkBasementRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Dark Basement", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.PalaceOfDarknessDarkBasementLeft, 0x1EA4C, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TenArrows,
                    access: items => Logic.CanPassFireRodDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.ZeldaKeysanity ? 4 : 3),
                    memoryAddress: 0x6A,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.PalaceOfDarknessDarkBasementRight, 0x1EA4F, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.KeyPD,
                    access: items => Logic.CanPassFireRodDarkRooms(items) && items.KeyPD >= ((items.Hammer && items.Bow) || Config.ZeldaKeysanity ? 4 : 3),
                    memoryAddress: 0x6A,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }
}
