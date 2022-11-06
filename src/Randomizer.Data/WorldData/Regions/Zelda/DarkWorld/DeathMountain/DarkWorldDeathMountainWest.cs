using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain
{
    public class DarkWorldDeathMountainWest : Z3Region
    {
        public DarkWorldDeathMountainWest(World world, Config config, IMetadataService? metadata, TrackerState? trackerState)
            : base(world, config, metadata, trackerState)
        {
            SpikeCave = new SpikeCaveRoom(this, metadata, trackerState);
            StartingRooms = new List<int>() { 67 };
            IsOverworld = true;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Dark World Death Mountain West");
        }

        public override string Name => "Dark World Death Mountain West";

        public override string Area => "Dark World";

        public SpikeCaveRoom SpikeCave { get; }

        public class SpikeCaveRoom : Room
        {
            public SpikeCaveRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Spike Cave", metadata)
            {
                Chest = new Location(this, 256 + 64, 0x1EA8B, LocationType.Regular,
                    name: "Spike Cave",
                    access: items => items.MoonPearl && items.Hammer && Logic.CanLiftLight(items) &&
                        ((Logic.CanExtendMagic(items, 2) && items.Cape) || items.Byrna) &&
                        World.LightWorldDeathMountainWest.CanEnter(items, true),
                    memoryAddress: 0x117,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Chest { get; }
        }
    }
}
