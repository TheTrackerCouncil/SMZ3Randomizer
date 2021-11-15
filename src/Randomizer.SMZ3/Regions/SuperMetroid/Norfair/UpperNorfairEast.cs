using System.Collections.Generic;

using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Norfair
{
    public class UpperNorfairEast : SMRegion
    {
        public UpperNorfairEast(World world, Config config) : base(world, config)
        {
            
            BubbleMountainMissileRoom = new(this, 63, 0x8F8C52, LocationType.Visible,
                name: "Missile (bubble Norfair green door)",
                alsoKnownAs: "Bubble Mountain Missile Room",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.CardNorfairL2 && (
                        items.CanFly() ||
                        items.Grapple && items.Morph && (items.SpeedBooster || items.CanPassBombPassages()) ||
                        items.HiJump || items.Ice
                    ),
                    _ => new Requirement(items => items.CardNorfairL2 && items.Super)
                });
            BubbleMountain = new(this, 64, 0x8F8C66, LocationType.Visible,
                name: "Missile (bubble Norfair)",
                alsoKnownAs: "Bubble Mountain",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => new Requirement(items => items.CardNorfairL2)
                });
            SpeedBoosterHallCeiling = new(this, 65, 0x8F8C74, LocationType.Hidden,
                name: "Missile (Speed Booster)",
                alsoKnownAs: "Speed Booster Hall - Ceiling",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.CardNorfairL2 && (
                        items.CanFly() ||
                        items.Morph && (items.SpeedBooster || items.CanPassBombPassages()) ||
                        items.HiJump || items.Ice
                    ),
                    _ => new Requirement(items => items.CardNorfairL2 && items.Super)
                });
            SpeedBoosterRoom = new(this, 66, 0x8F8C82, LocationType.Chozo,
                name: "Speed Booster",
                alsoKnownAs: "Speed Booster Room",
                vanillaItem: ItemType.SpeedBooster,
                access: Logic switch
                {
                    Normal => items => items.CardNorfairL2 && (
                        items.CanFly() ||
                        items.Morph && (items.SpeedBooster || items.CanPassBombPassages()) ||
                        items.HiJump || items.Ice
                    ),
                    _ => new Requirement(items => items.CardNorfairL2 && items.Super)
                });
            DoubleChamber = new(this, 67, 0x8F8CBC, LocationType.Visible,
                name: "Missile (Wave Beam)",
                alsoKnownAs: new[] { "Double Chamber", "Grapple Crossing" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.CardNorfairL2 && (
                        items.CanFly() ||
                        items.Morph && (items.SpeedBooster || items.CanPassBombPassages()) ||
                        items.HiJump || items.Ice
                    ) ||
                    items.SpeedBooster && items.Wave && items.Morph && items.Super,
                    _ => new Requirement(items => items.CardNorfairL2 || items.Varia)
                });
            WaveBeamRoom = new(this, 68, 0x8F8CCA, LocationType.Chozo,
                name: "Wave Beam",
                alsoKnownAs: "Wave Beam Room",
                vanillaItem: ItemType.Wave,
                access: Logic switch
                {
                    Normal => items => items.Morph && (
                        items.CardNorfairL2 && (
                            items.CanFly() ||
                            items.Morph && (items.SpeedBooster || items.CanPassBombPassages()) ||
                            items.HiJump || items.Ice
                        ) ||
                        items.SpeedBooster && items.Wave && items.Morph && items.Super
                    ),
                    _ => new Requirement(items => items.CanOpenRedDoors() && (items.CardNorfairL2 || items.Varia) &&
                        (items.Morph || items.Grapple || items.HiJump && items.Varia || items.SpaceJump))
                });
            BubbleMountainHiddenHall = new(this);
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
        public override bool CanEnter(Progression items)
        {
            return Logic switch
            {
                Normal => (
                        (items.CanDestroyBombWalls() || items.SpeedBooster) && items.Super && items.Morph ||
                        items.CanAccessNorfairUpperPortal()
                    ) && items.Varia && items.Super && (
                        /* Cathedral */
                        items.CanOpenRedDoors() && (Config.Keysanity ? items.CardNorfairL2 : items.Super) &&
                            (items.CanFly() || items.HiJump || items.SpeedBooster) ||
                        /* Frog Speedway */
                        items.SpeedBooster && (items.CardNorfairL2 || items.Wave) && items.CanUsePowerBombs()
                    ),
                _ => (
                        (items.CanDestroyBombWalls() || items.SpeedBooster) && items.Super && items.Morph ||
                        items.CanAccessNorfairUpperPortal()
                    ) &&
                    items.CanHellRun() && (
                        /* Cathedral */
                        items.CanOpenRedDoors() && (Config.Keysanity ? items.CardNorfairL2 : items.Super) && (
                            items.CanFly() || items.HiJump || items.SpeedBooster ||
                            items.CanSpringBallJump() || items.Varia && items.Ice
                        ) ||
                        /* Frog Speedway */
                        items.SpeedBooster && (items.CardNorfairL2 || items.Missile || items.Super || items.Wave) && items.CanUsePowerBombs()
                    ),
            };
        }

        public class BubbleMountainHiddenHallRoom : Room
        {
            public BubbleMountainHiddenHallRoom(UpperNorfairEast region)
                : base(region, "Bubble Mountain Hidden Hall")
            {
                MainItem = new(this, 61, 0x8F8C3E, LocationType.Chozo,
                    name: "Main Item",
                    alsoKnownAs: new[] { "Reserve Tank, Norfair" },
                    vanillaItem: ItemType.ReserveTank,
                    access: region.Logic switch
                    {
                        Normal => items => items.CardNorfairL2 && items.Morph && (
                            items.CanFly() ||
                            items.Grapple && (items.SpeedBooster || items.CanPassBombPassages()) ||
                            items.HiJump || items.Ice
                        ),
                        _ => new Requirement(items => items.CardNorfairL2 && items.Morph && items.Super)
                    });

                HiddenItem = new(this, 62, 0x8F8C44, LocationType.Hidden,
                    name: "Hidden Item",
                    alsoKnownAs: new[] { "Missile (Norfair Reserve Tank)" },
                    vanillaItem: ItemType.Missile,
                    access: region.Logic switch
                    {
                        Normal => items => items.CardNorfairL2 && items.Morph && (
                            items.CanFly() ||
                            items.Grapple && (items.SpeedBooster || items.CanPassBombPassages()) ||
                            items.HiJump || items.Ice
                        ),
                        _ => new Requirement(items => items.CardNorfairL2 && items.Morph && items.Super)
                    });
            }

            public Location MainItem { get; }

            public Location HiddenItem { get; }
        }
    }

}
