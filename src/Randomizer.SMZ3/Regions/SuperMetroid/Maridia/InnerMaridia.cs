using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Maridia
{
    public class InnerMaridia : SMRegion, IHasReward
    {
        public InnerMaridia(World world, Config config) : base(world, config)
        {
            PseudoSparkRoom = new Location(this, 142, 0x8FC533, LocationType.Visible,
                name: "Missile (yellow Maridia false wall)",
                alsoKnownAs: new[] { "Pseudo Plasma Spark Room" },
                vanillaItem: ItemType.Missile,
                access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && CanPassPipeCrossroads(items),
                memoryAddress: 0x11,
                memoryFlag: 0x40);
            PlasmaBeamRoom = new Location(this, 143, 0x8FC559, LocationType.Chozo,
                name: "Plasma Beam",
                access: items => items.Draygon
                              && (items.ScrewAttack || items.Plasma)
                              && ((items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Medium)) || Logic.CanFly(items)),
                memoryAddress: 0x11,
                memoryFlag: 0x80);
            RightSandPitLeft = new Location(this, 146, 0x8FC5EB, LocationType.Visible,
                name: "Missile (right Maridia sand pit room)",
                alsoKnownAs: new[] { "Right Sand Pit - Left item" },
                vanillaItem: ItemType.Missile,
                access: items => CanReachAqueduct(items, Logic) && items.Super && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump || items.SpaceJump),
                memoryAddress: 0x12,
                memoryFlag: 0x4);
            RightSandPitRight = new Location(this, 147, 0x8FC5F1, LocationType.Visible,
                name: "Power Bomb (right Maridia sand pit room)",
                alsoKnownAs: new[] { "Right Sand Pit - Right item" },
                vanillaItem: ItemType.PowerBomb,
                access: items => CanReachAqueduct(items, Logic) && items.Super && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump || items.SpaceJump),
                memoryAddress: 0x12,
                memoryFlag: 0x8);
            AqueductLeft = new Location(this, 148, 0x8FC603, LocationType.Visible,
                name: "Missile (pink Maridia)",
                alsoKnownAs: new[] { "Aqueduct - Left item" },
                vanillaItem: ItemType.Missile,
                access: items => CanReachAqueduct(items, Logic) && items.SpeedBooster,
                memoryAddress: 0x12,
                memoryFlag: 0x10);
            AqueductRight = new Location(this, 149, 0x8FC609, LocationType.Visible,
                name: "Super Missile (pink Maridia)",
                alsoKnownAs: new[] { "Aqueduct - Right item" },
                vanillaItem: ItemType.Super,
                access: items => CanReachAqueduct(items, Logic) && items.SpeedBooster,
                memoryAddress: 0x12,
                memoryFlag: 0x20);
            ShaktoolItem = new Location(this, 150, 0x8FC6E5, LocationType.Chozo,
                name: "Spring Ball",
                alsoKnownAs: new[] { "Shaktool's item" },
                vanillaItem: ItemType.SpringBall,
                access: items => items.Super && Logic.CanUsePowerBombs(items)
                              && (items.Grapple || Config.ShaktoolWithoutGrapple)
                              && (items.SpaceJump || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Medium)))
                              && (Logic.CanWallJump(WallJumpDifficulty.Medium) || items.SpringBall || items.SpaceJump), // Leaving again
                memoryAddress: 0x12,
                memoryFlag: 0x40);
            PreDraygonRoom = new Location(this, 151, 0x8FC74D, LocationType.Hidden,
                name: "Missile (Draygon)",
                alsoKnownAs: new[] { "Pre-Draygon Room", "The Precious Room" },
                vanillaItem: ItemType.Missile,
                access: items => items.Super
                              && (Logic.CanWallJump(WallJumpDifficulty.Hard) || items.Grapple || items.SpaceJump)
                              && (Logic.CanAccessMaridiaPortal(items) || (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items))),
                memoryAddress: 0x12,
                memoryFlag: 0x80);
            Botwoon = new Location(this, 152, 0x8FC755, LocationType.Visible,
                name: "Energy Tank, Botwoon",
                alsoKnownAs: new[] { "Sandy Path" },
                vanillaItem: ItemType.ETank,
                access: items => (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items))
                                  || (Logic.CanAccessMaridiaPortal(items) && items.CardMaridiaL2),
                memoryAddress: 0x13,
                memoryFlag: 0x1);
            DraygonTreasure = new Location(this, 154, 0x8FC7A7, LocationType.Chozo,
                name: "Space Jump",
                alsoKnownAs: new[] { "Draygon's Reliquary" },
                vanillaItem: ItemType.SpaceJump,
                access: items => CanDefeatDraygon(items),
                memoryAddress: 0x13,
                memoryFlag: 0x4);

            WateringHole = new WateringHoleRoom(this);
            LeftSandPit = new LeftSandPitRoom(this);
            MemoryRegionId = 4;
        }

        public override string Name => "Inner Maridia";

        public override string Area => "Maridia";

        public RewardType Reward { get; set; } = RewardType.Draygon;

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

        public override bool CanEnter(Progression items)
            => items.Gravity && (
                    (World.UpperNorfairWest.CanEnter(items) && items.Super && Logic.CanUsePowerBombs(items) &&
                        (Logic.CanFly(items) || items.SpeedBooster || items.Grapple)) ||
                    Logic.CanAccessMaridiaPortal(items));

        public bool CanComplete(Progression items)
            => DraygonTreasure.IsAvailable(items);

        private static bool CanReachAqueduct(Progression items, ILogic logic)
            => (items.CardMaridiaL1 && (logic.CanFly(items) || items.SpeedBooster || items.Grapple))
                         || (items.CardMaridiaL2 && logic.CanAccessMaridiaPortal(items));

        private bool CanDefeatDraygon(Progression items)
            => ((items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)) || Logic.CanAccessMaridiaPortal(items))
                && items.CardMaridiaBoss && items.Gravity && CanLeaveDrayonRoom(items);

        private bool CanLeaveDrayonRoom(Progression items)
            => (items.SpeedBooster && items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy)) || Logic.CanFly(items);

        private bool CanDefeatBotwoon(Progression items)
            => (items.SpeedBooster || Logic.CanAccessMaridiaPortal(items))
                && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.Grapple || Logic.CanFly(items));

        private bool CanPassPipeCrossroads(Progression items)
            => Logic.CanWallJump(WallJumpDifficulty.Medium) || items.HiJump || Logic.CanFly(items);

        public class WateringHoleRoom : Room
        {
            public WateringHoleRoom(InnerMaridia region)
                : base(region, "Watering Hole")
            {
                Left = new Location(this, 140, 0x8FC4AF, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: new[] { "Super Missile (yellow Maridia)", "Watering Hole - Left", },
                    vanillaItem: ItemType.Super,
                    access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items),
                    memoryAddress: 0x11,
                    memoryFlag: 0x10);

                Right = new Location(this, 141, 0x8FC4B5, LocationType.Visible,
                    name: "Right",
                    alsoKnownAs: new[] { "Missile (yellow Maridia super missile)", "Watering Hole - right" },
                    vanillaItem: ItemType.Missile,
                    access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items),
                    memoryAddress: 0x11,
                    memoryFlag: 0x20);
            }

            public Location Left { get; }

            public Location Right { get; }
        }

        public class LeftSandPitRoom : Room
        {
            public LeftSandPitRoom(InnerMaridia region)
                : base(region, "Left Sand Pit")
            {
                Left = new Location(this, 144, 0x8FC5DD, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: new[] { "Missile (left Maridia sand pit room)" },
                    vanillaItem: ItemType.Missile,
                    access: items => CanEnter(items) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    memoryAddress: 0x12,
                    memoryFlag: 0x1);

                Right = new Location(this, 145, 0x8FC5E3, LocationType.Chozo,
                    name: "Right",
                    alsoKnownAs: new[] { "Reserve Tank, Maridia" },
                    vanillaItem: ItemType.ReserveTank,
                    access: items => CanEnter(items) && Logic.CanNavigateMaridiaLeftSandPit(items),
                    memoryAddress: 0x12,
                    memoryFlag: 0x2);
            }

            public Location Left { get; }

            public Location Right { get; }

            public bool CanEnter(Progression items)
                => InnerMaridia.CanReachAqueduct(items, Logic) && items.Super && Logic.CanPassBombPassages(items);
        }
    }
}
