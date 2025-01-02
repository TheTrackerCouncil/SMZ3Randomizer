using System.Collections.Generic;
using static Randomizer.SMZ3.LegacySMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Crateria {

    class LegacyCentral : LegacySMRegion {

        public override string Name => "Crateria Central";
        public override string Area => "Crateria";

        public LegacyCentral(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 0, 0x8F81CC, LocationType.Visible, "Power Bomb (Crateria surface)", Logic switch {
                    _ => new Requirement(items => (legacyConfig.Keysanity ? items.CardCrateriaL1 : items.CanUsePowerBombs()) && (items.SpeedBooster || items.CanFly()))
                }),
                new LegacyLocation(this, 12, 0x8F8486, LocationType.Visible, "Missile (Crateria middle)", Logic switch {
                    _ => new Requirement(items => items.CanPassBombPassages())
                }),
                new LegacyLocation(this, 6, 0x8F83EE, LocationType.Visible, "Missile (Crateria bottom)", Logic switch {
                    _ => new Requirement(items => items.CanDestroyBombWalls())
                }),
                new LegacyLocation(this, 11, 0x8F8478, LocationType.Visible, "Super Missile (Crateria)", Logic switch {
                    _ => new Requirement(items => items.CanUsePowerBombs() && items.HasEnergyReserves(2) && items.SpeedBooster)
                }),
                new LegacyLocation(this, 7, 0x8F8404, LocationType.Chozo, "Bombs", Logic switch {
                    Normal => items => (legacyConfig.Keysanity ? items.CardCrateriaBoss : items.CanOpenRedDoors()) && items.CanPassBombPassages(),
                    _ => new Requirement(items => (legacyConfig.Keysanity ? items.CardCrateriaBoss : items.CanOpenRedDoors()) && items.Morph)
                })
            };
        }

    }

}
