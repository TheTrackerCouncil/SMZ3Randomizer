using System.Collections.Generic;

using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class BrinstarRed : SMRegion
    {
        public BrinstarRed(World world, Config config) : base(world, config)
        {
            Locations = new List<Location>
            {
                XRayScopeRoom,
                BetaPowerBombRoom,
                AlphaPowerBombRoom,
                AlphaPowerBombRoomWall,
                SpazerRoom,
            };
        }

        public override string Name => "Brinstar Red";

        public override string Area => "Brinstar";

        public Location XRayScopeRoom => new(this, 38, 0x8F8876, LocationType.Chozo,
            name: "X-Ray Scope",
            alsoKnownAs: "The Chozo room after the dark room with all the spikes",
            vanillaItem: ItemType.XRay,
            access: Logic switch
            {
                Normal => items => items.CanUsePowerBombs() && items.CanOpenRedDoors() && (items.Grapple || items.SpaceJump),
                _ => new Requirement(items => items.CanUsePowerBombs() && items.CanOpenRedDoors() && (
                    items.Grapple || items.SpaceJump ||
                    ((items.CanIbj() || (items.HiJump && items.SpeedBooster) || items.CanSpringBallJump()) &&
                        ((items.Varia && items.HasEnergyReserves(3)) || items.HasEnergyReserves(5)))))
            });

        public Location BetaPowerBombRoom => new(this, 39, 0x8F88CA, LocationType.Visible,
            name: "Power Bomb (red Brinstar sidehopper room)",
            alsoKnownAs: "Beta Power Bomb Room",
            vanillaItem: ItemType.PowerBomb,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanUsePowerBombs() && items.Super)
            });

        public Location AlphaPowerBombRoom => new(this, 40, 0x8F890E, LocationType.Chozo,
            name: "Power Bomb (red Brinstar spike room)",
            alsoKnownAs: "Alpha Power Bomb Room",
            vanillaItem: ItemType.PowerBomb,
            access: Logic switch
            {
                Normal => items => (items.CanUsePowerBombs() || items.Ice) && items.Super,
                _ => new Requirement(items => items.Super)
            });

        public Location AlphaPowerBombRoomWall => new(this, 41, 0x8F8914, LocationType.Visible,
            name: "Missile (red Brinstar spike room)",
            alsoKnownAs: "Alpha Power Bomb Room - Behind the wall",
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanUsePowerBombs() && items.Super)
            });

        public Location SpazerRoom => new(this, 42, 0x8F896E, LocationType.Chozo,
            name: "Spazer",
            alsoKnownAs: "~ S p A z E r ~",
            vanillaItem: ItemType.Spazer,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanPassBombPassages() && items.Super)
            });

        public override bool CanEnter(Progression items)
        {
            return Logic switch
            {
                Normal =>
                    (items.CanDestroyBombWalls() || items.SpeedBooster) && items.Super && items.Morph ||
                    items.CanAccessNorfairUpperPortal() && (items.Ice || items.HiJump || items.SpaceJump),
                _ =>
                    (items.CanDestroyBombWalls() || items.SpeedBooster) && items.Super && items.Morph ||
                    items.CanAccessNorfairUpperPortal() && (items.Ice || items.CanSpringBallJump() || items.HiJump || items.CanFly())
            };
        }

    }

}
