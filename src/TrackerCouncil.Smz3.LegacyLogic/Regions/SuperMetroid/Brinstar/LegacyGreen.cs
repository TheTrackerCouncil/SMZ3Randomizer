using System.Collections.Generic;
using static Randomizer.SMZ3.LegacySMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar {

    class LegacyGreen : LegacySMRegion {

        public override string Name => "Brinstar Green";
        public override string Area => "Brinstar";

        public LegacyGreen(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Weight = -6;

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 13, 0x8F84AC, LocationType.Chozo, "Power Bomb (green Brinstar bottom)", Logic switch {
                    _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs())
                }),
                new LegacyLocation(this, 15, 0x8F8518, LocationType.Visible, "Missile (green Brinstar below super missile)", Logic switch {
                    _ => new Requirement(items => items.CanPassBombPassages() && items.CanOpenRedDoors())
                }),
                new LegacyLocation(this, 16, 0x8F851E, LocationType.Visible, "Super Missile (green Brinstar top)", Logic switch {
                    Normal => items => items.CanOpenRedDoors() && items.SpeedBooster,
                    _ => new Requirement(items => items.CanOpenRedDoors() && (items.Morph || items.SpeedBooster))
                }),
                new LegacyLocation(this, 17, 0x8F852C, LocationType.Chozo, "Reserve Tank, Brinstar", Logic switch {
                    Normal => items => items.CanOpenRedDoors() && items.SpeedBooster,
                    _ => new Requirement(items => items.CanOpenRedDoors() && (items.Morph || items.SpeedBooster))
                }),
                new LegacyLocation(this, 18, 0x8F8532, LocationType.Hidden, "Missile (green Brinstar behind missile)", Logic switch {
                    Normal => items => items.SpeedBooster && items.CanPassBombPassages() && items.CanOpenRedDoors(),
                    _ => new Requirement(items => (items.CanPassBombPassages() || items.Morph && items.ScrewAttack) && items.CanOpenRedDoors())
                }),
                new LegacyLocation(this, 19, 0x8F8538, LocationType.Visible, "Missile (green Brinstar behind reserve tank)", Logic switch {
                    Normal => items => items.SpeedBooster && items.CanOpenRedDoors() && items.Morph,
                    _ => new Requirement(items => items.CanOpenRedDoors() && items.Morph)
                }),
                new LegacyLocation(this, 30, 0x8F87C2, LocationType.Visible, "Energy Tank, Etecoons", Logic switch {
                    _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs())
                }),
                new LegacyLocation(this, 31, 0x8F87D0, LocationType.Visible, "Super Missile (green Brinstar bottom)", Logic switch {
                    _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs() && items.Super)
                }),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.CanDestroyBombWalls() || items.SpeedBooster;
        }

    }

}
