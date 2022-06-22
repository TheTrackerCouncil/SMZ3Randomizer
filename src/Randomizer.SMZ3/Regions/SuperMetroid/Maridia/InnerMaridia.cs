using System.Runtime.InteropServices;

using Npgsql.PostgresTypes;

using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Maridia
{
    public class InnerMaridia : SMRegion, IHasReward
    {
        public InnerMaridia(World world, Config config) : base(world, config)
        {
            PseudoSparkRoom = new(this, 142, 0x8FC533, LocationType.Visible,
                name: "Missile (yellow Maridia false wall)",
                alsoKnownAs: "Pseudo Plasma Spark Room",
                vanillaItem: ItemType.Missile,
                access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && CanPassPipeCrossroads(items),
                memoryAddress: 0x11,
                memoryFlag: 0x40);
            PlasmaBeamRoom = new(this, 143, 0x8FC559, LocationType.Chozo,
                name: "Plasma Beam",
                access: items => CanDefeatDraygon(items)
                                       && (items.ScrewAttack || items.Plasma)
                                       && ((items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Medium)) || Logic.CanFly(items)),
                memoryAddress: 0x11,
                memoryFlag: 0x80);
            RightSandPitLeft = new(this, 146, 0x8FC5EB, LocationType.Visible,
                name: "Missile (right Maridia sand pit room)",
                alsoKnownAs: "Right Sand Pit - Left item",
                vanillaItem: ItemType.Missile,
                access: items => CanReachAqueduct(items) && items.Super && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump),
                memoryAddress: 0x12,
                memoryFlag: 0x4);
            RightSandPitRight = new(this, 147, 0x8FC5F1, LocationType.Visible,
                name: "Power Bomb (right Maridia sand pit room)",
                alsoKnownAs: "Right Sand Pit - Right item",
                vanillaItem: ItemType.PowerBomb,
                access: items => CanReachAqueduct(items) && items.Super && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump),
                memoryAddress: 0x12,
                memoryFlag: 0x8);
            AqueductLeft = new(this, 148, 0x8FC603, LocationType.Visible,
                name: "Missile (pink Maridia)",
                alsoKnownAs: "Aqueduct - Left item",
                vanillaItem: ItemType.Missile,
                access: items => CanReachAqueduct(items) && items.SpeedBooster,
                memoryAddress: 0x12,
                memoryFlag: 0x10);
            AqueductRight = new(this, 149, 0x8FC609, LocationType.Visible,
                name: "Super Missile (pink Maridia)",
                alsoKnownAs: "Aqueduct - Right item",
                vanillaItem: ItemType.Super,
                access: items => CanReachAqueduct(items) && items.SpeedBooster,
                memoryAddress: 0x12,
                memoryFlag: 0x20);
            ShaktoolItem = new(this, 150, 0x8FC6E5, LocationType.Chozo,
                name: "Spring Ball",
                alsoKnownAs: "Shaktool's item",
                vanillaItem: ItemType.SpringBall,
                access: items => items.Super && items.Grapple && Logic.CanUsePowerBombs(items)
                              && (items.SpaceJump || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Medium))),
                memoryAddress: 0x12,
                memoryFlag: 0x40);
            PreDraygonRoom = new(this, 151, 0x8FC74D, LocationType.Hidden,
                name: "Missile (Draygon)",
                alsoKnownAs: new[] { "Pre-Draygon Room", "The Precious Room" },
                vanillaItem: ItemType.Missile,
                access: items => items.Super
                              && (Logic.CanWallJump(WallJumpDifficulty.Hard) || items.Grapple || items.SpaceJump)
                              && (Logic.CanAccessMaridiaPortal(items) || (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items))),
                memoryAddress: 0x12,
                memoryFlag: 0x80);
            Botwoon = new(this, 152, 0x8FC755, LocationType.Visible,
                name: "Energy Tank, Botwoon",
                alsoKnownAs: "Sandy Path",
                vanillaItem: ItemType.ETank,
                access: items => (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items))
                                  || (Logic.CanAccessMaridiaPortal(items) && items.CardMaridiaL2),
                memoryAddress: 0x13,
                memoryFlag: 0x1);
            DraygonTreasure = new(this, 154, 0x8FC7A7, LocationType.Chozo,
                name: "Space Jump",
                alsoKnownAs: "Draygon's Reliquary",
                vanillaItem: ItemType.SpaceJump,
                access: items => CanDefeatDraygon(items),
                memoryAddress: 0x13,
                memoryFlag: 0x4);

            WateringHole = new(this);
            LeftSandPit = new(this);
            MemoryRegionId = 4;
        }

        public override string Name => "Inner Maridia";

        public override string Area => "Maridia";

        public Reward Reward { get; set; } = Reward.GoldenFourBoss;

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

        private bool CanReachAqueduct(Progression items)
            => (items.CardMaridiaL1 && (Logic.CanFly(items) || items.SpeedBooster || items.Grapple))
                         || (items.CardMaridiaL2 && Logic.CanAccessMaridiaPortal(items));

        private bool CanDefeatDraygon(Progression items)
            => ((items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)) || Logic.CanAccessMaridiaPortal(items))
                && items.CardMaridiaBoss && items.Gravity && CanLeaveDrayonRoom(items);

        private bool CanLeaveDrayonRoom(Progression items)
            => (items.SpeedBooster && items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy)) || Logic.CanFly(items);

        private bool CanDefeatBotwoon(Progression items)
            => (items.SpeedBooster || Logic.CanAccessMaridiaPortal(items))
                && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.Grapple);

        private bool CanPassPipeCrossroads(Progression items)
            => Logic.CanWallJump(WallJumpDifficulty.Medium) || items.HiJump;

        public class WateringHoleRoom : Room
        {
            public WateringHoleRoom(InnerMaridia region)
                : base(region, "Watering Hole")
            {
                Left = new(this, 140, 0x8FC4AF, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: new[] { "Super Missile (yellow Maridia)", "Watering Hole - Left", },
                    vanillaItem: ItemType.Super,
                    access: items => items.CardMaridiaL1 && Logic.CanPassBombPassages(items) && region.CanPassPipeCrossroads(items),
                    memoryAddress: 0x11,
                    memoryFlag: 0x10);

                Right = new(this, 141, 0x8FC4B5, LocationType.Visible,
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
                Left = new(this, 144, 0x8FC5DD, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: new[] { "Missile (left Maridia sand pit room)" },
                    vanillaItem: ItemType.Missile,
                    access: items => CanEnter(items) && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanNavigateMaridiaLeftSandPit(items)), //! Double check the logic here
                    memoryAddress: 0x12,
                    memoryFlag: 0x1);

                Right = new(this, 145, 0x8FC5E3, LocationType.Chozo,
                    name: "Right",
                    alsoKnownAs: new[] { "Reserve Tank, Maridia" },
                    vanillaItem: ItemType.ReserveTank,
                    access: items => CanEnter(items) && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanNavigateMaridiaLeftSandPit(items)), //! Double check the logic here
                    memoryAddress: 0x12,
                    memoryFlag: 0x2);
            }

            public Location Left { get; }

            public Location Right { get; }

            public new InnerMaridia Region
                => (InnerMaridia)base.Region;

            public bool CanEnter(Progression items) => Region.CanReachAqueduct(items) && items.Super && Logic.CanPassBombPassages(items);
        }
    }
}
