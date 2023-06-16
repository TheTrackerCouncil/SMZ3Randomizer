using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using System.Collections.Generic;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Crateria
{
    public class EastCrateria : SMRegion
    {
        public EastCrateria(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            WestOcean = new WestOceanRoom(this, metadata, trackerState);
            TheMoat = new TheMoatRoom(this, metadata, trackerState);
            MemoryRegionId = 0;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("East Crateria");
        }

        public override string Name => "East Crateria";

        public override string Area => "Crateria";

        public WestOceanRoom WestOcean { get; }

        public TheMoatRoom TheMoat { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return
                    /* Ship -> Moat */
                    ((Config.MetroidKeysanity ? items.CardCrateriaL2 : Logic.CanUsePowerBombs(items)) && items.Super) ||
                    /* UN Portal -> Red Tower -> Moat */
                    ((Config.MetroidKeysanity ? items.CardCrateriaL2 : Logic.CanUsePowerBombs(items)) && Logic.CanAccessNorfairUpperPortal(items) &&
                        (items.Ice || items.HiJump || items.SpaceJump)) ||
                    /*Through Maridia From Portal*/
                    (Logic.CanAccessMaridiaPortal(items, requireRewards) && items.Gravity && items.Super && (
                        /* Oasis -> Forgotten Highway */
                        (items.CardMaridiaL2 && Logic.CanDestroyBombWalls(items)) ||
                        /* Draygon -> Cactus Alley -> Forgotten Highway */
                        World.FindLocation(LocationId.InnerMaridiaSpaceJump).IsAvailable(items))) ||
                    /*Through Maridia from Pipe*/
                    (Logic.CanUsePowerBombs(items) && items.Super && items.Gravity);
        }

        public class WestOceanRoom : Room
        {
            public WestOceanRoom(EastCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "West Ocean", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaWestOceanFloodedCavern, 0x8F81E8, LocationType.Visible,
                        name: "Missile (outside Wrecked Ship bottom)",
                        vanillaItem: ItemType.Missile,
                        access: items => CanAccessFloodedCavernUnderWater(items, true),
                        relevanceRequirement: items => CanAccessFloodedCavernUnderWater(items, false),
                        memoryAddress: 0x0,
                        memoryFlag: 0x2,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.CrateriaWestOceanSky, 0x8F81EE, LocationType.Hidden,
                        name: "Missile (outside Wrecked Ship top)",
                        vanillaItem: ItemType.Missile,
                        access: items => CanAccessSkyItem(items, true),
                        relevanceRequirement: items => CanAccessSkyItem(items, false),
                        memoryAddress: 0x0,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.CrateriaWestOceanMorphBallMaze, 0x8F81F4, LocationType.Visible,
                        name: "Missile (outside Wrecked Ship middle)",
                        vanillaItem: ItemType.Missile,
                        access: items => CanPassThroughWreckedShip(items, true),
                        relevanceRequirement: items => CanPassThroughWreckedShip(items, false),
                        memoryAddress: 0x0,
                        memoryFlag: 0x8,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }

            private bool CanAccessFloodedCavernUnderWater(Progression items, bool requireRewards)
                => items.Morph && (
                    Logic.CanMoatSpeedBoost(items)
                    || items.Grapple
                    || items.SpaceJump
                    || (items.Gravity && (Logic.CanIbj(items) || items.HiJump))
                    || World.WreckedShip.CanEnter(items, true));

            private bool CanPassThroughWreckedShip(Progression items, bool requireRewards)
                => World.WreckedShip.CanEnter(items, requireRewards) && World.WreckedShip.CanAccessShutDownRooms(items, requireRewards);

            private bool CanAccessSkyItem(Progression items, bool requireRewards)
                => CanPassThroughWreckedShip(items, requireRewards) && (items.SpaceJump || items.SpeedBooster || !Config.LogicConfig.EasyEastCrateriaSkyItem);
        }

        public class TheMoatRoom : Room
        {
            public TheMoatRoom(EastCrateria region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "The Moat", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.CrateriaMoat, 0x8F8248, LocationType.Visible,
                        name: "Missile (Crateria moat)",
                        vanillaItem: ItemType.Missile,
                        memoryAddress: 0x0,
                        memoryFlag: 0x10,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }
    }
}
