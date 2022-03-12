using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Maridia
{
    public class OuterMaridia : SMRegion
    {
        public OuterMaridia(World world, Config config) : base(world, config)
        {
            MainStreetCeiling = new(this, 136, 0x8FC437, LocationType.Visible,
                name: "Missile (green Maridia shinespark)",
                alsoKnownAs: new[] { "Main Street - Ceiling (Shinespark)", "Main Street Missiles" },
                vanillaItem: ItemType.Missile,
                access: items => items.SpeedBooster,
                memoryAddress: 0x11,
                memoryFlag: 0x1);
            MainStreetCrabSupers = new(this, 137, 0x8FC43D, LocationType.Visible,
                name: "Super Missile (green Maridia)",
                alsoKnownAs: "Main Street - Crab Supers",
                vanillaItem: ItemType.Super,
                access: items => Logic.CanWallJump(WallJumpDifficulty.Medium)
                              || (Logic.CanWallJump(WallJumpDifficulty.Easy) && items.Ice)
                              || items.HiJump,
                memoryAddress: 0x11,
                memoryFlag: 0x2);
            MamaTurtleRoom = new(this, 138, 0x8FC47D, LocationType.Visible,
                name: "Energy Tank, Mama turtle",
                alsoKnownAs: "Mama Turtle Room",
                vanillaItem: ItemType.ETank,
                access: items => Logic.CanOpenRedDoors(items)
                              && (Logic.CanWallJump(WallJumpDifficulty.Medium) || items.HiJump) // Reaching the room
                              && (Logic.CanFly(items) || items.SpeedBooster || items.Grapple)), // Reaching the item
                memoryAddress: 0x11,
                memoryFlag: 0x4);
            MamaTurtleWallItem = new(this, 139, 0x8FC483, LocationType.Hidden,
                name: "Missile (green Maridia tatori)",
                alsoKnownAs: "Mama Turtle Room - Wall item",
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanOpenRedDoors(items)
                              && (Logic.CanWallJump(WallJumpDifficulty.Medium) || items.HiJump) // Reaching the room
                              && (Logic.CanWallJump(WallJumpDifficulty.Easy) || items.SpeedBooster || (items.Grapple && items.HiJump))), // Reaching the item
                memoryAddress: 0x11,
                memoryFlag: 0x8);
            MemoryRegionId = 4;
        }

        public override string Name => "Outer Maridia";

        public override string Area => "Maridia";

        public Location MainStreetCeiling { get; }

        public Location MainStreetCrabSupers { get; }

        public Location MamaTurtleRoom { get; }

        public Location MamaTurtleWallItem { get; }

        public override bool CanEnter(Progression items)
        {
            return items.Gravity && (
                        (World.UpperNorfairWest.CanEnter(items) && Logic.CanUsePowerBombs(items)) ||
                        (Logic.CanAccessMaridiaPortal(items) && items.CardMaridiaL1 && items.CardMaridiaL2 && (Logic.CanPassBombPassages(items) || items.ScrewAttack))
                    );
        }

    }

}
