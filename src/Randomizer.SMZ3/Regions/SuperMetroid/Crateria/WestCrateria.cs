using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Crateria
{
    public class WestCrateria : SMRegion
    {
        public WestCrateria(World world, Config config) : base(world, config)
        {
            Terminator = new(this, 8, 0x8F8432, LocationType.Visible,
                name: "Energy Tank, Terminator",
                alsoKnownAs: new[] { "Terminator Room", "Fungal Slope" },
                vanillaItem: ItemType.ETank);
            Gauntlet = new(this, 5, 0x8F8264, LocationType.Visible,
                name: "Energy Tank, Gauntlet",
                alsoKnownAs: "Gauntlet (Chozo)",
                vanillaItem: ItemType.ETank,
                access: items => CanEnterAndLeaveGauntlet(items) && Logic.HasEnergyReserves(items, 1));
            GauntletShaft = new(this);
        }

        public override string Name => "West Crateria";

        public override string Area => "Crateria";

        public Location Terminator { get; }

        public Location Gauntlet { get; }

        public GauntletShaftRoom GauntletShaft { get; }

        public override bool CanEnter(Progression items)
        {
            return Logic.CanDestroyBombWalls(items) || Logic.CanParlorSpeedBoost(items);
        }

        private bool CanEnterAndLeaveGauntlet(Progression items)
        {
            return items.CardCrateriaL1 && items.Morph && (Logic.CanFly(items) || items.SpeedBooster) && (
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
                GauntletRight = new(this, 9, 0x8F8464, LocationType.Visible,
                name: "Right",
                alsoKnownAs: "Missile (Crateria gauntlet right)",
                vanillaItem: ItemType.Missile,
                access: items => region.CanEnterAndLeaveGauntlet(items) && Logic.CanPassBombPassages(items) && Logic.HasEnergyReserves(items, 2));

                GauntletLeft = new(this, 10, 0x8F846A, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: "Missile (Crateria gauntlet left)",
                    vanillaItem: ItemType.Missile,
                    access: items => region.CanEnterAndLeaveGauntlet(items) && Logic.CanPassBombPassages(items) && Logic.HasEnergyReserves(items, 2));
            }

            public Location GauntletRight { get; }

            public Location GauntletLeft { get; }
        }

    }

}
