using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain
{
    public class DarkWorldDeathMountainEast : Z3Region
    {
        public DarkWorldDeathMountainEast(World world, Config config, IMetadataService? metadata, TrackerState? trackerState)
            : base(world, config, metadata, trackerState)
        {
            HookshotCave = new HookshotCaveRoom(this, metadata, trackerState);
            SuperbunnyCave = new SuperbunnyCaveRoom(this, metadata, trackerState);
            StartingRooms = new List<int>() { 69, 71 };
            IsOverworld = true;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Dark World Death Mountain East");
        }

        public override string Name => "Dark World Death Mountain East";

        public override string Area => "Dark World";

        public HookshotCaveRoom HookshotCave { get; }

        public SuperbunnyCaveRoom SuperbunnyCave { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return Logic.CanLiftHeavy(items) && World.LightWorldDeathMountainEast.CanEnter(items, requireRewards);
        }

        public class HookshotCaveRoom : Room
        {
            public HookshotCaveRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Hookshot Cave", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.HookshotCaveTopRight, 0x1EB51, LocationType.Regular,
                        name: "Hookshot Cave - Top Right",
                        vanillaItem: ItemType.FiftyRupees,
                        access: items => items.MoonPearl && items.Hookshot,
                        memoryAddress: 0x3C,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.HookshotCaveTopLeft, 0x1EB54, LocationType.Regular,
                        name: "Hookshot Cave - Top Left",
                        vanillaItem: ItemType.FiftyRupees,
                        access: items => items.MoonPearl && items.Hookshot,
                        memoryAddress: 0x3C,
                        memoryFlag: 0x5,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.HookshotCaveBottomLeft, 0x1EB57, LocationType.Regular,
                        name: "Hookshot Cave - Bottom Left",
                        vanillaItem: ItemType.FiftyRupees,
                        access: items => items.MoonPearl && items.Hookshot,
                        memoryAddress: 0x3C,
                        memoryFlag: 0x6,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.HookshotCaveBottomRight, 0x1EB5A, LocationType.Regular,
                        name: "Hookshot Cave - Bottom Right",
                        vanillaItem: ItemType.FiftyRupees,
                        access: items => items.MoonPearl && (items.Hookshot || items.Boots),
                        memoryAddress: 0x3C,
                        memoryFlag: 0x7,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class SuperbunnyCaveRoom : Room
        {
            public SuperbunnyCaveRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Superbunny Cave", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.SuperbunnyCaveTop, 0x1EA7C, LocationType.Regular,
                        name: "Superbunny Cave - Top",
                        access: items => items.MoonPearl,
                        memoryAddress: 0xF8,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.SuperbunnyCaveBottom, 0x1EA7F, LocationType.Regular,
                        name: "Superbunny Cave - Bottom",
                        access: items => items.MoonPearl,
                        memoryAddress: 0xF8,
                        memoryFlag: 0x5,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }
    }
}
