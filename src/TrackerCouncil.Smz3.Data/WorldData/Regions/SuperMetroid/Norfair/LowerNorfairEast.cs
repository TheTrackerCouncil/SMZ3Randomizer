using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Norfair;

public class LowerNorfairEast : SMRegion, IHasBoss, IHasReward
{
    public LowerNorfairEast(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        SpringBallMaze = new SpringBallMazeRoom(this, metadata, trackerState);
        EscapePowerBomb = new EscapePowerBombRoom(this, metadata, trackerState);
        Wasteland = new WastelandRoom(this, metadata, trackerState);
        ThreeMusketeers = new ThreeMusketeersRoom(this, metadata, trackerState);
        RidleyTank = new RidleyTankRoom(this, metadata, trackerState);
        Fireflea = new FirefleaRoom(this, metadata, trackerState);
        MemoryRegionId = 2;
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Lower Norfair East");
        MapName = "Norfair";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Lower Norfair, East";

    public override string Area => "Lower Norfair";

    public Boss Boss { get; set; } = null!;

    public BossType DefaultBossType => BossType.Ridley;

    public LocationId? BossLocationId => LocationId.LowerNorfairRidleyTank;

    public bool UnifiedBossAndItemLocation => false;

    public Reward Reward { get; set; } = null!;

    public RewardType DefaultRewardType => RewardType.RidleyToken;

    public TrackerRewardState RewardState { get; set; } = null!;

    public bool IsShuffledReward => false;

    public SpringBallMazeRoom SpringBallMaze { get; }

    public EscapePowerBombRoom EscapePowerBomb { get; }

    public WastelandRoom Wasteland { get; }

    public ThreeMusketeersRoom ThreeMusketeers { get; }

    public RidleyTankRoom RidleyTank { get; }

    public FirefleaRoom Fireflea { get; }

    public override bool CanEnter(Progression items, bool requireRewards) => items.Varia && items.CardLowerNorfairL1 && (
        // Access via elevator from upper norfair east past Ridley's mouth
        (World.UpperNorfairEast.CanEnter(items, requireRewards) && Logic.CanUsePowerBombs(items) && Logic.CanFly(items) && items.Gravity) ||
        // Access via Zelda portal and passing worst room in the game
        (Logic.CanAccessNorfairLowerPortal(items) && Logic.CanDestroyBombWalls(items) && items.Super && Logic.CanUsePowerBombs(items) && (
            Logic.CanWallJump(WallJumpDifficulty.Insane) ||
            (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Hard)) ||
            Logic.CanFly(items)
        ))
    );

    public bool CanBeatBoss(Progression items)
    {
        return CanEnter(items, true) && CanExit(items) && items.CardLowerNorfairBoss && Logic.CanUsePowerBombs(items) && items.Super;
    }

    public bool CanRetrieveReward(Progression items) => CanBeatBoss(items);

    public bool CanSeeReward(Progression items) => true;

    private bool CanExit(Progression items)
    {
        return items.CardNorfairL2 /*Bubble Mountain*/ ||
               items is { Gravity: true, SpaceJump: true } /* Path back to Mire*/ ||
               (items is { Gravity: true, Wave: true } /* Volcano Room and Blue Gate */ && (items.Grapple || items.SpaceJump /*Spikey Acid Snakes and Croc Escape*/));
    }

    public class SpringBallMazeRoom : Room
    {
        public SpringBallMazeRoom(LowerNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Spring Ball Maze Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.LowerNorfairSpringBallMaze, 0x8F8FCA, LocationType.Visible,
                    name: "Missile (lower Norfair above fire flea room)",
                    vanillaItem: ItemType.Missile,
                    access: items => region.CanExit(items),
                    memoryAddress: 0x9,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class EscapePowerBombRoom : Room
    {
        public EscapePowerBombRoom(LowerNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Escape Power Bomb Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.LowerNorfairEscapePowerBomb, 0x8F8FD2, LocationType.Visible,
                    name: "Power Bomb (lower Norfair above fire flea room)",
                    vanillaItem: ItemType.PowerBomb,
                    access: items => region.CanExit(items),
                    memoryAddress: 0x9,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class WastelandRoom : Room
    {
        public WastelandRoom(LowerNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Wasteland", metadata, "Power Bomb of Shame Room")
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.LowerNorfairWasteland, 0x8F90C0, LocationType.Visible,
                    name: "Power Bomb (Power Bombs of shame)",
                    vanillaItem: ItemType.PowerBomb,
                    access: items => region.CanExit(items) && Logic.CanUsePowerBombs(items),
                    memoryAddress: 0x9,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class ThreeMusketeersRoom : Room
    {
        public ThreeMusketeersRoom(LowerNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Three Musketeers' Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.LowerNorfairThreeMusketeers, 0x8F9100, LocationType.Visible,
                    name: "Missile (lower Norfair near Wave Beam)",
                    vanillaItem: ItemType.Missile,
                    access: region.CanExit,
                    memoryAddress: 0x9,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class RidleyTankRoom : Room
    {
        public RidleyTankRoom(LowerNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Ridley Tank Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.LowerNorfairRidleyTank, 0x8F9108, LocationType.Hidden,
                    name: "Energy Tank, Ridley",
                    vanillaItem: ItemType.ETank,
                    access: items => items.Ridley,
                    relevanceRequirement: region.CanBeatBoss,
                    memoryAddress: 0x9,
                    memoryFlag: 0x40,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class FirefleaRoom : Room
    {
        public FirefleaRoom(LowerNorfairEast region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Lower Norfair Fireflea Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.LowerNorfairFireflea, 0x8F9184, LocationType.Visible,
                    name: "Energy Tank, Firefleas",
                    vanillaItem: ItemType.ETank,
                    access: region.CanExit,
                    memoryAddress: 0xA,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }
}
