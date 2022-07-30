﻿using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3
{
    public class Logic : ILogic
    {
        public Logic(World world)
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
            return items.CardMaridiaL1 && items.CardMaridiaL2 && CanUsePowerBombs(items) && items.Super && items.Gravity && items.SpeedBooster;
        }

        public bool CanAccessMiseryMirePortal(Progression items)
        {
            return (items.CardNorfairL2 || (items.SpeedBooster && items.Wave)) && items.Varia && items.Super && (items.Gravity && items.SpaceJump) && CanUsePowerBombs(items);
        }

        public bool CanIbj(Progression items)
        {
            return items.Morph && items.Bombs && World.Config.LogicConfig.InfiniteBombJump;
        }

        public bool CanFly(Progression items)
        {
            return items.SpaceJump || CanIbj(items);
        }

        public bool CanUsePowerBombs(Progression items)
        {
            return items.Morph && ((!World.Config.LogicConfig.PreventFivePowerBombSeed && items.PowerBomb) || items.TwoPowerBombs);
        }

        public bool CanPassBombPassages(Progression items)
        {
            return items.Morph && (items.Bombs || CanUsePowerBombs(items));
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
            return items.MoonPearl && items.Flippers &&
                    items.Gravity && items.Morph &&
                    (World.CanAquire(items, ItemType.Agahnim) || items.Hammer && CanLiftLight(items) || CanLiftHeavy(items));
        }

        public bool CanSafelyUseScrewAttack(Progression items)
        {
            return items.ScrewAttack && (!World.Config.LogicConfig.PreventScrewAttackSoftLock || items.Morph);
        }

        public bool CanPassFireRodDarkRooms(Progression items)
        {
            return CanPassSwordOnlyDarkRooms(items) || (items.FireRod && World.Config.LogicConfig.FireRodDarkRooms);
        }

        public bool CanPassSwordOnlyDarkRooms(Progression items)
        {
            return items.Lamp || (items.Sword && World.Config.LogicConfig.SwordOnlyDarkRooms);
        }

        public bool CanParlorSpeedBoost(Progression items)
        {
            return items.SpeedBooster && World.Config.LogicConfig.ParlorSpeedBooster;
        }

        public bool CanMoveAtHighSpeeds(Progression items)
        {
            return items.SpeedBooster || (items.Morph && World.Config.LogicConfig.MockBall);
        }

        public bool CanHyruleSouthFakeFlippers(Progression items, bool fairyChests)
        {
            return World.Config.LogicConfig.LightWorldSouthFakeFlippers && (!fairyChests || items.MoonPearl);
        }

        public bool CanNavigateMaridiaLeftSandPit(Progression items)
        {
            if (World.Config.LogicConfig.LeftSandPitRequiresSpringBall)
            {
                return items.SpringBall && items.HiJump;
            }

            return CanWallJump(WallJumpDifficulty.Medium)
                || (CanWallJump(WallJumpDifficulty.Easy) && items.SpringBall)
                || CanFly(items);
        }

        public bool CanWallJump(WallJumpDifficulty difficulty)
        {
            return World.Config.LogicConfig.WallJumpDifficulty >= difficulty;
        }

        public World World { get; }

        public static IEnumerable<ItemType[]> GetMissingRequiredItems(Location location, Progression items)
        {
            if (location.IsAvailable(items))
            {
                return Enumerable.Empty<ItemType[]>();
            }

            // Build an item pool of all missing progression items
            var combinations = new List<ItemType[]>();
            var itemPool = Item.CreateProgressionPool(null).Select(x => x.Type).ToList();
            foreach (var ownedItem in items)
                itemPool.Remove(ownedItem);

            // Try all items by themselves
            foreach (var missingItem in itemPool)
            {
                var progression = items.Clone();
                progression.Add(missingItem);

                if (location.IsAvailable(progression))
                    combinations.Add(new[] { missingItem });
            }

            // Remove all successfull combinations from the pool to prevent redundant combinations
            foreach (var combination in combinations.SelectMany(x => x))
                itemPool.Remove(combination);

            // Try all combinations of two
            foreach (var missingItem in itemPool)
                foreach (var missingItem2 in itemPool)
                {
                    var progression = items.Clone();
                    progression.Add(missingItem);
                    progression.Add(missingItem2);
                    if (location.IsAvailable(progression))
                        combinations.Add(new[] { missingItem, missingItem2 });
                }

            // Once again, remove successfull combinations
            foreach (var combination in combinations.SelectMany(x => x))
                itemPool.Remove(combination);

            // Try all combinations of three
            foreach (var missingItem in itemPool)
                foreach (var missingItem2 in itemPool)
                    foreach (var missingItem3 in itemPool)
                    {
                        var progression = items.Clone();
                        progression.Add(missingItem);
                        progression.Add(missingItem2);
                        progression.Add(missingItem3);
                        if (location.IsAvailable(progression))
                            combinations.Add(new[] { missingItem, missingItem2, missingItem3 });
                    }

            return combinations;
        }
    }

}
