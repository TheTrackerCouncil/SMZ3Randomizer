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
                    Normal => items => items.CardMaridiaL1 && World.Logic.CanPassBombPassages(items),
                    _ => items => items.CardMaridiaL1 && World.Logic.CanPassBombPassages(items) &&
                        (items.Gravity || items.Ice || (items.HiJump && World.Logic.CanSpringBallJump(items)))
                });
            PlasmaBeamRoom = new(this, 143, 0x8FC559, LocationType.Chozo,
                name: "Plasma Beam",
                access: Logic switch
                {
                    Normal => items => CanDefeatDraygon(items)
                                       && (items.ScrewAttack || items.Plasma)
                                       && (items.HiJump || World.Logic.CanFly(items)),
                    _ => items => CanDefeatDraygon(items)
                                  && ((items.Charge && World.Logic.HasEnergyReserves(items, 3)) || items.ScrewAttack || items.Plasma || items.SpeedBooster)
                                  && (items.HiJump || World.Logic.CanSpringBallJump(items) || World.Logic.CanFly(items) || items.SpeedBooster)
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
                    _ => items => CanReachAqueduct(items) && items.Super && ((items.HiJump && World.Logic.CanSpringBallJump(items)) || items.Gravity)
                });
            AqueductLeft = new(this, 148, 0x8FC603, LocationType.Visible,
                name: "Missile (pink Maridia)",
                alsoKnownAs: "Aqueduct - Left item",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => CanReachAqueduct(items) && items.SpeedBooster,
                    _ => items => CanReachAqueduct(items) && items.Gravity
                });
            AqueductRight = new(this, 149, 0x8FC609, LocationType.Visible,
                name: "Super Missile (pink Maridia)",
                alsoKnownAs: "Aqueduct - Right item",
                vanillaItem: ItemType.Super,
                access: Logic switch
                {
                    Normal => items => CanReachAqueduct(items) && items.SpeedBooster,
                    _ => items => CanReachAqueduct(items) && items.Gravity
                });
            ShaktoolItem = new(this, 150, 0x8FC6E5, LocationType.Chozo,
                name: "Spring Ball",
                alsoKnownAs: "Shaktool's item",
                vanillaItem: ItemType.SpringBall,
                access: Logic switch
                {
                    Normal => items => items.Super && items.Grapple && World.Logic.CanUsePowerBombs(items) && (items.SpaceJump || items.HiJump),
                    _ => items => items.Super && items.Grapple && World.Logic.CanUsePowerBombs(items) && (
                        (items.Gravity && (World.Logic.CanFly(items) || items.HiJump)) ||
                        (items.Ice && items.HiJump && World.Logic.CanSpringBallJump(items) && items.SpaceJump))
                });
            PreDraygonRoom = new(this, 151, 0x8FC74D, LocationType.Hidden,
                name: "Missile (Draygon)",
                alsoKnownAs: new[] { "Pre-Draygon Room", "The Precious Room" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => World.Logic.CanAccessMaridiaPortal(items)
                                       || (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)),
                    _ => items => items.Gravity && (World.Logic.CanAccessMaridiaPortal(items)
                                                    || (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)))
                });
            Botwoon = new(this, 152, 0x8FC755, LocationType.Visible,
                name: "Energy Tank, Botwoon",
                alsoKnownAs: "Sandy Path",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    _ => items => (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items))
                                  || (World.Logic.CanAccessMaridiaPortal(items) && items.CardMaridiaL2)
                });
            DraygonTreasure = new(this, 154, 0x8FC7A7, LocationType.Chozo,
                name: "Space Jump",
                alsoKnownAs: "Draygon's Reliquary",
                vanillaItem: ItemType.SpaceJump,
                access: Logic switch
                {
                    _ => items => CanDefeatDraygon(items)
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
                    (World.UpperNorfairWest.CanEnter(items) && items.Super && World.Logic.CanUsePowerBombs(items) &&
                        (World.Logic.CanFly(items) || items.SpeedBooster || items.Grapple)) ||
                    World.Logic.CanAccessMaridiaPortal(items)
                ),
                _ =>
                    (items.Super && World.UpperNorfairWest.CanEnter(items) && World.Logic.CanUsePowerBombs(items) &&
                        (items.Gravity || (items.HiJump && (items.Ice || World.Logic.CanSpringBallJump(items)) && items.Grapple))) ||
                    World.Logic.CanAccessMaridiaPortal(items)
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
                Normal => (items.CardMaridiaL1 && (World.Logic.CanFly(items) || items.SpeedBooster || items.Grapple))
                         || (items.CardMaridiaL2 && World.Logic.CanAccessMaridiaPortal(items)),
                _ => (items.CardMaridiaL1 && (items.Gravity || (items.HiJump && (items.Ice || World.Logic.CanSpringBallJump(items)) && items.Grapple)))
                         || (items.CardMaridiaL2 && World.Logic.CanAccessMaridiaPortal(items))
            };
        }

        private bool CanDefeatDraygon(Progression items)
        {
            return Logic switch
            {
                Normal => (
                    (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)) ||
                    World.Logic.CanAccessMaridiaPortal(items)
                ) && items.CardMaridiaBoss && items.Gravity && ((items.SpeedBooster && items.HiJump) || World.Logic.CanFly(items)),
                _ => (
                    (items.CardMaridiaL1 && items.CardMaridiaL2 && CanDefeatBotwoon(items)) ||
                    World.Logic.CanAccessMaridiaPortal(items)
                ) && items.CardMaridiaBoss && items.Gravity
            };
        }

        private bool CanDefeatBotwoon(Progression items)
        {
            return Logic switch
            {
                Normal => items.SpeedBooster || World.Logic.CanAccessMaridiaPortal(items),
                _ => items.Ice || (items.SpeedBooster && items.Gravity) || World.Logic.CanAccessMaridiaPortal(items)
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
                        Normal => items => items.CardMaridiaL1 && World.Logic.CanPassBombPassages(items),
                        _ => items => items.CardMaridiaL1 && World.Logic.CanPassBombPassages(items) &&
                            (items.Gravity || items.Ice || (items.HiJump && World.Logic.CanSpringBallJump(items)))
                    });

                Right = new(this, 141, 0x8FC4B5, LocationType.Visible,
                    name: "Right",
                    alsoKnownAs: new[] { "Missile (yellow Maridia super missile)", "Watering Hole - right" },
                    vanillaItem: ItemType.Missile,
                    access: region.Logic switch
                    {
                        Normal => items => items.CardMaridiaL1 && World.Logic.CanPassBombPassages(items),
                        _ => items => items.CardMaridiaL1 && World.Logic.CanPassBombPassages(items) &&
                            (items.Gravity || items.Ice || (items.HiJump && World.Logic.CanSpringBallJump(items)))
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
                        Normal => items => region.CanReachAqueduct(items) && items.Super && World.Logic.CanPassBombPassages(items),
                        _ => items => region.CanReachAqueduct(items) && items.Super &&
                            ((items.HiJump && (items.SpaceJump || World.Logic.CanSpringBallJump(items))) || items.Gravity)
                    });

                Right = new(this, 145, 0x8FC5E3, LocationType.Chozo,
                    name: "Right",
                    alsoKnownAs: new[] { "Reserve Tank, Maridia" },
                    vanillaItem: ItemType.ReserveTank,
                    access: region.Logic switch
                    {
                        Normal => items => region.CanReachAqueduct(items) && items.Super && World.Logic.CanPassBombPassages(items),
                        _ => items => region.CanReachAqueduct(items) && items.Super &&
                            ((items.HiJump && (items.SpaceJump || World.Logic.CanSpringBallJump(items))) || items.Gravity)
                    });
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
