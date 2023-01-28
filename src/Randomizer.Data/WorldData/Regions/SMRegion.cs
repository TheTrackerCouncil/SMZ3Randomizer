using Randomizer.Data.Configuration;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions
{
    public abstract class SMRegion : Region
    {
        public SMRegion(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState) { }

        public int MemoryRegionId { get; init; }
    }

}
