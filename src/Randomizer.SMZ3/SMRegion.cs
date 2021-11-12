namespace Randomizer.SMZ3
{
    public abstract class SMRegion : Region {
        public SMLogic Logic => Config.SMLogic;
        public SMRegion(World world, Config config) : base(world, config) { }
    }

}
