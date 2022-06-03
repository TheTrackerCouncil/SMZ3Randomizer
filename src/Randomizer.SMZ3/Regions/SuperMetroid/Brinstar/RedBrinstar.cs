using Randomizer.Shared;

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
                access: items => Logic.CanUsePowerBombs(items) && Logic.CanOpenRedDoors(items) && (items.Grapple || items.SpaceJump),
                memoryAddress: 0x4,
                memoryFlag: 0x40);
            BetaPowerBombRoom = new(this, 39, 0x8F88CA, LocationType.Visible,
                name: "Power Bomb (red Brinstar sidehopper room)",
                alsoKnownAs: "Beta Power Bomb Room",
                vanillaItem: ItemType.PowerBomb,
                access: items => Logic.CanUsePowerBombs(items) && items.Super,
                memoryAddress: 0x4,
                memoryFlag: 0x80);
            AlphaPowerBombRoom = new(this, 40, 0x8F890E, LocationType.Chozo,
                name: "Power Bomb (red Brinstar spike room)",
                alsoKnownAs: "Alpha Power Bomb Room",
                vanillaItem: ItemType.PowerBomb,
                access: items => (Logic.CanUsePowerBombs(items) || items.Ice) && items.Super,
                memoryAddress: 0x5,
                memoryFlag: 0x1);
            AlphaPowerBombRoomWall = new(this, 41, 0x8F8914, LocationType.Visible,
                name: "Missile (red Brinstar spike room)",
                alsoKnownAs: "Alpha Power Bomb Room - Behind the wall",
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanUsePowerBombs(items) && items.Super,
                memoryAddress: 0x5,
                memoryFlag: 0x2);
            SpazerRoom = new(this, 42, 0x8F896E, LocationType.Chozo,
                name: "Spazer",
                alsoKnownAs: "~ S p A z E r ~",
                vanillaItem: ItemType.Spazer,
                access: items => Logic.CanPassBombPassages(items) && items.Super,
                memoryAddress: 0x5,
                memoryFlag: 0x4);
        }

        public override string Name => "Red Brinstar";

        public override string Area => "Brinstar";

        public Location XRayScopeRoom { get; }

        public Location BetaPowerBombRoom { get; }

        public Location AlphaPowerBombRoom { get; }

        public Location AlphaPowerBombRoomWall { get; }

        public Location SpazerRoom { get; }

        public override bool CanEnter(Progression items) =>
                    ((Logic.CanDestroyBombWalls(items) || items.SpeedBooster)
                     && items.Super
                     && items.Morph) ||
                    (Logic.CanAccessNorfairUpperPortal(items) && (items.Ice || items.HiJump || items.SpaceJump));

    }

}
