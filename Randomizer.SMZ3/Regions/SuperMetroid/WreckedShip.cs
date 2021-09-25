using System.Collections.Generic;

using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid
{
    public class WreckedShip : SMRegion, IHasReward
    {
        public WreckedShip(World world, Config config) : base(world, config)
        {
            Locations = new List<Location> {
                MainShaftSideRoom,
                PostChozoConcertSpeedBoosterItem,
                PostChozoConcertBreakableChozo,
                AtticAssemblyLine,
                WreckedPool,
                LeftSuperMissileChamber,
                RightSuperMissileChamber,
                PostChozoConcertGravitySuitChamber
            };
        }

        public override string Name => "Wrecked Ship";
        public override string Area => "Wrecked Ship";
        public Reward Reward { get; set; } = Reward.GoldenFourBoss;

        public Location MainShaftSideRoom => new(this, 128, 0x8FC265, LocationType.Visible,
            name: "Missile (Wrecked Ship middle)",
            alsoKnownAs: "Main Shaft - Side room",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanPassBombPassages())
            });

        public Location PostChozoConcertSpeedBoosterItem => new(this, 129, 0x8FC2E9, LocationType.Chozo, // This isn't a Chozo item?
            name: "Reserve Tank, Wrecked Ship",
            alsoKnownAs: new[] { "Post Chozo Concert - Speed Booster Item", "Bowling Alley - Speed Booster Item" },
            vanillaItem: ItemType.ReserveTank,
            access: Logic switch
            {
                Normal => items => CanUnlockShip(items) && items.CardWreckedShipL1 && items.SpeedBooster && items.CanUsePowerBombs() &&
                    (items.Grapple || items.SpaceJump || (items.Varia && items.HasEnergyReserves(2)) || items.HasEnergyReserves(3)),
                _ => new Requirement(items => CanUnlockShip(items) && items.CardWreckedShipL1 && items.CanUsePowerBombs() && items.SpeedBooster &&
                    (items.Varia || items.HasEnergyReserves(2)))
            });

        public Location PostChozoConcertBreakableChozo => new(this, 130, 0x8FC2EF, LocationType.Visible,
            name: "Missile (Gravity Suit)",
            alsoKnownAs: "Post Chozo Concert - Breakable Chozo",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                Normal => items => CanUnlockShip(items) && items.CardWreckedShipL1 &&
                    (items.Grapple || items.SpaceJump || (items.Varia && items.HasEnergyReserves(2)) || items.HasEnergyReserves(3)),
                _ => new Requirement(items => CanUnlockShip(items) && items.CardWreckedShipL1 && (items.Varia || items.HasEnergyReserves(1)))
            });

        public Location AtticAssemblyLine => new(this, 131, 0x8FC319, LocationType.Visible,
            name: "Missile (Wrecked Ship top)",
            alsoKnownAs: "Attic - Assembly Line",
            vanillaItem: ItemType.Missile,
            access: items => CanUnlockShip(items));

        public Location WreckedPool => new(this, 132, 0x8FC337, LocationType.Visible,
            name: "Energy Tank, Wrecked Ship",
            alsoKnownAs: "Wrecked Pool",
            vanillaItem: ItemType.ETank,
            access: Logic switch
            {
                Normal => items => CanUnlockShip(items) &&
                    (items.HiJump || items.SpaceJump || items.SpeedBooster || items.Gravity),
                _ => new Requirement(items => CanUnlockShip(items) && (items.Bombs || items.PowerBomb || items.CanSpringBallJump() ||
                    items.HiJump || items.SpaceJump || items.SpeedBooster || items.Gravity))
            });

        public Location LeftSuperMissileChamber => new(this, 133, 0x8FC357, LocationType.Visible,
            name: "Super Missile (Wrecked Ship left)",
            alsoKnownAs: "Left Super Missile Chamber",
            vanillaItem: ItemType.Super,
            access: items => CanUnlockShip(items));

        public Location RightSuperMissileChamber => new(this, 134, 0x8FC365, LocationType.Visible,
            name: "Right Super, Wrecked Ship",
            alsoKnownAs: "Right Super Missile Chamber",
            vanillaItem: ItemType.Super,
            access: items => CanUnlockShip(items));

        public Location PostChozoConcertGravitySuitChamber => new(this, 135, 0x8FC36D, LocationType.Chozo,
            name: "Gravity Suit",
            alsoKnownAs: "Post Chozo Concert - Gravity Suit Chamber",
            vanillaItem: ItemType.Gravity,
            access: Logic switch
            {
                Normal => items => CanUnlockShip(items) && items.CardWreckedShipL1 &&
                    (items.Grapple || items.SpaceJump || items.Varia && items.HasEnergyReserves(2) || items.HasEnergyReserves(3)),
                _ => new Requirement(items => CanUnlockShip(items) && items.CardWreckedShipL1 && (items.Varia || items.HasEnergyReserves(1)))
            });

        public override bool CanEnter(Progression items)
        {
            return Logic switch
            {
                Normal =>
                    items.Super && (
                        /* Over the Moat */
                        ((Config.Keysanity ? items.CardCrateriaL2 : items.CanUsePowerBombs()) && (
                            items.SpeedBooster || items.Grapple || items.SpaceJump ||
                            (items.Gravity && (items.CanIbj() || items.HiJump))
                        )) ||
                        /* Through Maridia -> Forgotten Highway */
                        (items.CanUsePowerBombs() && items.Gravity) ||
                        /* From Maridia portal -> Forgotten Highway */
                        (items.CanAccessMaridiaPortal(World) && items.Gravity && (
                            (items.CanDestroyBombWalls() && items.CardMaridiaL2) ||
                            World.GetLocation("Space Jump").IsAvailable(items)
                        ))
                    ),
                _ =>
                    items.Super && (
                        /* Over the Moat */
                        (Config.Keysanity ? items.CardCrateriaL2 : items.CanUsePowerBombs()) ||
                        /* Through Maridia -> Forgotten Highway */
                        (items.CanUsePowerBombs() && (
                            items.Gravity ||
                            /* Climb Mt. Everest */
                            (items.HiJump && (items.Ice || items.CanSpringBallJump()) && items.Grapple && items.CardMaridiaL1)
                        )) ||
                        /* From Maridia portal -> Forgotten Highway */
                        (items.CanAccessMaridiaPortal(World) && (
                            (items.HiJump && items.CanPassBombPassages() && items.CardMaridiaL2) ||
                            (items.Gravity && (
                                (items.CanDestroyBombWalls() && items.CardMaridiaL2) ||
                                World.GetLocation("Space Jump").IsAvailable(items)
                            ))
                        ))
                    ),
            };
        }

        public bool CanComplete(Progression items)
        {
            return CanEnter(items) && CanUnlockShip(items);
        }

        private static bool CanUnlockShip(Progression items)
        {
            return items.CardWreckedShipBoss && items.CanPassBombPassages();
        }
    }
}
