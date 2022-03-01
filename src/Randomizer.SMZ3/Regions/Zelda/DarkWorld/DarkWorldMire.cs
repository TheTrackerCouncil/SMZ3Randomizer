using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld
{
    public class DarkWorldMire : Z3Region
    {
        public DarkWorldMire(World world, Config config) : base(world, config)
        {
            MireShed = new(this);
        }

        public override string Name => "Dark World Mire";

        public override string Area => "Dark World";

        public MireShedRoom MireShed { get; }

        public override bool CanEnter(Progression items)
        {
            return (items.Flute && World.AdvancedLogic.CanLiftHeavy(items)) || World.AdvancedLogic.CanAccessMiseryMirePortal(items);
        }

        public class MireShedRoom : Room
        {
            public MireShedRoom(Region region) : base(region, "Mire Shed")
            {
                Left = new Location(this, 256 + 89, 0x1EA73, LocationType.Regular,
                    "Mire Shed - Left",
                    ItemType.HeartPiece,
                    items => items.MoonPearl);
                Right = new Location(this, 256 + 90, 0x1EA76, LocationType.Regular,
                    "Mire Shed - Right",
                    ItemType.TwentyRupees,
                    items => items.MoonPearl);
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
