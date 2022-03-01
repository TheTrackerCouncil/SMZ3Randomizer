using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid
{
    public class WreckedShip : SMRegion, IHasReward
    {
        public WreckedShip(World world, Config config) : base(world, config)
        {
            MainShaftSideRoom = new(this, 128, 0x8FC265, LocationType.Visible,
                name: "Missile (Wrecked Ship middle)",
                alsoKnownAs: "Main Shaft - Side room",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => items => World.AdvancedLogic.CanPassBombPassages(items)
                });
            PostChozoConcertSpeedBoosterItem = new(this, 129, 0x8FC2E9, LocationType.Chozo, // This isn't a Chozo item?
                name: "Reserve Tank, Wrecked Ship",
                alsoKnownAs: new[] { "Post Chozo Concert - Speed Booster Item", "Bowling Alley - Speed Booster Item" },
                vanillaItem: ItemType.ReserveTank,
                access: Logic switch
                {
                    Normal => items => CanUnlockShip(items) && items.CardWreckedShipL1 && items.SpeedBooster && World.AdvancedLogic.CanUsePowerBombs(items) &&
                        (items.Grapple || items.SpaceJump || (items.Varia && World.AdvancedLogic.HasEnergyReserves(items, 2)) || World.AdvancedLogic.HasEnergyReserves(items, 3)),
                    _ => items => CanUnlockShip(items) && items.CardWreckedShipL1 && World.AdvancedLogic.CanUsePowerBombs(items) && items.SpeedBooster &&
                        (items.Varia || World.AdvancedLogic.HasEnergyReserves(items, 2))
                });
            PostChozoConcertBreakableChozo = new(this, 130, 0x8FC2EF, LocationType.Visible,
                name: "Missile (Gravity Suit)",
                alsoKnownAs: "Post Chozo Concert - Breakable Chozo",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => CanUnlockShip(items) && items.CardWreckedShipL1 &&
                        (items.Grapple || items.SpaceJump || (items.Varia && World.AdvancedLogic.HasEnergyReserves(items, 2)) || World.AdvancedLogic.HasEnergyReserves(items, 3)),
                    _ => items => CanUnlockShip(items) && items.CardWreckedShipL1 && (items.Varia || World.AdvancedLogic.HasEnergyReserves(items, 1))
                });
            AtticAssemblyLine = new(this, 131, 0x8FC319, LocationType.Visible,
                name: "Missile (Wrecked Ship top)",
                alsoKnownAs: "Attic - Assembly Line",
                vanillaItem: ItemType.Missile,
                access: items => CanUnlockShip(items));
            WreckedPool = new(this, 132, 0x8FC337, LocationType.Visible,
                name: "Energy Tank, Wrecked Ship",
                alsoKnownAs: "Wrecked Pool",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    Normal => items => CanUnlockShip(items) &&
                        (items.HiJump || items.SpaceJump || items.SpeedBooster || items.Gravity),
                    _ => items => CanUnlockShip(items) && (items.Bombs || items.PowerBomb || World.AdvancedLogic.CanSpringBallJump(items) ||
                        items.HiJump || items.SpaceJump || items.SpeedBooster || items.Gravity)
                });
            LeftSuperMissileChamber = new(this, 133, 0x8FC357, LocationType.Visible,
                name: "Super Missile (Wrecked Ship left)",
                alsoKnownAs: "Left Super Missile Chamber",
                vanillaItem: ItemType.Super,
                access: items => CanUnlockShip(items));
            RightSuperMissileChamber = new(this, 134, 0x8FC365, LocationType.Visible,
                name: "Right Super, Wrecked Ship",
                alsoKnownAs: "Right Super Missile Chamber",
                vanillaItem: ItemType.Super,
                access: items => CanUnlockShip(items));
            PostChozoConcertGravitySuitChamber = new(this, 135, 0x8FC36D, LocationType.Chozo,
                name: "Gravity Suit",
                alsoKnownAs: "Post Chozo Concert - Gravity Suit Chamber",
                vanillaItem: ItemType.Gravity,
                access: Logic switch
                {
                    Normal => items => CanUnlockShip(items) && items.CardWreckedShipL1 &&
                        (items.Grapple || items.SpaceJump || (items.Varia && World.AdvancedLogic.HasEnergyReserves(items, 2)) || World.AdvancedLogic.HasEnergyReserves(items, 3)),
                    _ => items => CanUnlockShip(items) && items.CardWreckedShipL1 && (items.Varia || World.AdvancedLogic.HasEnergyReserves(items, 1))
                });
        }

        public override string Name => "Wrecked Ship";

        public override string Area => "Wrecked Ship";

        public Reward Reward { get; set; } = Reward.GoldenFourBoss;

        public Location MainShaftSideRoom { get; }

        public Location PostChozoConcertSpeedBoosterItem { get; }

        public Location PostChozoConcertBreakableChozo { get; }

        public Location AtticAssemblyLine { get; }

        public Location WreckedPool { get; }

        public Location LeftSuperMissileChamber { get; }

        public Location RightSuperMissileChamber { get; }

        public Location PostChozoConcertGravitySuitChamber { get; }

        public override bool CanEnter(Progression items)
        {
            return Logic switch
            {
                Normal =>
                    items.Super && (
                        /* Over the Moat */
                        ((Config.Keysanity ? items.CardCrateriaL2 : World.AdvancedLogic.CanUsePowerBombs(items)) && (
                            items.SpeedBooster || items.Grapple || items.SpaceJump ||
                            (items.Gravity && (World.AdvancedLogic.CanIbj(items) || items.HiJump))
                        )) ||
                        /* Through Maridia -> Forgotten Highway */
                        (World.AdvancedLogic.CanUsePowerBombs(items) && items.Gravity) ||
                        /* From Maridia portal -> Forgotten Highway */
                        (World.AdvancedLogic.CanAccessMaridiaPortal(items) && items.Gravity && (
                            (World.AdvancedLogic.CanDestroyBombWalls(items) && items.CardMaridiaL2) ||
                            World.InnerMaridia.DraygonTreasure.IsAvailable(items)
                        ))
                    ),
                _ =>
                    items.Super && (
                        /* Over the Moat */
                        (Config.Keysanity ? items.CardCrateriaL2 : World.AdvancedLogic.CanUsePowerBombs(items)) ||
                        /* Through Maridia -> Forgotten Highway */
                        (World.AdvancedLogic.CanUsePowerBombs(items) && (
                            items.Gravity ||
                            /* Climb Mt. Everest */
                            (items.HiJump && (items.Ice || World.AdvancedLogic.CanSpringBallJump(items)) && items.Grapple && items.CardMaridiaL1)
                        )) ||
                        /* From Maridia portal -> Forgotten Highway */
                        (World.AdvancedLogic.CanAccessMaridiaPortal(items) && (
                            (items.HiJump && World.AdvancedLogic.CanPassBombPassages(items) && items.CardMaridiaL2) ||
                            (items.Gravity && (
                                (World.AdvancedLogic.CanDestroyBombWalls(items) && items.CardMaridiaL2) ||
                                World.InnerMaridia.DraygonTreasure.IsAvailable(items)
                            ))
                        ))
                    ),
            };
        }

        public bool CanComplete(Progression items)
        {
            return CanEnter(items) && CanUnlockShip(items);
        }

        private bool CanUnlockShip(Progression items)
        {
            return items.CardWreckedShipBoss && World.AdvancedLogic.CanPassBombPassages(items);
        }
    }
}
