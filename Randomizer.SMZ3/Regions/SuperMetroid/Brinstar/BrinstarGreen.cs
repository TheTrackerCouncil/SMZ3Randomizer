using System.Collections.Generic;

using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class BrinstarGreen : SMRegion
    {
        public BrinstarGreen(World world, Config config) : base(world, config)
        {
            Weight = -6;

            Locations = new List<Location> {
                PowerBomb,
                MissileBelowSuperMissile,
                TopSuperMissile,
                ReserveTank,
                MissileBehindMissile,
                MissileBehindReserveTank,
                ETank,
                BottomSuperMissile,
            };
        }

        public override string Name => "Brinstar Green";

        public override string Area => "Brinstar";

        public Location PowerBomb => new(this, 13, 0x8F84AC, LocationType.Chozo,
            name: "Power Bomb (green Brinstar bottom)",
            access: Logic switch
            {
                _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs())
            });

        public Location MissileBelowSuperMissile => new(this, 15, 0x8F8518, LocationType.Visible,
            name: "Missile (green Brinstar below super missile)",
            alsoKnownAs: "Mockball Room - Fail item",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanPassBombPassages() && items.CanOpenRedDoors())
            });

        public Location TopSuperMissile => new(this, 16, 0x8F851E, LocationType.Visible,
            name: "Super Missile (green Brinstar top)",
            alsoKnownAs: "Mockball Room Attic",
            vanillaItem: ItemType.Super,
            access: Logic switch
            {
                Normal => items => items.CanOpenRedDoors() && items.SpeedBooster,
                _ => new Requirement(items => items.CanOpenRedDoors() && (items.Morph || items.SpeedBooster))
            });

        public Location ReserveTank => new(this, 17, 0x8F852C, LocationType.Chozo,
            name: "Reserve Tank, Brinstar",
            alsoKnownAs: "Mockball Chozo",
            vanillaItem: ItemType.ReserveTank,
            access: Logic switch
            {
                Normal => items => items.CanOpenRedDoors() && items.SpeedBooster,
                _ => new Requirement(items => items.CanOpenRedDoors() && (items.Morph || items.SpeedBooster))
            });

        public Location MissileBehindMissile => new(this, 18, 0x8F8532, LocationType.Hidden,
            name: "Missile (green Brinstar behind missile)",
            alsoKnownAs: new[] { "Mockball - Back room hidden item", "Ron Popeil missiles" },
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                Normal => items => items.SpeedBooster && items.CanPassBombPassages() && items.CanOpenRedDoors(),
                _ => new Requirement(items => (items.CanPassBombPassages() || (items.Morph && items.ScrewAttack)) && items.CanOpenRedDoors())
            });

        public Location MissileBehindReserveTank => new(this, 19, 0x8F8538, LocationType.Visible,
            name: "Missile (green Brinstar behind reserve tank)",
            alsoKnownAs: "Mockball - Back room",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                Normal => items => items.SpeedBooster && items.CanOpenRedDoors() && items.Morph,
                _ => new Requirement(items => items.CanOpenRedDoors() && items.Morph)
            });

        public Location ETank => new(this, 30, 0x8F87C2, LocationType.Visible,
            name: "Energy Tank, Etecoons",
            vanillaItem: ItemType.ETank,
            access: Logic switch
            {
                _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs())
            });

        public Location BottomSuperMissile => new(this, 31, 0x8F87D0, LocationType.Visible,
            name: "Super Missile (green Brinstar bottom)",
            vanillaItem: ItemType.Super,
            access: Logic switch
            {
                _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs() && items.Super)
            });

        public override bool CanEnter(Progression items)
        {
            return items.CanDestroyBombWalls() || items.SpeedBooster;
        }

    }

}
