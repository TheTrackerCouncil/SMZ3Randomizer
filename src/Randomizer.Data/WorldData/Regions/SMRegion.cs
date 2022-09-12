using Randomizer.Data.Options;
using Randomizer.Data.WorldData;

namespace Randomizer.Data.WorldData.Regions
{
    public abstract class SMRegion : Region
    {
        public SMRegion(World world, Config config) : base(world, config) { }

        public int MemoryRegionId { get; init; }
    }

}
