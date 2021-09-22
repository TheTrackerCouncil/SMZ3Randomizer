using System.Collections.Generic;

using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class BrinstarBlue : SMRegion
    {
        public BrinstarBlue(World world, Config config) : base(world, config)
        {
            Locations = new List<Location> {
                MorphBall,
                PowerBomb,
                MiddleMissile,
                Ceiling,
                BottomMissile,
                TopMissile,
                MissileBehindMissile,
            };
        }

        public override string Name => "Brinstar Blue";

        public override string Area => "Brinstar";

        public Location MorphBall => new(this, 26, 0x8F86EC, LocationType.Visible,
            name: "Morphing Ball",
            alsoKnownAs: "Morph Ball (Corridor No. 1)",
            vanillaItem: ItemType.Morph);

        public Location PowerBomb => new(this, 27, 0x8F874C, LocationType.Visible,
            name: "Power Bomb (blue Brinstar)",
            alsoKnownAs: "Power Bomb wall (Corridor No. 1)",
            vanillaItem: ItemType.PowerBomb,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanUsePowerBombs())
            });

        public Location MiddleMissile => new(this, 28, 0x8F8798, LocationType.Visible,
            name: "Missile (blue Brinstar middle)",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.CardBrinstarL1 && items.Morph)
            });

        public Location Ceiling => new(this, 29, 0x8F879E, LocationType.Hidden,
            name: "Energy Tank, Brinstar Ceiling",
            vanillaItem: ItemType.ETank,
            access: Logic switch
            {
                Normal => items => items.CardBrinstarL1 && (items.CanFly() || items.HiJump || items.SpeedBooster || items.Ice),
                _ => new Requirement(items => items.CardBrinstarL1)
            });

        public Location BottomMissile => new(this, 34, 0x8F8802, LocationType.Chozo,
            name: "Missile (blue Brinstar bottom)",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.Morph)
            });

        public Location TopMissile => new(this, 36, 0x8F8836, LocationType.Visible,
            name: "Missile (blue Brinstar top)",
            alsoKnownAs: "Billy Mays Room",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.CardBrinstarL1 && items.CanUsePowerBombs())
            });

        public Location MissileBehindMissile => new(this, 37, 0x8F883C, LocationType.Hidden,
            name: "Missile (blue Brinstar behind missile)",
            alsoKnownAs: "Billy Mays Room - Hidden item",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.CardBrinstarL1 && items.CanUsePowerBombs())
            });
    }
}
