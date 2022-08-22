using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Norfair
{
    public class UpperNorfairEast : SMRegion
    {
        public UpperNorfairEast(World world, Config config) : base(world, config)
        {
            
            BubbleMountainMissileRoom = new Location(this, 63, 0x8F8C52, LocationType.Visible,
                name: "Missile (bubble Norfair green door)",
                alsoKnownAs: new[] { "Bubble Mountain Missile Room" },
                vanillaItem: ItemType.Missile,
                access: items => items.CardNorfairL2 && CanReachBubbleMountainLeftSide(items),
                memoryAddress: 0x7,
                memoryFlag: 0x80);
            BubbleMountain = new Location(this, 64, 0x8F8C66, LocationType.Visible,
                name: "Missile (bubble Norfair)",
                alsoKnownAs: new[] { "Bubble Mountain" },
                vanillaItem: ItemType.Missile,
                access: items => items.CardNorfairL2,
                memoryAddress: 0x8,
                memoryFlag: 0x1);
            SpeedBoosterHallCeiling = new Location(this, 65, 0x8F8C74, LocationType.Hidden,
                name: "Missile (Speed Booster)",
                alsoKnownAs: new[] { "Speed Booster Hall - Ceiling" },
                vanillaItem: ItemType.Missile,
                access: items => items.CardNorfairL2 && CanReachBubbleMountainRightSide(items)
                                 && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump),
                memoryAddress: 0x8,
                memoryFlag: 0x2);
            SpeedBoosterRoom = new Location(this, 66, 0x8F8C82, LocationType.Chozo,
                name: "Speed Booster",
                alsoKnownAs: new[] { "Speed Booster Room" },
                vanillaItem: ItemType.SpeedBooster,
                access: items => items.CardNorfairL2 && CanReachBubbleMountainRightSide(items)
                                 && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.HiJump),
                memoryAddress: 0x8,
                memoryFlag: 0x4);
            DoubleChamber = new Location(this, 67, 0x8F8CBC, LocationType.Visible,
                name: "Missile (Wave Beam)",
                alsoKnownAs: new[] { "Double Chamber", "Grapple Crossing" },
                vanillaItem: ItemType.Missile,
                access: items => (items.CardNorfairL2 && CanReachBubbleMountainRightSide(items)) ||
                    (items.SpeedBooster && items.Wave && items.Morph && items.Super),
                memoryAddress: 0x8,
                memoryFlag: 0x8);
            WaveBeamRoom = new Location(this, 68, 0x8F8CCA, LocationType.Chozo,
                name: "Wave Beam",
                alsoKnownAs: new[] { "Wave Beam Room" },
                vanillaItem: ItemType.Wave,
                access: items => items.Morph && (
                        (items.CardNorfairL2 && CanReachBubbleMountainRightSide(items)) ||
                        (items.SpeedBooster && items.Wave && items.Morph && items.Super)
                    ),
                memoryAddress: 0x8,
                memoryFlag: 0x10);
            BubbleMountainHiddenHall = new BubbleMountainHiddenHallRoom(this);
            MemoryRegionId = 2;
        }

        public override string Name => "Upper Norfair, East";

        public override string Area => "Upper Norfair";

        public Location BubbleMountainMissileRoom { get; }

        public Location BubbleMountain { get; }

        public Location SpeedBoosterHallCeiling { get; }

        public Location SpeedBoosterRoom { get; }

        public Location DoubleChamber { get; }

        public Location WaveBeamRoom { get; }

        public BubbleMountainHiddenHallRoom BubbleMountainHiddenHall { get; }

        // Todo: Super is not actually needed for Frog Speedway, but changing this will affect locations
        // Todo: Ice Beam -> Croc Speedway is not considered
        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return (
                        ((Logic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph) ||
                        Logic.CanAccessNorfairUpperPortal(items)
                    ) && items.Varia && items.Super && (
                        /* Cathedral */
                        (Logic.CanOpenRedDoors(items) && (Config.MetroidKeysanity ? items.CardNorfairL2 : items.Super) &&
                            (Logic.CanFly(items) || items.HiJump || items.SpeedBooster)) ||
                        /* Frog Speedway */
                        (items.SpeedBooster && (items.CardNorfairL2 || items.Wave) && Logic.CanUsePowerBombs(items))
                    );
        }

        private bool CanReachBubbleMountainLeftSide(Progression items)
            => Logic.CanFly(items)
            || (items.Grapple && items.Morph && (items.SpeedBooster || Logic.CanPassBombPassages(items)))
            || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy))
            || (items.Ice && items.HiJump)
            || Logic.CanWallJump(WallJumpDifficulty.Insane)
            || (Logic.CanWallJump(WallJumpDifficulty.Hard) && items.Grapple);

        private bool CanReachBubbleMountainRightSide(Progression items)
            => Logic.CanFly(items)
            || (items.Morph && (items.SpeedBooster || Logic.CanPassBombPassages(items)))
            || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy))
            || (items.Ice && items.HiJump)
            || Logic.CanWallJump(WallJumpDifficulty.Hard);

        public class BubbleMountainHiddenHallRoom : Room
        {
            public BubbleMountainHiddenHallRoom(UpperNorfairEast region)
                : base(region, "Bubble Mountain Hidden Hall")
            {
                MainItem = new Location(this, 61, 0x8F8C3E, LocationType.Chozo,
                    name: "Main Item",
                    alsoKnownAs: new[] { "Reserve Tank, Norfair" },
                    vanillaItem: ItemType.ReserveTank,
                    access: items => items.CardNorfairL2 && items.Morph && region.CanReachBubbleMountainLeftSide(items),
                    memoryAddress: 0x7,
                    memoryFlag: 0x20);

                HiddenItem = new Location(this, 62, 0x8F8C44, LocationType.Hidden,
                    name: "Hidden Item",
                    alsoKnownAs: new[] { "Missile (Norfair Reserve Tank)" },
                    vanillaItem: ItemType.Missile,
                    access: items => items.CardNorfairL2 && items.Morph && region.CanReachBubbleMountainLeftSide(items),
                    memoryAddress: 0x7,
                    memoryFlag: 0x40);
            }

            public Location MainItem { get; }

            public Location HiddenItem { get; }
        }
    }

}
