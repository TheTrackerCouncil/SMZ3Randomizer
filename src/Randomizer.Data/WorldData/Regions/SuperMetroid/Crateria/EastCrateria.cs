using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Crateria
{
    public class EastCrateria : SMRegion
    {
        public EastCrateria(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            FloodedCavernUnderWater = new Location(this, 1, 0x8F81E8, LocationType.Visible,
                name: "Missile (outside Wrecked Ship bottom)",
                alsoKnownAs: new[] { "Flooded Cavern - under water", "West Ocean - under water" },
                vanillaItem: ItemType.Missile,
                access: items => CanAccessFloodedCavernUnderWater(items, true),
                relevanceRequirement: items => CanAccessFloodedCavernUnderWater(items, false),
                memoryAddress: 0x0,
                memoryFlag: 0x2,
                metadata: metadata,
                trackerState: trackerState);
            SkyMissile = new Location(this, 2, 0x8F81EE, LocationType.Hidden,
                name: "Missile (outside Wrecked Ship top)",
                alsoKnownAs: new[] { "Sky Missile" },
                vanillaItem: ItemType.Missile,
                access: items => CanAccessSkyItem(items, true),
                relevanceRequirement: items => CanAccessSkyItem(items, false),
                memoryAddress: 0x0,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState);
            MorphBallMaze = new Location(this, 3, 0x8F81F4, LocationType.Visible,
                name: "Missile (outside Wrecked Ship middle)",
                alsoKnownAs: new[] { "Morph Ball Maze" },
                vanillaItem: ItemType.Missile,
                access: items => CanPassThroughWreckedShip(items, true),
                relevanceRequirement: items => CanPassThroughWreckedShip(items, false),
                memoryAddress: 0x0,
                memoryFlag: 0x8,
                metadata: metadata,
                trackerState: trackerState);
            Moat = new Location(this, 4, 0x8F8248, LocationType.Visible,
                name: "Missile (Crateria moat)",
                alsoKnownAs: new[] { "The Moat", "Interior Lake" },
                vanillaItem: ItemType.Missile,
                memoryAddress: 0x0,
                memoryFlag: 0x10,
                metadata: metadata,
                trackerState: trackerState);
            MemoryRegionId = 0;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("East Crateria");
        }

        public override string Name => "East Crateria";

        public override string Area => "Crateria";

        public Location FloodedCavernUnderWater { get; }

        public Location SkyMissile { get; }

        public Location MorphBallMaze { get; }

        public Location Moat { get; }

        private bool CanAccessFloodedCavernUnderWater(Progression items, bool requireRewards)
            => items.Morph && (
                items.SpeedBooster
                || items.Grapple
                || items.SpaceJump
                || (items.Gravity && (Logic.CanIbj(items) || items.HiJump))
                || World.WreckedShip.CanEnter(items, true));

        private bool CanPassThroughWreckedShip(Progression items, bool requireRewards)
            => World.WreckedShip.CanEnter(items, requireRewards) && World.WreckedShip.CanAccessShutDownRooms(items, requireRewards);

        private bool CanAccessSkyItem(Progression items, bool requireRewards)
            => CanPassThroughWreckedShip(items, requireRewards) && (items.SpaceJump || items.SpeedBooster || !Config.LogicConfig.EasyEastCrateriaSkyItem);

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
                        World.InnerMaridia.DraygonTreasure.IsAvailable(items))) ||
                    /*Through Maridia from Pipe*/
                    (Logic.CanUsePowerBombs(items) && items.Super && items.Gravity);
        }
    }
}
