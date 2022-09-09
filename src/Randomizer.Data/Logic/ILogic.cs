using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData;
using Randomizer.Shared;

namespace Randomizer.Data.Logic
{
    public interface ILogic
    {
        public bool CanLiftLight(Progression items);

        public bool CanLiftHeavy(Progression items);

        public bool CanLightTorches(Progression items);

        public bool CanMeltFreezors(Progression items);

        public bool CanExtendMagic(Progression items, int bars = 2);

        public bool CanKillManyEnemies(Progression items);

        public bool CanAccessDeathMountainPortal(Progression items);

        public bool CanAccessDarkWorldPortal(Progression items);

        public bool CanAccessMiseryMirePortal(Progression items);

        public bool CanIbj(Progression items);

        public bool CanFly(Progression items);

        public bool CanUsePowerBombs(Progression items);

        public bool CanPassBombPassages(Progression items);

        public bool CanDestroyBombWalls(Progression items);

        public bool CanSpringBallJump(Progression items);

        public bool CanHellRun(Progression items);

        public bool HasEnergyReserves(Progression items, int amount);

        public bool CanOpenRedDoors(Progression items);

        public bool CanAccessNorfairUpperPortal(Progression items);

        public bool CanAccessNorfairLowerPortal(Progression items);

        public bool CanAccessMaridiaPortal(Progression items, bool requireRewards);

        public bool CanSafelyUseScrewAttack(Progression items);

        public bool CanPassFireRodDarkRooms(Progression items);

        public bool CanParlorSpeedBoost(Progression items);

        public bool CanMoveAtHighSpeeds(Progression items);

        public bool CanPassSwordOnlyDarkRooms(Progression items);

        public bool CanHyruleSouthFakeFlippers(Progression items, bool fairyChests);

        public bool CanNavigateMaridiaLeftSandPit(Progression items);
        
        public bool CanWallJump(WallJumpDifficulty difficulty);

        public bool CheckAgahnim(Progression items, World world, bool requireRewards);
    }
}
