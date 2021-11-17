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
                access: Logic switch
                {
                    Normal => items => items.CardMaridiaL1 && items.CanPassBombPassages(),
                    _ => new Requirement(items => items.CardMaridiaL1 && items.CanPassBombPassages() &&
                        (items.Gravity || items.Ice || (items.HiJump && items.CanSpringBallJump())))
                });
            PlasmaBeamRoom = new(this, 143, 0x8FC559, LocationType.Chozo,
                name: "Plasma Beam",
                access: Logic switch
                {
                    Normal => items => CanDefeatDraygon(items)
                                       && (items.ScrewAttack || items.Plasma)
                                       && (items.HiJump || items.CanFly()),
                    _ => items => CanDefeatDraygon(items)
                                  && ((items.Charge && items.HasEnergyReserves(3)) || items.ScrewAttack || items.Plasma || items.SpeedBooster)
                                  && (items.HiJump || items.CanSpringBallJump() || items.CanFly() || items.SpeedBooster)
                });
            RightSandPitLeft = new(this, 146, 0x8FC5EB, LocationType.Visible,
                name: "Missile (right Maridia sand pit room)",
                alsoKnownAs: "Right Sand Pit - Left item",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => CanReachAqueduct(items) && items.Super,
                    _ => items => CanReachAqueduct(items) && items.Super && (items.HiJump || items.Gravity)
                });
            RightSandPitRight = new(this, 147, 0x8FC5F1, LocationType.Visible,
                name: "Power Bomb (right Maridia sand pit room)",
                alsoKnownAs: "Right Sand Pit - Right item",
                vanillaItem: ItemType.PowerBomb,
                access: Logic switch
                {
                    Normal => items => CanReachAqueduct(items) && items.Super,
                    _ => items => CanReachAqueduct(items) && items.Super && ((items.HiJump && items.CanSpringBallJump()) || items.Gravity)
                });
            AqueductLeft = new(this, 148, 0x8FC603, LocationType.Visible,
                name: "Missile (pink Maridia)",
                alsoKnownAs: "Aqueduct - Left item",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => CanReachAqueduct(items) && items.SpeedBooster,
                    _ => new Requirement(items => CanReachAqueduct(items) && items.Gravity)
                });
            AqueductRight = new(this, 149, 0x8FC609, LocationType.Visible,
                name: "Super Missile (pink Maridia)",
                alsoKnownAs: "Aqueduct - Right item",
                vanillaItem: ItemType.Super,
                access: Logic switch
                {
                    Normal => items => CanReachAqueduct(items) && items.SpeedBooster,
                    _ => new Requirement(items => CanReachAqueduct(items) && items.Gravity)
                });
            ShaktoolItem = new(this, 150, 0x8FC6E5, LocationType.Chozo,
                name: "Spring Ball",
                alsoKnownAs: "Shaktool's item",
                vanillaItem: ItemType.SpringBall,
                access: Logic switch
                {
                    Normal => items => items.Super && items.Grapple && items.CanUsePowerBombs() && (items.SpaceJump || items.HiJump),
                    _ => items => items.Super && items.Grapple && items.CanUsePowerBombs() && (
                        (items.Gravity && (items.CanFly() || items.HiJump)) ||
                        (items.Ice && items.HiJump && items.CanSpringBallJump() && items.SpaceJump))
                });
            PreDraygonRoom = new(this, 151, 0x8FC74D, LocationType.Hidden,
                name: "Missile (Draygon)",
                alsoKnownAs: new[] { "Pre-Draygon Room", "The Precious Room" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.CanAccessMaridiaPortal(World)
                                       || (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)),
                    _ => items => items.Gravity && (items.CanAccessMaridiaPortal(World)
                                                    || (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)))
                });
            Botwoon = new(this, 152, 0x8FC755, LocationType.Visible,
                name: "Energy Tank, Botwoon",
                alsoKnownAs: "Sandy Path",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    _ => items => (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items))
                                  || (items.CanAccessMaridiaPortal(World) && items.CardMaridiaL2)
                });
            DraygonTreasure = new(this, 154, 0x8FC7A7, LocationType.Chozo,
                name: "Space Jump",
                alsoKnownAs: "Draygon's Reliquary",
                vanillaItem: ItemType.SpaceJump,
                access: Logic switch
                {
                    _ => new Requirement(items => CanDefeatDraygon(items))
                });

            WateringHole = new(this);
            LeftSandPit = new(this);
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
        {
            return Logic switch
            {
                Normal => items.Gravity && (
                    (World.UpperNorfairWest.CanEnter(items) && items.Super && items.CanUsePowerBombs() &&
                        (items.CanFly() || items.SpeedBooster || items.Grapple)) ||
                    items.CanAccessMaridiaPortal(World)
                ),
                _ =>
                    (items.Super && World.UpperNorfairWest.CanEnter(items) && items.CanUsePowerBombs() &&
                        (items.Gravity || (items.HiJump && (items.Ice || items.CanSpringBallJump()) && items.Grapple))) ||
                    items.CanAccessMaridiaPortal(World)
            };
        }

        public bool CanComplete(Progression items)
        {
            return DraygonTreasure.IsAvailable(items);
        }

        private bool CanReachAqueduct(Progression items)
        {
            return Logic switch
            {
                Normal => (items.CardMaridiaL1 && (items.CanFly() || items.SpeedBooster || items.Grapple))
                         || (items.CardMaridiaL2 && items.CanAccessMaridiaPortal(World)),
                _ => (items.CardMaridiaL1 && (items.Gravity || (items.HiJump && (items.Ice || items.CanSpringBallJump()) && items.Grapple)))
                         || (items.CardMaridiaL2 && items.CanAccessMaridiaPortal(World))
            };
        }

        private bool CanDefeatDraygon(Progression items)
        {
            return Logic switch
            {
                Normal => (
                    (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)) ||
                    items.CanAccessMaridiaPortal(World)
                ) && items.CardMaridiaBoss && items.Gravity && ((items.SpeedBooster && items.HiJump) || items.CanFly()),
                _ => (
                    (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)) ||
                    items.CanAccessMaridiaPortal(World)
                ) && items.CardMaridiaBoss && items.Gravity
            };
        }

        private bool CanDefeatBotwoon(Progression items)
        {
            return Logic switch
            {
                Normal => items.SpeedBooster || items.CanAccessMaridiaPortal(World),
                _ => items.Ice || (items.SpeedBooster && items.Gravity) || items.CanAccessMaridiaPortal(World)
            };
        }

        public class WateringHoleRoom : Room
        {
            public WateringHoleRoom(InnerMaridia region)
                : base(region, "Watering Hole")
            {
                Left = new(this, 140, 0x8FC4AF, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: new[] { "Super Missile (yellow Maridia)", "Watering Hole - Left", },
                    vanillaItem: ItemType.Super,
                    access: region.Logic switch
                    {
                        Normal => items => items.CardMaridiaL1 && items.CanPassBombPassages(),
                        _ => new Requirement(items => items.CardMaridiaL1 && items.CanPassBombPassages() &&
                            (items.Gravity || items.Ice || (items.HiJump && items.CanSpringBallJump())))
                    });

                Right = new(this, 141, 0x8FC4B5, LocationType.Visible,
                    name: "Right",
                    alsoKnownAs: new[] { "Missile (yellow Maridia super missile)", "Watering Hole - right" },
                    vanillaItem: ItemType.Missile,
                    access: region.Logic switch
                    {
                        Normal => items => items.CardMaridiaL1 && items.CanPassBombPassages(),
                        _ => new Requirement(items => items.CardMaridiaL1 && items.CanPassBombPassages() &&
                            (items.Gravity || items.Ice || (items.HiJump && items.CanSpringBallJump())))
                    });
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
                    access: region.Logic switch
                    {
                        Normal => items => region.CanReachAqueduct(items) && items.Super && items.CanPassBombPassages(),
                        _ => items => region.CanReachAqueduct(items) && items.Super &&
                            ((items.HiJump && (items.SpaceJump || items.CanSpringBallJump())) || items.Gravity)
                    });

                Right = new(this, 145, 0x8FC5E3, LocationType.Chozo,
                    name: "Right",
                    alsoKnownAs: new[] { "Reserve Tank, Maridia" },
                    vanillaItem: ItemType.ReserveTank,
                    access: region.Logic switch
                    {
                        Normal => items => region.CanReachAqueduct(items) && items.Super && items.CanPassBombPassages(),
                        _ => items => region.CanReachAqueduct(items) && items.Super &&
                            ((items.HiJump && (items.SpaceJump || items.CanSpringBallJump())) || items.Gravity)
                    });
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
