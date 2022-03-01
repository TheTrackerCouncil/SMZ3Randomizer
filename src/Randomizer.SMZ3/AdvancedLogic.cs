using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3
{
    public class AdvancedLogic
    {
        public AdvancedLogic(World world)
        {
            World = world;
        }

        public bool CanLiftLight(Progression items) => items.Glove;
        public bool CanLiftHeavy(Progression items) => items.Mitt;

        public bool CanLightTorches(Progression items)
        {
            return items.FireRod || items.Lamp;
        }

        public bool CanMeltFreezors(Progression items)
        {
            return items.FireRod || (items.Bombos && items.Sword);
        }

        public bool CanExtendMagic(Progression items, int bars = 2)
        {
            return (items.HalfMagic ? 2 : 1) * (items.Bottle ? 2 : 1) >= bars;
        }

        public bool CanKillManyEnemies(Progression items)
        {
            return items.Sword || items.Hammer || items.Bow || items.FireRod ||
                items.Somaria || (items.Byrna && CanExtendMagic(items));
        }

        public bool CanAccessDeathMountainPortal(Progression items)
        {
            return (CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph;
        }

        public bool CanAccessDarkWorldPortal(Progression items)
        {
            return World.Config.SMLogic switch
            {
                Normal =>
                    items.CardMaridiaL1 && items.CardMaridiaL2 && CanUsePowerBombs(items) && items.Super && items.Gravity && items.SpeedBooster,
                _ =>
                    items.CardMaridiaL1 && items.CardMaridiaL2 && CanUsePowerBombs(items) && items.Super &&
                    (items.Charge || (items.Super && items.Missile)) &&
                    (items.Gravity || (items.HiJump && items.Ice && items.Grapple)) &&
                    (items.Ice || (items.Gravity && items.SpeedBooster))
            };
        }

        public bool CanAccessMiseryMirePortal(Progression items)
        {
            return World.Config.SMLogic switch
            {
                Normal =>
                    (items.CardNorfairL2 || (items.SpeedBooster && items.Wave)) && items.Varia && items.Super && (items.Gravity && items.SpaceJump) && World.AdvancedLogic.CanUsePowerBombs(items),
                _ =>
                    (items.CardNorfairL2 || items.SpeedBooster) && items.Varia && items.Super && (
                        World.AdvancedLogic.CanFly(items) || items.HiJump || items.SpeedBooster || CanSpringBallJump(items) || items.Ice
                   ) && (items.Gravity || items.HiJump) && CanUsePowerBombs(items)
            };
        }

        public bool CanIbj(Progression items)
        {
            return false;
        }

        public bool CanFly(Progression items)
        {
            return items.SpaceJump || CanIbj(items);
        }

        public bool CanUsePowerBombs(Progression items)
        {
            return items.Morph && items.PowerBomb;
        }

        public bool CanPassBombPassages(Progression items)
        {
            return items.Morph && (items.Bombs || items.PowerBomb);
        }

        public bool CanDestroyBombWalls(Progression items)
        {
            return CanPassBombPassages(items) || CanSafelyUseScrewAttack(items);
        }

        public bool CanSpringBallJump(Progression items)
        {
            return items.Morph && items.SpringBall;
        }

        public bool CanHellRun(Progression items)
        {
            return items.Varia || HasEnergyReserves(items, 5);
        }

        public bool HasEnergyReserves(Progression items, int amount)
        {
            return (items.ETank + items.ReserveTank) >= amount;
        }

        public bool CanOpenRedDoors(Progression items)
        {
            return items.Missile || items.Super;
        }

        public bool CanAccessNorfairUpperPortal(Progression items)
        {
            return items.Flute || (CanLiftLight(items) && items.Lamp);
        }

        public bool CanAccessNorfairLowerPortal(Progression items)
        {
            return items.Flute && CanLiftHeavy(items);
        }

        public bool CanAccessMaridiaPortal(Progression items)
        {
            return World.Config.SMLogic switch
            {
                Normal =>
                    items.MoonPearl && items.Flippers &&
                    items.Gravity && items.Morph &&
                    (World.CanAquire(items, Reward.Agahnim) || items.Hammer && CanLiftLight(items) || CanLiftHeavy(items)),
                _ =>
                    items.MoonPearl && items.Flippers &&
                    (CanSpringBallJump(items) || items.HiJump || items.Gravity) && items.Morph &&
                    (World.CanAquire(items, Reward.Agahnim) || items.Hammer && CanLiftLight(items) || CanLiftHeavy(items))
            };
        }

        public bool CanSafelyUseScrewAttack(Progression items)
        {
            return items.ScrewAttack && (!World.Config.LogicConfig.PreventScrewAttackSoftLock || items.Morph);
        }

        public World World { get;  }
    }

}
