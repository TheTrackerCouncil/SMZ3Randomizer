using System.Collections.Generic;
using static Randomizer.SMZ3.LegacySMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.NorfairLower {

    class LegacyEast : LegacySMRegion, ILegacyReward {

        public override string Name => "Norfair Lower East";
        public override string Area => "Norfair Lower";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;

        public LegacyEast(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 74, 0x8F8FCA, LocationType.Visible, "Missile (lower Norfair above fire flea room)", Logic switch {
                    _ => new Requirement(items => CanExit(items))
                }),
                new LegacyLocation(this, 75, 0x8F8FD2, LocationType.Visible, "Power Bomb (lower Norfair above fire flea room)", Logic switch {
                    Normal => new Requirement(items => CanExit(items)),
                    _ => items => CanExit(items) && items.CanPassBombPassages()
                }),
                new LegacyLocation(this, 76, 0x8F90C0, LocationType.Visible, "Power Bomb (Power Bombs of shame)", Logic switch {
                    _ => new Requirement(items => CanExit(items) && items.CanUsePowerBombs())
                }),
                new LegacyLocation(this, 77, 0x8F9100, LocationType.Visible, "Missile (lower Norfair near Wave Beam)", Logic switch {
                    Normal => new Requirement(items => CanExit(items)),
                    _ => items => CanExit(items) && items.Morph && items.CanDestroyBombWalls()
                }),
                new LegacyLocation(this, 78, 0x8F9108, LocationType.Hidden, "Energy Tank, Ridley", Logic switch {
                    _ => new Requirement(items => CanExit(items) && items.CardLowerNorfairBoss && items.CanUsePowerBombs() && items.Super)
                }),
                new LegacyLocation(this, 80, 0x8F9184, LocationType.Visible, "Energy Tank, Firefleas", Logic switch {
                    _ => new Requirement(items => CanExit(items))
                })
            };
        }

        bool CanExit(LegacyProgression items) {
            return Logic switch {
                Normal => /* Intended LN Escape */
                    items.Morph && (
                        items.CardNorfairL2 /* Bubble Mountain */ ||
                        items.Gravity && items.Wave /* Volcano Room, Blue Gate */ &&
                            (items.Grapple || items.SpaceJump) /* Spikey Acid Snakes -> Croc Escape (this shortcuts Frog Speedway) */
                    ),
                _ => /* Intended LN Escape */
                    items.Morph && (
                        items.CardNorfairL2 /* Bubble Mountain */ ||
                        (items.Missile || items.Super || items.Wave /* Blue Gate */) && (
                            items.SpeedBooster || items.CanFly() || items.Grapple ||
                            items.HiJump && (items.CanSpringBallJump() || items.Ice) /* Frog Speedway / Croc Escape */
                        )
                    ) ||
                    /* Reverse Amphitheater */
                    items.HasEnergyReserves(5),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return Logic switch {
                Normal =>
                    items.Varia && items.CardLowerNorfairL1 && (
                        LegacyWorld.CanEnter("Norfair Upper East", items) && items.CanUsePowerBombs() && items.SpaceJump && items.Gravity ||
                        items.CanAccessNorfairLowerPortal() && items.CanDestroyBombWalls() && items.Super && items.CanUsePowerBombs() && items.CanFly()
                    ),
                _ =>
                    items.Varia && items.CardLowerNorfairL1 && (
                        LegacyWorld.CanEnter("Norfair Upper East", items) && items.CanUsePowerBombs() && (items.HiJump || items.Gravity) ||
                        items.CanAccessNorfairLowerPortal() && items.CanDestroyBombWalls() && items.Super && (items.CanFly() || items.CanSpringBallJump() || items.SpeedBooster)
                    ) &&
                    (items.CanFly() || items.HiJump || items.CanSpringBallJump() || items.Ice && items.Charge) &&
                    (items.CanPassBombPassages() || items.ScrewAttack && items.SpaceJump)
            };
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Energy Tank, Ridley").Available(items);
        }

    }

}
