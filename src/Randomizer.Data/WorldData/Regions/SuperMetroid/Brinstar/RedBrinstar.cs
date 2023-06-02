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

            XRayScope = new XRayScopeRoom(this, metadata, trackerState);
            BetaPowerBomb = new BetaPowerBombRoom(this, metadata, trackerState);
            AlphaPowerBomb = new AlphaPowerBombRoom(this, metadata, trackerState);
            Spazer = new SpazerRoom(this, metadata, trackerState);
            MemoryRegionId = 1;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Red Brinstar");
        }

        public override string Name => "Red Brinstar";

        public override string Area => "Brinstar";

        public XRayScopeRoom XRayScope { get; }

        public BetaPowerBombRoom BetaPowerBomb { get; }

        public AlphaPowerBombRoom AlphaPowerBomb { get; }

        public SpazerRoom Spazer { get; }

        public override bool CanEnter(Progression items, bool requireRewards) =>
                    ((Logic.CanDestroyBombWalls(items) || items.SpeedBooster)
                     && items.Super
                     && items.Morph) ||
                    (Logic.CanAccessNorfairUpperPortal(items) && (items.Ice || items.HiJump || items.SpaceJump));

        public class XRayScopeRoom : Room
        {
            public XRayScopeRoom(RedBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "X-Ray Scope Room", metadata)
            {
                XRayScope = new Location(this, LocationId.XRayScope, 0x8F8876, LocationType.Chozo,
                    name: "X-Ray Scope",
                    vanillaItem: ItemType.XRay,
                    access: items => Logic.CanUsePowerBombs(items) && Logic.CanOpenRedDoors(items) && (items.Grapple || items.SpaceJump),
                    memoryAddress: 0x4,
                    memoryFlag: 0x40,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location XRayScope { get; }
        }

        public class BetaPowerBombRoom : Room
        {
            public BetaPowerBombRoom(RedBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Beta Power Bomb Room", metadata)
            {
                PowerBomb = new Location(this, LocationId.BetaPowerBombRoom, 0x8F88CA, LocationType.Visible,
                    name: "Power Bomb (red Brinstar sidehopper room)",
                    vanillaItem: ItemType.PowerBomb,
                    access: items => Logic.CanUsePowerBombs(items) && items.Super,
                    memoryAddress: 0x4,
                    memoryFlag: 0x80,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location PowerBomb { get; }
        }

        public class AlphaPowerBombRoom : Room
        {
            public AlphaPowerBombRoom(RedBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Alpha Power Bomb Room", metadata)
            {
                PowerBomb = new Location(this, LocationId.AlphaPowerBombRoomRight, 0x8F890E, LocationType.Chozo,
                    name: "Power Bomb (red Brinstar spike room)",
                    vanillaItem: ItemType.PowerBomb,
                    access: items => (Logic.CanUsePowerBombs(items) || items.Ice) && items.Super,
                    memoryAddress: 0x5,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState);
                AlphaPowerBombRoomWall = new Location(this, LocationId.AlphaPowerBombRoomLeft, 0x8F8914, LocationType.Visible,
                    name: "Missile (red Brinstar spike room)",
                    vanillaItem: ItemType.Missile,
                    access: items => Logic.CanUsePowerBombs(items) && items.Super,
                    memoryAddress: 0x5,
                    memoryFlag: 0x2,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location PowerBomb { get; }

            public Location AlphaPowerBombRoomWall { get; }
        }

        public class SpazerRoom : Room
        {
            public SpazerRoom(RedBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Spazer Room", metadata)
            {
                Spazer = new Location(this, LocationId.Spazer, 0x8F896E, LocationType.Chozo,
                    name: "Spazer",
                    vanillaItem: ItemType.Spazer,
                    access: items => Logic.CanPassBombPassages(items) && items.Super
                                  && (items.HiJump || Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                    memoryAddress: 0x5,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Spazer { get; }
        }
    }
}
