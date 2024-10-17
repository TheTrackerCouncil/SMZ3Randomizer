using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Maridia;

public class InnerMaridia : SMRegion, IHasBoss, IHasReward
{
    public InnerMaridia(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        WateringHole = new WateringHoleRoom(this, metadata, trackerState);
        PseudoPlasmaSpark = new PseudoPlasmaSparkRoom(this, metadata, trackerState);
        Plasma = new PlasmaRoom(this, metadata, trackerState);
        LeftSandPit = new LeftSandPitRoom(this, metadata, trackerState);
        RightSandPit = new RightSandPitRoom(this, metadata, trackerState);
        Aqueduct = new AqueductRoom(this, metadata, trackerState);
        SpringBall = new SpringBallRoom(this, metadata, trackerState);
        ThePrecious = new ThePreciousRoom(this, metadata, trackerState);
        Botwoons = new BotwoonsRoom(this, metadata, trackerState);
        SpaceJump = new SpaceJumpRoom(this, metadata, trackerState);
        MemoryRegionId = 4;
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Inner Maridia");
        MapName = "Maridia";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Inner Maridia";

    public override string Area => "Maridia";

    public Boss Boss { get; set; } = null!;

    public BossType DefaultBossType => BossType.Draygon;

    public LocationId? BossLocationId => LocationId.InnerMaridiaSpaceJump;

    public Reward Reward { get; set; } = null!;

    public RewardType DefaultRewardType => RewardType.DraygonToken;

    public TrackerRewardState RewardState { get; set; } = null!;

    public bool IsShuffledReward => false;

    public WateringHoleRoom WateringHole { get; }

    public PseudoPlasmaSparkRoom PseudoPlasmaSpark { get; }

    public PlasmaRoom Plasma { get; }

    public LeftSandPitRoom LeftSandPit { get; }

    public RightSandPitRoom RightSandPit { get; }

    public AqueductRoom Aqueduct { get; }

    public SpringBallRoom SpringBall { get; }

    public ThePreciousRoom ThePrecious { get; }

    public BotwoonsRoom Botwoons { get; }

    public SpaceJumpRoom SpaceJump { get; }

    public override bool CanEnter(Progression items, bool requireRewards)
        => items.Gravity && (
            (World.UpperNorfairWest.CanEnter(items, true) && items.Super && Logic.CanUsePowerBombs(items) && CanPassMountDeath(items, Logic)) ||
            Logic.CanAccessMaridiaPortal(items, requireRewards));

    public bool CanBeatBoss(Progression items)
        => CanEnter(items, true) && CanDefeatDraygon(items, true);

    public bool CanRetrieveReward(Progression items) => CanBeatBoss(items);

    public bool CanSeeReward(Progression items) => true;

    private static bool CanReachAqueduct(Progression items, ILogic logic, bool requireRewards)
        => (items.CardMaridiaL1 && CanPassMountDeath(items, logic))
           || (items.CardMaridiaL2 && logic.CanAccessMaridiaPortal(items, requireRewards));

    private bool CanAccessPreciousRoom(Progression items, bool requireRewards)
        => items.Super
           && (Logic.CanWallJump(WallJumpDifficulty.Hard) || items.Grapple || items.SpaceJump)
           && (Logic.CanAccessMaridiaPortal(items, requireRewards) || (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items, requireRewards)));

    private bool CanAccessDraygon(Progression items, bool requireRewards)
        => CanAccessPreciousRoom(items, requireRewards) && items.CardMaridiaBoss;

    private bool CanDefeatDraygon(Progression items, bool requireRewards)
        => CanAccessDraygon(items, requireRewards) && CanLeaveDraygonRoom(items);

    private bool CanLeaveDraygonRoom(Progression items)
        => (items.SpeedBooster && items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy)) || Logic.CanFly(items);

    private bool CanDefeatBotwoon(Progression items, bool requireRewards)
        => (items.SpeedBooster || Logic.CanAccessMaridiaPortal(items, requireRewards))
           && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.Grapple || Logic.CanFly(items));

    private bool CanPassPipeCrossroads(Progression items)
        => Logic.CanWallJump(WallJumpDifficulty.Medium) || items.HiJump || Logic.CanFly(items);

    private bool CanAccessPlasmaBeamRoom(Progression items, bool requireRewards) =>
        (items.Draygon || (!requireRewards && CanDefeatDraygon(items, requireRewards)))
        && (items.ScrewAttack || items.Plasma)
        && ((items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Medium)) || Logic.CanFly(items));

    // Large room on the west side of Maridia where you are intended to grapple over
    private static bool CanPassMountDeath(Progression items, ILogic logic)
    {
        return logic.CanFly(items) || items.SpeedBooster || items.Grapple || (items.HiJump && logic.CanWallJump(WallJumpDifficulty.Hard));
    }

    public class WateringHoleRoom : Room
    {
        public WateringHoleRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Watering Hole", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaWateringHoleLeft, 0x8FC4AF, LocationType.Visible,
                    name: "Left",
                    vanillaItem: ItemType.Super,
                    access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, true),
                    relevanceRequirement: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, false),
                    memoryAddress: 0x11,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState),

                new Location(this, LocationId.InnerMaridiaWateringHoleRight, 0x8FC4B5, LocationType.Visible,
                    name: "Right",
                    vanillaItem: ItemType.Missile,
                    access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, true),
                    relevanceRequirement: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, false),
                    memoryAddress: 0x11,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class PseudoPlasmaSparkRoom : Room
    {
        public PseudoPlasmaSparkRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Pseudo Plasma Spark Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaPseudoPlasmaSpark, 0x8FC533, LocationType.Visible,
                    name: "Missile (yellow Maridia false wall)",
                    vanillaItem: ItemType.Missile,
                    access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, true),
                    relevanceRequirement: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, false),
                    memoryAddress: 0x11,
                    memoryFlag: 0x40,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class PlasmaRoom : Room
    {
        public PlasmaRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Plasma Room", metadata, "Plasma Beam Room")
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaPlasma, 0x8FC559, LocationType.Chozo,
                    name: "Plasma Beam",
                    vanillaItem: ItemType.Plasma,
                    access: items => region.CanAccessPlasmaBeamRoom(items, requireRewards: true),
                    relevanceRequirement: items => region.CanAccessPlasmaBeamRoom(items, requireRewards: false),
                    memoryAddress: 0x11,
                    memoryFlag: 0x80,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class LeftSandPitRoom : Room
    {
        public LeftSandPitRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Left Sand Pit", metadata, "West Sand Hole")
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaWestSandHoleLeft, 0x8FC5DD, LocationType.Visible,
                    name: "Left",
                    vanillaItem: ItemType.Missile,
                    access: items => CanEnter(items, true) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    relevanceRequirement: items => CanEnter(items, false) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    memoryAddress: 0x12,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.InnerMaridiaWestSandHoleRight, 0x8FC5E3, LocationType.Chozo,
                    name: "Right",
                    vanillaItem: ItemType.ReserveTank,
                    access: items => CanEnter(items, true) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    relevanceRequirement: items => CanEnter(items, false) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    memoryAddress: 0x12,
                    memoryFlag: 0x2,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }

        public bool CanEnter(Progression items, bool requireRewards)
            => InnerMaridia.CanReachAqueduct(items, Logic, requireRewards) && items.Super && Logic.CanPassBombPassages(items);
    }

    public class RightSandPitRoom : Room
    {
        public RightSandPitRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Right Sand Pit", metadata, "East Sand Hole")
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaEastSandHoleLeft, 0x8FC5EB, LocationType.Visible,
                    name: "Missile (right Maridia sand pit room)",
                    vanillaItem: ItemType.Missile,
                    access: items => CanReachRightSandPit(items, true),
                    relevanceRequirement: items => CanReachRightSandPit(items, false),
                    memoryAddress: 0x12,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.InnerMaridiaEastSandHoleRight, 0x8FC5F1, LocationType.Visible,
                    name: "Power Bomb (right Maridia sand pit room)",
                    vanillaItem: ItemType.PowerBomb,
                    access: items => CanReachRightSandPit(items, true),
                    relevanceRequirement: items => CanReachRightSandPit(items, false),
                    memoryAddress: 0x12,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }

        private bool CanReachRightSandPit(Progression items, bool requireRewards)
            => CanReachAqueduct(items, Logic, requireRewards) && items.Super && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump || items.SpaceJump);
    }

    public class AqueductRoom: Room
    {
        public AqueductRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Aqueduct", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaAqueductLeft, 0x8FC603, LocationType.Visible,
                    name: "Missile (pink Maridia)",
                    vanillaItem: ItemType.Missile,
                    access: items => CanReachAqueduct(items, Logic, true) && items.SpeedBooster,
                    relevanceRequirement: items => CanReachAqueduct(items, Logic, false) && items.SpeedBooster,
                    memoryAddress: 0x12,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.InnerMaridiaAqueductRight, 0x8FC609, LocationType.Visible,
                    name: "Super Missile (pink Maridia)",
                    vanillaItem: ItemType.Super,
                    access: items => CanReachAqueduct(items, Logic, true) && items.SpeedBooster,
                    relevanceRequirement: items => CanReachAqueduct(items, Logic, false) && items.SpeedBooster,
                    memoryAddress: 0x12,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class SpringBallRoom : Room
    {
        public SpringBallRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Spring Ball Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaSpringBall, 0x8FC6E5, LocationType.Chozo,
                    name: "Spring Ball",
                    vanillaItem: ItemType.SpringBall,
                    access: items => items.Super && Logic.CanUsePowerBombs(items) && items.Grapple
                                     && (items.SpaceJump || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Medium)))
                                     && (Logic.CanWallJump(WallJumpDifficulty.Medium) || items.SpringBall || items.SpaceJump), // Leaving again
                    memoryAddress: 0x12,
                    memoryFlag: 0x40,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class ThePreciousRoom : Room
    {
        public ThePreciousRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "The Precious Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaPreciousRoom, 0x8FC74D, LocationType.Hidden,
                    name: "Missile (Draygon)",
                    vanillaItem: ItemType.Missile,
                    access: items => region.CanAccessPreciousRoom(items, true),
                    relevanceRequirement: items => region.CanAccessPreciousRoom(items, false),
                    memoryAddress: 0x12,
                    memoryFlag: 0x80,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class BotwoonsRoom : Room
    {
        public BotwoonsRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Botwoon's Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaBotwoon, 0x8FC755, LocationType.Visible,
                    name: "Energy Tank, Botwoon",
                    vanillaItem: ItemType.ETank,
                    access: items => (items.CardMaridiaL1 && items.CardMaridiaL2 && region.CanDefeatBotwoon(items, true))
                                     || (Logic.CanAccessMaridiaPortal(items, requireRewards: true) && items.CardMaridiaL2),
                    relevanceRequirement: items => (items.CardMaridiaL1 && items.CardMaridiaL2 && region.CanDefeatBotwoon(items, false))
                                                   || (Logic.CanAccessMaridiaPortal(items, requireRewards: false) && items.CardMaridiaL2),
                    memoryAddress: 0x13,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class SpaceJumpRoom : Room
    {
        public SpaceJumpRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Space Jump Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.InnerMaridiaSpaceJump, 0x8FC7A7, LocationType.Chozo,
                    name: "Space Jump",
                    vanillaItem: ItemType.SpaceJump,
                    access: items => items.Draygon,
                    relevanceRequirement: items => region.CanDefeatDraygon(items, false),
                    memoryAddress: 0x13,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }
}
