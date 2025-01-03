using System.Collections.Generic;
using static Randomizer.SMZ3.LegacySMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar {

    class LegacyBlue : LegacySMRegion {

        public override string Name => "Brinstar Blue";
        public override string Area => "Brinstar";

        public LegacyBlue(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 26, 0x8F86EC, LocationType.Visible, "Morphing Ball"),
                new LegacyLocation(this, 27, 0x8F874C, LocationType.Visible, "Power Bomb (blue Brinstar)", Logic switch {
                    _ => new Requirement(items => items.CanUsePowerBombs())
                }),
                new LegacyLocation(this, 28, 0x8F8798, LocationType.Visible, "Missile (blue Brinstar middle)", Logic switch {
                    _ => new Requirement(items => items.CardBrinstarL1 && items.Morph)
                }),
                new LegacyLocation(this, 29, 0x8F879E, LocationType.Hidden, "Energy Tank, Brinstar Ceiling", Logic switch {
                    Normal => items => items.CardBrinstarL1 && (items.CanFly() || items.HiJump || items.SpeedBooster || items.Ice),
                    _ => new Requirement(items => items.CardBrinstarL1)
                }),
                new LegacyLocation(this, 34, 0x8F8802, LocationType.Chozo, "Missile (blue Brinstar bottom)", Logic switch {
                    _ => new Requirement(items => items.Morph)
                }),
                new LegacyLocation(this, 36, 0x8F8836, LocationType.Visible, "Missile (blue Brinstar top)", Logic switch {
                    _ => new Requirement(items => items.CardBrinstarL1 && items.CanUsePowerBombs())
                }),
                new LegacyLocation(this, 37, 0x8F883C, LocationType.Hidden, "Missile (blue Brinstar behind missile)", Logic switch {
                    _ => new Requirement(items => items.CardBrinstarL1 && items.CanUsePowerBombs())
                }),
            };
        }

    }

}
