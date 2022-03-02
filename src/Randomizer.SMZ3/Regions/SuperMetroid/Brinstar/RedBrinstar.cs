using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class RedBrinstar : SMRegion
    {
        public RedBrinstar(World world, Config config) : base(world, config)
        {
            XRayScopeRoom = new(this, 38, 0x8F8876, LocationType.Chozo,
                name: "X-Ray Scope",
                alsoKnownAs: "The Chozo room after the dark room with all the spikes",
                vanillaItem: ItemType.XRay,
                access: Logic switch
                {
                    Normal => items => World.Logic.CanUsePowerBombs(items) && World.Logic.CanOpenRedDoors(items) && (items.Grapple || items.SpaceJump),
                    _ => items => World.Logic.CanUsePowerBombs(items) && World.Logic.CanOpenRedDoors(items) && (
                        items.Grapple || items.SpaceJump ||
                        ((World.Logic.CanIbj(items) || (items.HiJump && items.SpeedBooster) || World.Logic.CanSpringBallJump(items)) &&
                            ((items.Varia && World.Logic.HasEnergyReserves(items, 3)) || World.Logic.HasEnergyReserves(items, 5))))
                });
            BetaPowerBombRoom = new(this, 39, 0x8F88CA, LocationType.Visible,
                name: "Power Bomb (red Brinstar sidehopper room)",
                alsoKnownAs: "Beta Power Bomb Room",
                vanillaItem: ItemType.PowerBomb,
                access: Logic switch
                {
                    _ => items => World.Logic.CanUsePowerBombs(items) && items.Super
                });
            AlphaPowerBombRoom = new(this, 40, 0x8F890E, LocationType.Chozo,
                name: "Power Bomb (red Brinstar spike room)",
                alsoKnownAs: "Alpha Power Bomb Room",
                vanillaItem: ItemType.PowerBomb,
                access: Logic switch
                {
                    Normal => items => (World.Logic.CanUsePowerBombs(items) || items.Ice) && items.Super,
                    _ => items => items.Super
                });
            AlphaPowerBombRoomWall = new(this, 41, 0x8F8914, LocationType.Visible,
                name: "Missile (red Brinstar spike room)",
                alsoKnownAs: "Alpha Power Bomb Room - Behind the wall",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => items => World.Logic.CanUsePowerBombs(items) && items.Super
                });
            SpazerRoom = new(this, 42, 0x8F896E, LocationType.Chozo,
                name: "Spazer",
                alsoKnownAs: "~ S p A z E r ~",
                vanillaItem: ItemType.Spazer,
                access: Logic switch
                {
                    _ => items => World.Logic.CanPassBombPassages(items) && items.Super
                });
        }

        public override string Name => "Red Brinstar";

        public override string Area => "Brinstar";

        public Location XRayScopeRoom { get; }

        public Location BetaPowerBombRoom { get; }

        public Location AlphaPowerBombRoom { get; }

        public Location AlphaPowerBombRoomWall { get; }

        public Location SpazerRoom { get; }

        public override bool CanEnter(Progression items)
        {
            return Logic switch
            {
                Normal =>
                    ((World.Logic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph) ||
                    (World.Logic.CanAccessNorfairUpperPortal(items) && (items.Ice || items.HiJump || items.SpaceJump)),
                _ =>
                    ((World.Logic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph) ||
                    (World.Logic.CanAccessNorfairUpperPortal(items) && (items.Ice || World.Logic.CanSpringBallJump(items) || items.HiJump || World.Logic.CanFly(items)))
            };
        }

    }

}
