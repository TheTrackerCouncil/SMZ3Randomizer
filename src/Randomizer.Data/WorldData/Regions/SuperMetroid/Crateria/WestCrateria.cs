using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Options;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Crateria
{
    public class WestCrateria : SMRegion
    {
        public WestCrateria(World world, Config config) : base(world, config)
        {
            Terminator = new Location(this, 8, 0x8F8432, LocationType.Visible,
                name: "Energy Tank, Terminator",
                alsoKnownAs: new[] { "Terminator Room", "Fungal Slope" },
                vanillaItem: ItemType.ETank,
                memoryAddress: 0x1,
                memoryFlag: 0x1);
            Gauntlet = new Location(this, 5, 0x8F8264, LocationType.Visible,
                name: "Energy Tank, Gauntlet",
                alsoKnownAs: new[] { "Gauntlet (Chozo)" },
                vanillaItem: ItemType.ETank,
                access: items => CanEnterAndLeaveGauntlet(items) && Logic.HasEnergyReserves(items, 1),
                memoryAddress: 0x0,
                memoryFlag: 0x20);
            GauntletShaft = new GauntletShaftRoom(this);
            MemoryRegionId = 0;
        }

        public override string Name => "West Crateria";

        public override string Area => "Crateria";

        public Location Terminator { get; }

        public Location Gauntlet { get; }

        public GauntletShaftRoom GauntletShaft { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return Logic.CanDestroyBombWalls(items) || Logic.CanParlorSpeedBoost(items);
        }

        private bool CanEnterAndLeaveGauntlet(Progression items)
        {
            return items.CardCrateriaL1 && items.Morph && (Logic.CanFly(items) || items.SpeedBooster || Logic.CanWallJump(WallJumpDifficulty.Hard)) && (
                        Logic.CanIbj(items) ||
                        (Logic.CanUsePowerBombs(items) && items.TwoPowerBombs) ||
                        Logic.CanSafelyUseScrewAttack(items)
                    );
        }

        public class GauntletShaftRoom : Room
        {
            public GauntletShaftRoom(WestCrateria region)
                : base(region, "Gauntlet Shaft")
            {
                GauntletRight = new Location(this, 9, 0x8F8464, LocationType.Visible,
                name: "Right",
                alsoKnownAs: new[] { "Missile (Crateria gauntlet right)" },
                vanillaItem: ItemType.Missile,
                access: items => region.CanEnterAndLeaveGauntlet(items) && Logic.CanPassBombPassages(items) && Logic.HasEnergyReserves(items, 2),
                memoryAddress: 0x1,
                memoryFlag: 0x2);

                GauntletLeft = new Location(this, 10, 0x8F846A, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: new[] { "Missile (Crateria gauntlet left)" },
                    vanillaItem: ItemType.Missile,
                    access: items => region.CanEnterAndLeaveGauntlet(items) && Logic.CanPassBombPassages(items) && Logic.HasEnergyReserves(items, 2),
                    memoryAddress: 0x1,
                    memoryFlag: 0x4);
            }

            public Location GauntletRight { get; }

            public Location GauntletLeft { get; }
        }

    }

}
