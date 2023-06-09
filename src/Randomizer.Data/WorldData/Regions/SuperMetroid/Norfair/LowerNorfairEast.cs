using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using System.Collections.Generic;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Norfair
{
    public class LowerNorfairEast : SMRegion, IHasBoss
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
            Boss = new Boss(Shared.Enums.BossType.Ridley, world, this, metadata, trackerState);
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Lower Norfair East");
        }

        public override string Name => "Lower Norfair, East";

        public override string Area => "Lower Norfair";

        public Boss Boss { get; set; }

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

        private bool CanExit(Progression items)
        {
            return items.CardNorfairL2 /*Bubble Mountain*/ ||
                    (items.Gravity && items.Wave /* Volcano Room and Blue Gate */ && (items.Grapple || items.SpaceJump /*Spikey Acid Snakes and Croc Escape*/));
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
}
