using System.Collections.Generic;
using Randomizer.Shared;
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
                    Normal => items => CanEnterAndLeaveGauntlet(items) && World.AdvancedLogic.HasEnergyReserves(items, 1),
                    _ => items => CanEnterAndLeaveGauntlet(items)
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
            return World.AdvancedLogic.CanDestroyBombWalls(items) || (Logic == Hard && items.SpeedBooster);
        }

        private bool CanEnterAndLeaveGauntlet(Progression items)
        {
            return Logic switch
            {
                Normal =>
                    items.CardCrateriaL1 && items.Morph && (World.AdvancedLogic.CanFly(items) || items.SpeedBooster) && (
                        World.AdvancedLogic.CanIbj(items) ||
                        (World.AdvancedLogic.CanUsePowerBombs(items) && items.TwoPowerBombs) ||
                        items.ScrewAttack
                    ),
                _ =>
                    items.CardCrateriaL1 && (
                        (items.Morph && (items.Bombs || items.TwoPowerBombs)) ||
                        items.ScrewAttack ||
                        (items.SpeedBooster && World.AdvancedLogic.CanUsePowerBombs(items) && World.AdvancedLogic.HasEnergyReserves(items, 2))
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
                    Normal => items => region.CanEnterAndLeaveGauntlet(items) && World.AdvancedLogic.CanPassBombPassages(items) && World.AdvancedLogic.HasEnergyReserves(items, 2),
                    _ => items => region.CanEnterAndLeaveGauntlet(items) && World.AdvancedLogic.CanPassBombPassages(items)
                });

                GauntletLeft = new(this, 10, 0x8F846A, LocationType.Visible,
                    name: "Left",
                    alsoKnownAs: "Missile (Crateria gauntlet left)",
                    vanillaItem: ItemType.Missile,
                    access: region.Logic switch
                    {
                        Normal => items => region.CanEnterAndLeaveGauntlet(items) && World.AdvancedLogic.CanPassBombPassages(items) && World.AdvancedLogic.HasEnergyReserves(items, 2),
                        _ => items => region.CanEnterAndLeaveGauntlet(items) && World.AdvancedLogic.CanPassBombPassages(items)
                    });
            }

            public Location GauntletRight { get; }

            public Location GauntletLeft { get; }
        }

    }

}
