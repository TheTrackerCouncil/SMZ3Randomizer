using System.Collections.Generic;
using static Randomizer.SMZ3.LegacySMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Crateria {

    class LegacyEast : LegacySMRegion {

        public override string Name => "Crateria East";
        public override string Area => "Crateria";

        public LegacyEast(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 1, 0x8F81E8, LocationType.Visible, "Missile (outside Wrecked Ship bottom)", Logic switch {
                    Normal => items => items.Morph && (
                        items.SpeedBooster || items.Grapple || items.SpaceJump ||
                        items.Gravity && (items.CanIbj() || items.HiJump) ||
                        LegacyWorld.CanEnter("Wrecked Ship", items)
                    ),
                    _ => new Requirement(items => items.Morph)
                }),
                new LegacyLocation(this, 2, 0x8F81EE, LocationType.Hidden, "Missile (outside Wrecked Ship top)", Logic switch {
                    _ => new Requirement(items => LegacyWorld.CanEnter("Wrecked Ship", items) && items.CardWreckedShipBoss && items.CanPassBombPassages())
                }),
                new LegacyLocation(this, 3, 0x8F81F4, LocationType.Visible, "Missile (outside Wrecked Ship middle)", Logic switch {
                    _ => new Requirement(items => LegacyWorld.CanEnter("Wrecked Ship", items) && items.CardWreckedShipBoss && items.CanPassBombPassages())
                }),
                new LegacyLocation(this, 4, 0x8F8248, LocationType.Visible, "Missile (Crateria moat)", Logic switch {
                    _ => new Requirement(items => true)
                }),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return Logic switch {
                Normal =>
                    /* Ship -> Moat */
                    (LegacyConfig.Keysanity ? items.CardCrateriaL2 : items.CanUsePowerBombs()) && items.Super ||
                    /* UN Portal -> Red Tower -> Moat */
                    (LegacyConfig.Keysanity ? items.CardCrateriaL2 : items.CanUsePowerBombs()) && items.CanAccessNorfairUpperPortal() &&
                        (items.Ice || items.HiJump || items.SpaceJump) ||
                    /* Through Maridia From Portal */
                    items.CanAccessMaridiaPortal(LegacyWorld) && items.Gravity && items.Super && (
                        /* Oasis -> Forgotten Highway */
                        items.CardMaridiaL2 && items.CanDestroyBombWalls() ||
                        /* Draygon -> Cactus Alley -> Forgotten Highway */
                        LegacyWorld.GetLocation("Space Jump").Available(items)
                    ) ||
                    /*Through Maridia from Pipe*/
                    items.CanUsePowerBombs() && items.Super && items.Gravity,
                _ =>
                    /* Ship -> Moat */
                    (LegacyConfig.Keysanity ? items.CardCrateriaL2 : items.CanUsePowerBombs()) && items.Super ||
                    /* UN Portal -> Red Tower -> Moat */
                    (LegacyConfig.Keysanity ? items.CardCrateriaL2 : items.CanUsePowerBombs()) && items.CanAccessNorfairUpperPortal() &&
                        (items.Ice || items.HiJump || items.CanFly() || items.CanSpringBallJump()) ||
                    /* Through Maridia From Portal */
                    items.CanAccessMaridiaPortal(LegacyWorld) && (
                        /* Oasis -> Forgotten Highway */
                        items.CardMaridiaL2 && items.Super && (
                            items.HiJump && items.CanPassBombPassages() ||
                            items.Gravity && items.CanDestroyBombWalls()
                        ) ||
                        /* Draygon -> Cactus Alley -> Forgotten Highway */
                        items.Gravity && LegacyWorld.GetLocation("Space Jump").Available(items)
                    ) ||
                    /* Through Maridia from Pipe */
                    items.CanUsePowerBombs() && items.Super && (
                        items.Gravity ||
                        items.HiJump && (items.Ice || items.CanSpringBallJump()) && items.Grapple && items.CardMaridiaL1
                    ),
            };
        }

    }

}
