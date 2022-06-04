namespace Randomizer.SMZ3
{
    public abstract class SMRegion : Region
    {
        public SMRegion(World world, Config config) : base(world, config) { }

        public int MemoryRegionId { get; init; }
    }

}
