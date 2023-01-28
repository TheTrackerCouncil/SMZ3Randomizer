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
                TopRight = new Location(this, 256 + 65, 0x1EB51, LocationType.Regular,
                    name: "Hookshot Cave - Top Right",
                    vanillaItem: ItemType.FiftyRupees,
                    access: items => items.MoonPearl && items.Hookshot,
                    memoryAddress: 0x3C,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
                TopLeft = new Location(this, 256 + 66, 0x1EB54, LocationType.Regular,
                    name: "Hookshot Cave - Top Left",
                    vanillaItem: ItemType.FiftyRupees,
                    access: items => items.MoonPearl && items.Hookshot,
                    memoryAddress: 0x3C,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);
                BottomLeft =new Location(this, 256 + 67, 0x1EB57, LocationType.Regular,
                    name: "Hookshot Cave - Bottom Left",
                    vanillaItem: ItemType.FiftyRupees,
                    access: items => items.MoonPearl && items.Hookshot,
                    memoryAddress: 0x3C,
                    memoryFlag: 0x6,
                    metadata: metadata,
                    trackerState: trackerState);
                BottomRight = new Location(this, 256 + 68, 0x1EB5A, LocationType.Regular,
                    name: "Hookshot Cave - Bottom Right",
                    vanillaItem: ItemType.FiftyRupees,
                    access: items => items.MoonPearl && (items.Hookshot || items.Boots),
                    memoryAddress: 0x3C,
                    memoryFlag: 0x7,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location TopRight { get; }

            public Location TopLeft { get; }

            public Location BottomLeft { get; }

            public Location BottomRight { get; }
        }

        public class SuperbunnyCaveRoom : Room
        {
            public SuperbunnyCaveRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Superbunny Cave", metadata)
            {
                Top = new Location(this, 256 + 69, 0x1EA7C, LocationType.Regular,
                    name: "Superbunny Cave - Top",
                    access: items => items.MoonPearl,
                    memoryAddress: 0xF8,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
                Bottom = new Location(this, 256 + 70, 0x1EA7F, LocationType.Regular,
                    name: "Superbunny Cave - Bottom",
                    access: items => items.MoonPearl,
                    memoryAddress: 0xF8,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Top { get; }

            public Location Bottom { get; }
        }
    }

}
