using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Brinstar
{
    public class RedBrinstar : SMRegion
    {
        public RedBrinstar(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            // TODO: some of these might expect you to have wall jump, but I'm not sure which or how

            XRayScopeRoom = new Location(this, 38, 0x8F8876, LocationType.Chozo,
                name: "X-Ray Scope",
                alsoKnownAs: new[] { "The Chozo room after the dark room with all the spikes" },
                vanillaItem: ItemType.XRay,
                access: items => Logic.CanUsePowerBombs(items) && Logic.CanOpenRedDoors(items) && (items.Grapple || items.SpaceJump),
                memoryAddress: 0x4,
                memoryFlag: 0x40,
                metadata: metadata,
                trackerState: trackerState);
            BetaPowerBombRoom = new Location(this, 39, 0x8F88CA, LocationType.Visible,
                name: "Power Bomb (red Brinstar sidehopper room)",
                alsoKnownAs: new[] { "Beta Power Bomb Room" },
                vanillaItem: ItemType.PowerBomb,
                access: items => Logic.CanUsePowerBombs(items) && items.Super,
                memoryAddress: 0x4,
                memoryFlag: 0x80,
                metadata: metadata,
                trackerState: trackerState);
            AlphaPowerBombRoom = new Location(this, 40, 0x8F890E, LocationType.Chozo,
                name: "Power Bomb (red Brinstar spike room)",
                alsoKnownAs: new[] { "Alpha Power Bomb Room" },
                vanillaItem: ItemType.PowerBomb,
                access: items => (Logic.CanUsePowerBombs(items) || items.Ice) && items.Super,
                memoryAddress: 0x5,
                memoryFlag: 0x1,
                metadata: metadata,
                trackerState: trackerState);
            AlphaPowerBombRoomWall = new Location(this, 41, 0x8F8914, LocationType.Visible,
                name: "Missile (red Brinstar spike room)",
                alsoKnownAs: new[] { "Alpha Power Bomb Room - Behind the wall" },
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanUsePowerBombs(items) && items.Super,
                memoryAddress: 0x5,
                memoryFlag: 0x2,
                metadata: metadata,
                trackerState: trackerState);
            SpazerRoom = new Location(this, 42, 0x8F896E, LocationType.Chozo,
                name: "Spazer",
                alsoKnownAs: new[] { "~ S p A z E r ~" },
                vanillaItem: ItemType.Spazer,
                access: items => Logic.CanPassBombPassages(items) && items.Super
                              && (items.HiJump || Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                memoryAddress: 0x5,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);
            MemoryRegionId = 1;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Red Brinstar");
        }

        public override string Name => "Red Brinstar";

        public override string Area => "Brinstar";

        public Location XRayScopeRoom { get; }

        public Location BetaPowerBombRoom { get; }

        public Location AlphaPowerBombRoom { get; }

        public Location AlphaPowerBombRoomWall { get; }

        public Location SpazerRoom { get; }

        public override bool CanEnter(Progression items, bool requireRewards) =>
                    ((Logic.CanDestroyBombWalls(items) || items.SpeedBooster)
                     && items.Super
                     && items.Morph) ||
                    (Logic.CanAccessNorfairUpperPortal(items) && (items.Ice || items.HiJump || items.SpaceJump));

    }

}
