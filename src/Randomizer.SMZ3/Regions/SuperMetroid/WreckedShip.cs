using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.SuperMetroid
{
    public class WreckedShip : SMRegion, IHasReward
    {
        public WreckedShip(World world, Config config) : base(world, config)
        {
            MainShaftSideRoom = new Location(this, 128, 0x8FC265, LocationType.Visible,
                name: "Missile (Wrecked Ship middle)",
                alsoKnownAs: new[] { "Main Shaft - Side room" },
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanPassBombPassages(items),
                memoryAddress: 0x10,
                memoryFlag: 0x1);
            PostChozoConcertSpeedBoosterItem = new Location(this, 129, 0x8FC2E9, LocationType.Chozo, // This isn't a Chozo item?
                name: "Reserve Tank, Wrecked Ship",
                alsoKnownAs: new[] { "Post Chozo Concert - Speed Booster Item", "Bowling Alley - Speed Booster Item" },
                vanillaItem: ItemType.ReserveTank,
                access: items => CanUnlockShip(items) && items.CardWreckedShipL1 && items.SpeedBooster && Logic.CanUsePowerBombs(items) &&
                        (items.Grapple || items.SpaceJump || (items.Varia && Logic.HasEnergyReserves(items, 2)) || Logic.HasEnergyReserves(items, 3)),
                memoryAddress: 0x10,
                memoryFlag: 0x2);
            PostChozoConcertBreakableChozo = new Location(this, 130, 0x8FC2EF, LocationType.Visible,
                name: "Missile (Gravity Suit)",
                alsoKnownAs: new[] { "Post Chozo Concert - Breakable Chozo" },
                vanillaItem: ItemType.Missile,
                access: items => CanUnlockShip(items) && items.CardWreckedShipL1 &&
                        (items.Grapple || items.SpaceJump || (items.Varia && Logic.HasEnergyReserves(items, 2)) || Logic.HasEnergyReserves(items, 3)),
                memoryAddress: 0x10,
                memoryFlag: 0x4);
            AtticAssemblyLine = new Location(this, 131, 0x8FC319, LocationType.Visible,
                name: "Missile (Wrecked Ship top)",
                alsoKnownAs: new[] { "Attic - Assembly Line" },
                vanillaItem: ItemType.Missile,
                access: items => CanUnlockShip(items),
                memoryAddress: 0x10,
                memoryFlag: 0x8);
            WreckedPool = new Location(this, 132, 0x8FC337, LocationType.Visible,
                name: "Energy Tank, Wrecked Ship",
                alsoKnownAs: new[] { "Wrecked Pool" },
                vanillaItem: ItemType.ETank,
                access: items => CanUnlockShip(items) &&
                        ((items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy)) || items.SpaceJump || items.SpeedBooster || items.Gravity),
                memoryAddress: 0x10,
                memoryFlag: 0x10);
            LeftSuperMissileChamber = new Location(this, 133, 0x8FC357, LocationType.Visible,
                name: "Super Missile (Wrecked Ship left)",
                alsoKnownAs: new[] { "Left Super Missile Chamber" },
                vanillaItem: ItemType.Super,
                access: items => CanUnlockShip(items),
                memoryAddress: 0x10,
                memoryFlag: 0x20);
            RightSuperMissileChamber = new Location(this, 134, 0x8FC365, LocationType.Visible,
                name: "Right Super, Wrecked Ship",
                alsoKnownAs: new[] { "Right Super Missile Chamber" },
                vanillaItem: ItemType.Super,
                access: items => CanUnlockShip(items),
                memoryAddress: 0x10,
                memoryFlag: 0x40);
            PostChozoConcertGravitySuitChamber = new Location(this, 135, 0x8FC36D, LocationType.Chozo,
                name: "Gravity Suit",
                alsoKnownAs: new[] { "Post Chozo Concert - Gravity Suit Chamber" },
                vanillaItem: ItemType.Gravity,
                access: items => CanUnlockShip(items) && items.CardWreckedShipL1 &&
                        (items.Grapple || items.SpaceJump || (items.Varia && Logic.HasEnergyReserves(items, 2)) || Logic.HasEnergyReserves(items, 3)),
                memoryAddress: 0x10,
                memoryFlag: 0x80);
            MemoryRegionId = 3;
        }

        public override string Name => "Wrecked Ship";

        public override string Area => "Wrecked Ship";

        public Reward Reward { get; set; } = Reward.GoldenFourBoss;

        public Location MainShaftSideRoom { get; }

        public Location PostChozoConcertSpeedBoosterItem { get; }

        public Location PostChozoConcertBreakableChozo { get; }

        public Location AtticAssemblyLine { get; }

        public Location WreckedPool { get; }

        public Location LeftSuperMissileChamber { get; }

        public Location RightSuperMissileChamber { get; }

        public Location PostChozoConcertGravitySuitChamber { get; }

        public override bool CanEnter(Progression items)
        {
            return items.Super && (
                        /* Over the Moat */
                        ((Config.Keysanity ? items.CardCrateriaL2 : Logic.CanUsePowerBombs(items)) && (
                            items.SpeedBooster || items.Grapple || items.SpaceJump ||
                            (items.Gravity && (Logic.CanIbj(items) || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy))))
                            || Logic.CanWallJump(WallJumpDifficulty.Insane)
                        )) ||
                        /* Through Maridia -> Forgotten Highway */
                        (Logic.CanUsePowerBombs(items) && CanPassReverseForgottenHighway(items)) ||
                        /* From Maridia portal -> Forgotten Highway */
                        (Logic.CanAccessMaridiaPortal(items) && CanPassReverseForgottenHighway(items) && (
                            (Logic.CanDestroyBombWalls(items) && items.CardMaridiaL2) ||
                            World.InnerMaridia.DraygonTreasure.IsAvailable(items)
                        ))
                    );
        }

        public bool CanPassReverseForgottenHighway(Progression items)
        {
            return items.Gravity && (
                Logic.CanFly(items) ||
                Logic.CanWallJump(WallJumpDifficulty.Easy) ||
                (items.HiJump && items.Ice)
            );
        }

        public bool CanComplete(Progression items)
        {
            return CanEnter(items) && CanUnlockShip(items);
        }

        private bool CanUnlockShip(Progression items)
        {
            return items.CardWreckedShipBoss && Logic.CanPassBombPassages(items);
        }
    }
}
