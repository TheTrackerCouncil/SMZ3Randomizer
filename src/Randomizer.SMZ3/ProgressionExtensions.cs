using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3
{
    static class ProgressionExtensions {

        public static bool CanLiftLight(this Progression items) => items.Glove;
        public static bool CanLiftHeavy(this Progression items) => items.Mitt;

        public static bool CanLightTorches(this Progression items) {
            return items.Firerod || items.Lamp;
        }

        public static bool CanMeltFreezors(this Progression items) {
            return items.Firerod || items.Bombos && items.Sword;
        }

        public static bool CanExtendMagic(this Progression items, int bars = 2) {
            return (items.HalfMagic ? 2 : 1) * (items.Bottle ? 2 : 1) >= bars;
        }

        public static bool CanKillManyEnemies(this Progression items) {
            return items.Sword || items.Hammer || items.Bow || items.Firerod ||
                items.Somaria || (items.Byrna && items.CanExtendMagic());
        }

        public static bool CanAccessDeathMountainPortal(this Progression items) {
            return (items.CanDestroyBombWalls() || items.SpeedBooster) && items.Super && items.Morph;
        }

        public static bool CanAccessDarkWorldPortal(this Progression items, Config config) {
            return config.SMLogic switch {
                Normal =>
                    items.CardMaridiaL1 && items.CardMaridiaL2 && items.CanUsePowerBombs() && items.Super && items.Gravity && items.SpeedBooster,
                _ =>
                    items.CardMaridiaL1 && items.CardMaridiaL2 && items.CanUsePowerBombs() && items.Super &&
                    (items.Charge || (items.Super && items.Missile)) &&
                    (items.Gravity || (items.HiJump && items.Ice && items.Grapple)) &&
                    (items.Ice || (items.Gravity && items.SpeedBooster))
            };
        }

        public static bool CanAccessMiseryMirePortal(this Progression items, Config config) {
            return config.SMLogic switch {
                Normal =>
                    (items.CardNorfairL2 || (items.SpeedBooster && items.Wave)) && items.Varia && items.Super && items.Gravity && items.SpaceJump && items.CanUsePowerBombs(),
                _ =>
                    (items.CardNorfairL2 || items.SpeedBooster) && items.Varia && items.Super && (
                        items.CanFly() || items.HiJump || items.SpeedBooster || items.CanSpringBallJump() || items.Ice
                   ) && (items.Gravity || items.HiJump) && items.CanUsePowerBombs()
             };
        }

        public static bool CanIbj(this Progression items) {
            return false;
        }

        public static bool CanFly(this Progression items) {
            return items.SpaceJump || items.CanIbj();
        }

        public static bool CanUsePowerBombs(this Progression items) {
            return items.Morph && items.PowerBomb;
        }

        public static bool CanPassBombPassages(this Progression items) {
            return items.Morph && (items.Bombs || items.PowerBomb);
        }

        public static bool CanDestroyBombWalls(this Progression items) {
            return items.CanPassBombPassages() || items.ScrewAttack;
        }

        public static bool CanSpringBallJump(this Progression items) {
            return items.Morph && items.SpringBall;
        }

        public static bool CanHellRun(this Progression items) {
            return items.Varia || items.HasEnergyReserves(5);
        }

        public static bool HasEnergyReserves(this Progression items, int amount) {
            return (items.ETank + items.ReserveTank) >= amount;
        }

        public static bool CanOpenRedDoors(this Progression items) {
            return items.Missile || items.Super;
        }

        public static bool CanAccessNorfairUpperPortal(this Progression items) {
            return items.Flute || items.CanLiftLight() && items.Lamp;
        }

        public static bool CanAccessNorfairLowerPortal(this Progression items) {
            return items.Flute && items.CanLiftHeavy();
        }

        public static bool CanAccessMaridiaPortal(this Progression items, World world) {
            return world.Config.SMLogic switch {
                Normal =>
                    items.MoonPearl && items.Flippers &&
                    items.Gravity && items.Morph &&
                    (world.CanAquire(items, Reward.Agahnim) || items.Hammer && items.CanLiftLight() || items.CanLiftHeavy()),
                _ =>
                    items.MoonPearl && items.Flippers &&
                    (items.CanSpringBallJump() || items.HiJump || items.Gravity) && items.Morph &&
                    (world.CanAquire(items, Reward.Agahnim) || items.Hammer && items.CanLiftLight() || items.CanLiftHeavy())
            };
        }

    }

}
