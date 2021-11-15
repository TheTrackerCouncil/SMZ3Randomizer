using System.Collections.Generic;

using static Randomizer.SMZ3.SMLogic;

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
                access: Logic switch
                {
                    Normal => items => CanEnterAndLeaveGauntlet(items) && items.HasEnergyReserves(1),
                    _ => new Requirement(items => CanEnterAndLeaveGauntlet(items))
                });
            GauntletShaft = new(this);
        }

        public override string Name => "West Crateria";

        public override string Area => "Crateria";

        public Location Terminator { get; }

        public Location Gauntlet { get; }

        public GauntletShaftRoom GauntletShaft { get;  }

        public override bool CanEnter(Progression items)
        {
            return items.CanDestroyBombWalls() || (Logic == Hard && items.SpeedBooster);
        }

        private bool CanEnterAndLeaveGauntlet(Progression items)
        {
            return Logic switch
            {
                Normal =>
                    items.CardCrateriaL1 && items.Morph && (items.CanFly() || items.SpeedBooster) && (
                        items.CanIbj() ||
                        items.CanUsePowerBombs() && items.TwoPowerBombs ||
                        items.ScrewAttack
                    ),
                _ =>
                    items.CardCrateriaL1 && (
                        items.Morph && (items.Bombs || items.TwoPowerBombs) ||
                        items.ScrewAttack ||
                        items.SpeedBooster && items.CanUsePowerBombs() && items.HasEnergyReserves(2)
                    )
            };
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
                access: region.Logic switch
                {
                    Normal => items => region.CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages() && items.HasEnergyReserves(2),
                    _ => new Requirement(items => region.CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages())
                });

                GauntletLeft = new(this, 10, 0x8F846A, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: "Missile (Crateria gauntlet left)",
                    vanillaItem: ItemType.Missile,
                    access: region.Logic switch
                    {
                        Normal => items => region.CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages() && items.HasEnergyReserves(2),
                        _ => new Requirement(items => region.CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages())
                    });
            }

            public Location GauntletRight { get; }

            public Location GauntletLeft { get; }
        }

    }

}
