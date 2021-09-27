using System.Collections.Generic;

using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Crateria
{
    public class CrateriaWest : SMRegion
    {
        public CrateriaWest(World world, Config config) : base(world, config)
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
            GauntletRight = new(this, 9, 0x8F8464, LocationType.Visible,
                name: "Missile (Crateria gauntlet right)",
                alsoKnownAs: "Gauntlet shaft (right)",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages() && items.HasEnergyReserves(2),
                    _ => new Requirement(items => CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages())
                });
            GauntletLeft = new(this, 10, 0x8F846A, LocationType.Visible,
                name: "Missile (Crateria gauntlet left)",
                alsoKnownAs: "Gauntlet shaft (left)",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages() && items.HasEnergyReserves(2),
                    _ => new Requirement(items => CanEnterAndLeaveGauntlet(items) && items.CanPassBombPassages())
                });
        }

        public override string Name => "Crateria West";

        public override string Area => "Crateria";

        public Location Terminator { get; }

        public Location Gauntlet { get; }

        public Location GauntletRight { get; }

        public Location GauntletLeft { get; }

        public override bool CanEnter(Progression items)
        {
            return items.CanDestroyBombWalls() || items.SpeedBooster;
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

    }

}
