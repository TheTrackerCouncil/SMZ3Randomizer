using System.Collections.Generic;
using static Randomizer.SMZ3.LegacySMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Maridia {

    class LegacyOuter : LegacySMRegion {

        public override string Name => "Maridia Outer";
        public override string Area => "Maridia";

        public LegacyOuter(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 136, 0x8FC437, LocationType.Visible, "Missile (green Maridia shinespark)", Logic switch {
                    Normal => items => items.SpeedBooster,
                    _ => new Requirement(items => items.Gravity && items.SpeedBooster)
                }),
                new LegacyLocation(this, 137, 0x8FC43D, LocationType.Visible, "Super Missile (green Maridia)"),
                new LegacyLocation(this, 138, 0x8FC47D, LocationType.Visible, "Energy Tank, Mama turtle", Logic switch {
                    Normal => items => items.CanOpenRedDoors() && (items.CanFly() || items.SpeedBooster || items.Grapple),
                    _ => new Requirement(items => items.CanOpenRedDoors() && (
                        items.CanFly() || items.SpeedBooster || items.Grapple ||
                        items.CanSpringBallJump() && (items.Gravity || items.HiJump)
                    ))
                }),
                new LegacyLocation(this, 139, 0x8FC483, LocationType.Hidden, "Missile (green Maridia tatori)", Logic switch {
                    _ => new Requirement(items => items.CanOpenRedDoors())
                }),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return Logic switch {
                Normal => items.Gravity && (
                        LegacyWorld.CanEnter("Norfair Upper West", items) && items.CanUsePowerBombs() ||
                        items.CanAccessMaridiaPortal(LegacyWorld) && items.CardMaridiaL1 && items.CardMaridiaL2 && (items.CanPassBombPassages() || items.ScrewAttack)
                    ),
                _ =>
                    LegacyWorld.CanEnter("Norfair Upper West", items) && items.CanUsePowerBombs() &&
                        (items.Gravity || items.HiJump && (items.CanSpringBallJump() || items.Ice)) ||
                    items.CanAccessMaridiaPortal(LegacyWorld) && items.CardMaridiaL1 && items.CardMaridiaL2 && (
                        items.CanPassBombPassages() ||
                        items.Gravity && items.ScrewAttack ||
                        items.Super && (items.Gravity || items.HiJump && (items.CanSpringBallJump() || items.Ice))
                    )
            };
        }

    }

}
