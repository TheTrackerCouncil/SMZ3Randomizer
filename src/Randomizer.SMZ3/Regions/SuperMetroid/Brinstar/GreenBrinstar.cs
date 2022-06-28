using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class GreenBrinstar : SMRegion
    {
        public GreenBrinstar(World world, Config config) : base(world, config)
        {
            Weight = -6;

            PowerBomb = new(this, 13, 0x8F84AC, LocationType.Chozo,
                name: "Power Bomb (green Brinstar bottom)",
                access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items)
                              && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                memoryAddress: 0x1,
                memoryFlag: 0x20);
            MissileBelowSuperMissile = new(this, 15, 0x8F8518, LocationType.Visible,
                name: "Missile (green Brinstar below super missile)",
                alsoKnownAs: "Mockball Room - Fail item",
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanPassBombPassages(items) && Logic.CanOpenRedDoors(items),
                memoryAddress: 0x1,
                memoryFlag: 0x80);
            TopSuperMissile = new(this, 16, 0x8F851E, LocationType.Visible,
                name: "Super Missile (green Brinstar top)",
                alsoKnownAs: "Mockball Room Attic",
                vanillaItem: ItemType.Super,
                access: items => Logic.CanOpenRedDoors(items) && Logic.CanMoveAtHighSpeeds(items),
                memoryAddress: 0x2,
                memoryFlag: 0x1);
            ReserveTank = new(this, 17, 0x8F852C, LocationType.Chozo,
                name: "Reserve Tank, Brinstar",
                alsoKnownAs: "Mockball Chozo",
                vanillaItem: ItemType.ReserveTank,
                access: items => Logic.CanOpenRedDoors(items) && Logic.CanMoveAtHighSpeeds(items),
                memoryAddress: 0x2,
                memoryFlag: 0x2);
            ETank = new(this, 30, 0x8F87C2, LocationType.Visible,
                name: "Energy Tank, Etecoons",
                vanillaItem: ItemType.ETank,
                access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items)
                              && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                memoryAddress: 0x3,
                memoryFlag: 0x40);
            BottomSuperMissile = new(this, 31, 0x8F87D0, LocationType.Visible,
                name: "Super Missile (green Brinstar bottom)",
                vanillaItem: ItemType.Super,
                access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items) && items.Super
                              && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                memoryAddress: 0x3,
                memoryFlag: 0x80);
            MockballHallHidden = new(this);

            MemoryRegionId = 1;
        }

        public override string Name => "Green Brinstar";

        public override string Area => "Brinstar";

        public Location PowerBomb { get; }

        public Location MissileBelowSuperMissile { get; }

        public Location TopSuperMissile { get; }

        public Location ReserveTank { get; }

        public Location ETank { get; }

        public Location BottomSuperMissile { get; }

        public MockballHallHiddenRoom MockballHallHidden { get; }

        public override bool CanEnter(Progression items)
        {
            return Logic.CanDestroyBombWalls(items) || Logic.CanParlorSpeedBoost(items);
        }

        public class MockballHallHiddenRoom : Room
        {
            public MockballHallHiddenRoom(GreenBrinstar region)
                : base(region, "Mockball Hall Hidden Room")
            {
                HiddenItem = new(this, 18, 0x8F8532, LocationType.Hidden,
                    name: "Hidden Item",
                    alsoKnownAs: new[] { "Missile (green Brinstar behind missile)", "Mockball - Back room hidden item", "Ron Popeil missiles" },
                    vanillaItem: ItemType.Missile,
                    access: items => Logic.CanMoveAtHighSpeeds(items) && Logic.CanPassBombPassages(items) && Logic.CanOpenRedDoors(items),
                    memoryAddress: 0x2,
                    memoryFlag: 0x4);

                MainItem = new(this, 19, 0x8F8538, LocationType.Visible,
                    name: "Main Item",
                    alsoKnownAs: new[] { "Missile (green Brinstar behind reserve tank)", "Mockball - Back room" },
                    vanillaItem: ItemType.Missile,
                    access: items => Logic.CanMoveAtHighSpeeds(items) && Logic.CanOpenRedDoors(items) && items.Morph,
                    memoryAddress: 0x2,
                    memoryFlag: 0x8);
            }

            public Location MainItem { get; }

            public Location HiddenItem { get; }
        }

    }

}
