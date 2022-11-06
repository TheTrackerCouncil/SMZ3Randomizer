using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Logic;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Maridia
{
    public class InnerMaridia : SMRegion, IHasBoss
    {
        public InnerMaridia(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            PseudoSparkRoom = new Location(this, 142, 0x8FC533, LocationType.Visible,
                name: "Missile (yellow Maridia false wall)",
                alsoKnownAs: new[] { "Pseudo Plasma Spark Room" },
                vanillaItem: ItemType.Missile,
                access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, true),
                relevanceRequirement: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, false),
                memoryAddress: 0x11,
                memoryFlag: 0x40,
                metadata: metadata,
                trackerState: trackerState);
            PlasmaBeamRoom = new Location(this, 143, 0x8FC559, LocationType.Chozo,
                name: "Plasma Beam",
                vanillaItem: ItemType.Plasma,
                access: items => CanAccessPlasmaBeamRoom(items, requireRewards: true),
                relevanceRequirement: items => CanAccessPlasmaBeamRoom(items, requireRewards: false),
                memoryAddress: 0x11,
                memoryFlag: 0x80,
                metadata: metadata,
                trackerState: trackerState);
            RightSandPitLeft = new Location(this, 146, 0x8FC5EB, LocationType.Visible,
                name: "Missile (right Maridia sand pit room)",
                alsoKnownAs: new[] { "Right Sand Pit - Left item" },
                vanillaItem: ItemType.Missile,
                access: items => CanReachRightSandPit(items, true),
                relevanceRequirement: items => CanReachRightSandPit(items, false),
                memoryAddress: 0x12,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);
            RightSandPitRight = new Location(this, 147, 0x8FC5F1, LocationType.Visible,
                name: "Power Bomb (right Maridia sand pit room)",
                alsoKnownAs: new[] { "Right Sand Pit - Right item" },
                vanillaItem: ItemType.PowerBomb,
                access: items => CanReachRightSandPit(items, true),
                relevanceRequirement: items => CanReachRightSandPit(items, false),
                memoryAddress: 0x12,
                memoryFlag: 0x8,
                metadata: metadata,
                trackerState: trackerState);
            AqueductLeft = new Location(this, 148, 0x8FC603, LocationType.Visible,
                name: "Missile (pink Maridia)",
                alsoKnownAs: new[] { "Aqueduct - Left item" },
                vanillaItem: ItemType.Missile,
                access: items => CanReachAqueduct(items, Logic, true) && items.SpeedBooster,
                relevanceRequirement: items => CanReachAqueduct(items, Logic, false) && items.SpeedBooster,
                memoryAddress: 0x12,
                memoryFlag: 0x10,
                metadata: metadata,
                trackerState: trackerState);
            AqueductRight = new Location(this, 149, 0x8FC609, LocationType.Visible,
                name: "Super Missile (pink Maridia)",
                alsoKnownAs: new[] { "Aqueduct - Right item" },
                vanillaItem: ItemType.Super,
                access: items => CanReachAqueduct(items, Logic, true) && items.SpeedBooster,
                relevanceRequirement: items => CanReachAqueduct(items, Logic, false) && items.SpeedBooster,
                memoryAddress: 0x12,
                memoryFlag: 0x20,
                metadata: metadata,
                trackerState: trackerState);
            ShaktoolItem = new Location(this, 150, 0x8FC6E5, LocationType.Chozo,
                name: "Spring Ball",
                alsoKnownAs: new[] { "Shaktool's item" },
                vanillaItem: ItemType.SpringBall,
                access: items => items.Super && Logic.CanUsePowerBombs(items) && items.Grapple
                              && (items.SpaceJump || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Medium)))
                              && (Logic.CanWallJump(WallJumpDifficulty.Medium) || items.SpringBall || items.SpaceJump), // Leaving again
                memoryAddress: 0x12,
                memoryFlag: 0x40,
                metadata: metadata,
                trackerState: trackerState);
            PreDraygonRoom = new Location(this, 151, 0x8FC74D, LocationType.Hidden,
                name: "Missile (Draygon)",
                alsoKnownAs: new[] { "Pre-Draygon Room", "The Precious Room" },
                vanillaItem: ItemType.Missile,
                access: items => CanAccessPreciousRoom(items, true),
                relevanceRequirement: items => CanAccessPreciousRoom(items, false),
                memoryAddress: 0x12,
                memoryFlag: 0x80,
                metadata: metadata,
                trackerState: trackerState);
            Botwoon = new Location(this, 152, 0x8FC755, LocationType.Visible,
                name: "Energy Tank, Botwoon",
                alsoKnownAs: new[] { "Sandy Path" },
                vanillaItem: ItemType.ETank,
                access: items => (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items, true))
                                  || (Logic.CanAccessMaridiaPortal(items, requireRewards: true) && items.CardMaridiaL2),
                relevanceRequirement: items => (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items, false))
                                  || (Logic.CanAccessMaridiaPortal(items, requireRewards: false) && items.CardMaridiaL2),
                memoryAddress: 0x13,
                memoryFlag: 0x1,
                metadata: metadata,
                trackerState: trackerState);
            DraygonTreasure = new Location(this, 154, 0x8FC7A7, LocationType.Chozo,
                name: "Space Jump",
                alsoKnownAs: new[] { "Draygon's Reliquary" },
                vanillaItem: ItemType.SpaceJump,
                access: items => items.Draygon,
                relevanceRequirement: items => CanDefeatDraygon(items, false),
                memoryAddress: 0x13,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);

            WateringHole = new WateringHoleRoom(this, metadata, trackerState);
            LeftSandPit = new LeftSandPitRoom(this, metadata, trackerState);

            MemoryRegionId = 4;
            Boss = new Boss(Shared.Enums.BossType.Draygon, world, this, metadata, trackerState);
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Inner Maridia");
        }

        public override string Name => "Inner Maridia";

        public override string Area => "Maridia";

        public Boss Boss{ get; set; }

        public Location PseudoSparkRoom { get; }

        public Location PlasmaBeamRoom { get; }

        public Location RightSandPitLeft { get; }

        public Location RightSandPitRight { get; }

        public Location AqueductLeft { get; }

        public Location AqueductRight { get; }

        public Location ShaktoolItem { get; }

        public Location PreDraygonRoom { get; }

        public Location Botwoon { get; }

        public Location DraygonTreasure { get; }

        public WateringHoleRoom WateringHole { get; }

        public LeftSandPitRoom LeftSandPit { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
            => items.Gravity && (
                    (World.UpperNorfairWest.CanEnter(items, true) && items.Super && Logic.CanUsePowerBombs(items) && CanPassMountDeath(items, Logic)) ||
                    Logic.CanAccessMaridiaPortal(items, requireRewards));

        public bool CanBeatBoss(Progression items)
            => CanEnter(items, true) && CanDefeatDraygon(items, true);

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
            => CanAccessDraygon(items, requireRewards) && CanLeaveDrayonRoom(items);

        private bool CanLeaveDrayonRoom(Progression items)
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

        private bool CanReachRightSandPit(Progression items, bool requireReweards)
            => CanReachAqueduct(items, Logic, requireReweards) && items.Super && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump || items.SpaceJump);

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
                Left = new Location(this, 140, 0x8FC4AF, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: new[] { "Super Missile (yellow Maridia)", "Watering Hole - Left", },
                    vanillaItem: ItemType.Super,
                    access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, true),
                    relevanceRequirement: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, false),
                    memoryAddress: 0x11,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState);

                Right = new Location(this, 141, 0x8FC4B5, LocationType.Visible,
                    name: "Right",
                    alsoKnownAs: new[] { "Missile (yellow Maridia super missile)", "Watering Hole - right" },
                    vanillaItem: ItemType.Missile,
                    access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, true),
                    relevanceRequirement: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items) && CanReachAqueduct(items, Logic, false),
                    memoryAddress: 0x11,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Left { get; }

            public Location Right { get; }
        }

        public class LeftSandPitRoom : Room
        {
            public LeftSandPitRoom(InnerMaridia region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Left Sand Pit", metadata)
            {
                Left = new Location(this, 144, 0x8FC5DD, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: new[] { "Missile (left Maridia sand pit room)" },
                    vanillaItem: ItemType.Missile,
                    access: items => CanEnter(items, true) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    relevanceRequirement: items => CanEnter(items, false) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    memoryAddress: 0x12,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState);

                Right = new Location(this, 145, 0x8FC5E3, LocationType.Chozo,
                    name: "Right",
                    alsoKnownAs: new[] { "Reserve Tank, Maridia" },
                    vanillaItem: ItemType.ReserveTank,
                    access: items => CanEnter(items, true) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    relevanceRequirement: items => CanEnter(items, false) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    memoryAddress: 0x12,
                    memoryFlag: 0x2,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Left { get; }

            public Location Right { get; }

            public bool CanEnter(Progression items, bool requireRewards)
                => InnerMaridia.CanReachAqueduct(items, Logic, requireRewards) && items.Super && Logic.CanPassBombPassages(items);
        }
    }
}
