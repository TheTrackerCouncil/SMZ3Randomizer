using System.Collections.Generic;
using static Randomizer.SMZ3.LegacySMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.NorfairUpper {

    class LegacyWest : LegacySMRegion {

        public override string Name => "Norfair Upper West";
        public override string Area => "Norfair Upper";

        public LegacyWest(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 49, 0x8F8AE4, LocationType.Hidden, "Missile (lava room)", Logic switch {
                    Normal => items => items.Varia && (
                            items.CanOpenRedDoors() && (items.CanFly() || items.HiJump || items.SpeedBooster) ||
                            LegacyWorld.CanEnter("Norfair Upper East", items) && items.CardNorfairL2
                        ) && items.Morph,
                    _ => new Requirement(items => items.CanHellRun() && (
                            items.CanOpenRedDoors() && (
                                items.CanFly() || items.HiJump || items.SpeedBooster ||
                                items.CanSpringBallJump() || items.Varia && items.Ice
                            ) ||
                            LegacyWorld.CanEnter("Norfair Upper East", items) && items.CardNorfairL2
                        ) && items.Morph),
                }),
                new LegacyLocation(this, 50, 0x8F8B24, LocationType.Chozo, "Ice Beam", Logic switch {
                    Normal => items => (legacyConfig.Keysanity ? items.CardNorfairL1 : items.Super) && items.CanPassBombPassages() && items.Varia && items.SpeedBooster,
                    _ => new Requirement(items => (legacyConfig.Keysanity ? items.CardNorfairL1 : items.Super) && items.Morph && (items.Varia || items.HasEnergyReserves(3)))
                }),
                new LegacyLocation(this, 51, 0x8F8B46, LocationType.Hidden, "Missile (below Ice Beam)", Logic switch {
                    Normal => items => (legacyConfig.Keysanity ? items.CardNorfairL1 : items.Super) && items.CanUsePowerBombs() && items.Varia && items.SpeedBooster,
                    _ => new Requirement(items =>
                        (legacyConfig.Keysanity ? items.CardNorfairL1 : items.Super) && items.CanUsePowerBombs() && (items.Varia || items.HasEnergyReserves(3)) ||
                        (items.Missile || items.Super || items.Wave /* Blue Gate */) && items.Varia && items.SpeedBooster &&
                            /* Access to Croc's room to get spark */
                            (legacyConfig.Keysanity ? items.CardNorfairBoss : items.Super) && items.CardNorfairL1)
                }),
                new LegacyLocation(this, 53, 0x8F8BAC, LocationType.Chozo, "Hi-Jump Boots", Logic switch {
                    _ => new Requirement(items => items.CanOpenRedDoors() && items.CanPassBombPassages())
                }),
                new LegacyLocation(this, 55, 0x8F8BE6, LocationType.Visible, "Missile (Hi-Jump Boots)", Logic switch {
                    _ => new Requirement(items => items.CanOpenRedDoors() && items.Morph)
                }),
                new LegacyLocation(this, 56, 0x8F8BEC, LocationType.Visible, "Energy Tank (Hi-Jump Boots)", Logic switch {
                    _ => new Requirement(items => items.CanOpenRedDoors())
                }),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return (items.CanDestroyBombWalls() || items.SpeedBooster) && items.Super && items.Morph ||
                items.CanAccessNorfairUpperPortal();
        }

    }

}
