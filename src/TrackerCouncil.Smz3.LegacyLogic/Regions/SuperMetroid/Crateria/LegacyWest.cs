using System.Collections.Generic;
using static Randomizer.SMZ3.LegacySMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Crateria {

    class LegacyWest : LegacySMRegion {

        public override string Name => "Crateria West";
        public override string Area => "Crateria";

        public LegacyWest(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 8, 0x8F8432, LocationType.Visible, "Energy Tank, Terminator"),
                new LegacyLocation(this, 5, 0x8F8264, LocationType.Visible, "Energy Tank, Gauntlet", Logic switch {
                    Normal => items => CanEnterAndLeaveGauntlet(items) && items.HasEnergyReserves(1),
                    _ => new Requirement(items => CanEnterAndLeaveGauntlet(items))
                }),
                new LegacyLocation(this, 9, 0x8F8464, LocationType.Visible, "Missile (Crateria gauntlet right)", Logic switch {
                    Normal => items => CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages() && items.HasEnergyReserves(2),
                    _ => new Requirement(items => CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages())
                }),
                new LegacyLocation(this, 10, 0x8F846A, LocationType.Visible, "Missile (Crateria gauntlet left)", Logic switch {
                    Normal => items => CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages() && items.HasEnergyReserves(2),
                    _ => new Requirement(items => CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages())
                })
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.CanDestroyBombWalls() || items.SpeedBooster;
        }

        private bool CanEnterAndLeaveGauntlet(LegacyProgression items) {
            return Logic switch {
                Normal =>
                    items.CardCrateriaL1 && items.Morph && (items.CanFly() || items.SpeedBooster) && (
                        items.CanIbj() ||
                        items.CanUsePowerBombs() && items.TwoPowerBombs ||
                        items.ScrewAttack
                    ),
                _ =>
                    items.CardCrateriaL1 && (
                        items.Morph && (items.Bombs || items.TwoPowerBombs) ||
                        items.ScrewAttack ||
                        items.SpeedBooster && items.CanUsePowerBombs() && items.HasEnergyReserves(2)
                    )
            };
        }

    }

}
