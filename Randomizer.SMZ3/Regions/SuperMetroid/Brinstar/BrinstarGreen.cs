using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class BrinstarGreen : SMRegion
    {
        public BrinstarGreen(World world, Config config) : base(world, config)
        {
            Weight = -6;

            PowerBomb = new(this, 13, 0x8F84AC, LocationType.Chozo,
                name: "Power Bomb (green Brinstar bottom)",
                access: Logic switch
                {
                    _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs())
                });
            MissileBelowSuperMissile = new(this, 15, 0x8F8518, LocationType.Visible,
                name: "Missile (green Brinstar below super missile)",
                alsoKnownAs: "Mockball Room - Fail item",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => new Requirement(items => items.CanPassBombPassages() && items.CanOpenRedDoors())
                });
            TopSuperMissile = new(this, 16, 0x8F851E, LocationType.Visible,
                name: "Super Missile (green Brinstar top)",
                alsoKnownAs: "Mockball Room Attic",
                vanillaItem: ItemType.Super,
                access: Logic switch
                {
                    Normal => items => items.CanOpenRedDoors() && items.SpeedBooster,
                    _ => new Requirement(items => items.CanOpenRedDoors() && (items.Morph || items.SpeedBooster))
                });
            ReserveTank = new(this, 17, 0x8F852C, LocationType.Chozo,
                name: "Reserve Tank, Brinstar",
                alsoKnownAs: "Mockball Chozo",
                vanillaItem: ItemType.ReserveTank,
                access: Logic switch
                {
                    Normal => items => items.CanOpenRedDoors() && items.SpeedBooster,
                    _ => new Requirement(items => items.CanOpenRedDoors() && (items.Morph || items.SpeedBooster))
                });
            MissileBehindMissile = new(this, 18, 0x8F8532, LocationType.Hidden,
                name: "Missile (green Brinstar behind missile)",
                alsoKnownAs: new[] { "Mockball - Back room hidden item", "Ron Popeil missiles" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.SpeedBooster && items.CanPassBombPassages() && items.CanOpenRedDoors(),
                    _ => new Requirement(items => (items.CanPassBombPassages() || (items.Morph && items.ScrewAttack)) && items.CanOpenRedDoors())
                });
            MissileBehindReserveTank = new(this, 19, 0x8F8538, LocationType.Visible,
                name: "Missile (green Brinstar behind reserve tank)",
                alsoKnownAs: "Mockball - Back room",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.SpeedBooster && items.CanOpenRedDoors() && items.Morph,
                    _ => new Requirement(items => items.CanOpenRedDoors() && items.Morph)
                });
            ETank = new(this, 30, 0x8F87C2, LocationType.Visible,
                name: "Energy Tank, Etecoons",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs())
                });
            BottomSuperMissile = new(this, 31, 0x8F87D0, LocationType.Visible,
                name: "Super Missile (green Brinstar bottom)",
                vanillaItem: ItemType.Super,
                access: Logic switch
                {
                    _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs() && items.Super)
                });
        }

        public override string Name => "Brinstar Green";

        public override string Area => "Brinstar";

        public Location PowerBomb { get; }

        public Location MissileBelowSuperMissile { get; }

        public Location TopSuperMissile { get; }

        public Location ReserveTank { get; }

        public Location MissileBehindMissile { get; }

        public Location MissileBehindReserveTank { get; }

        public Location ETank { get; }

        public Location BottomSuperMissile { get; }

        public override bool CanEnter(Progression items)
        {
            return items.CanDestroyBombWalls() || items.SpeedBooster;
        }

    }

}
