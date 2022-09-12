using System.Collections.Generic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Options;

namespace Randomizer.Data.WorldData.Regions.Zelda.DarkWorld
{
    public class DarkWorldMire : Z3Region
    {
        public DarkWorldMire(World world, Config config) : base(world, config)
        {
            MireShed = new MireShedRoom(this);

            StartingRooms = new List<int>() { 112 };
            IsOverworld = true;
        }

        public override string Name => "Dark World Mire";

        public override string Area => "Dark World";

        public MireShedRoom MireShed { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return (items.Flute && Logic.CanLiftHeavy(items)) || Logic.CanAccessMiseryMirePortal(items);
        }

        public class MireShedRoom : Room
        {
            public MireShedRoom(Region region) : base(region, "Mire Shed")
            {
                Left = new Location(this, 256 + 89, 0x1EA73, LocationType.Regular,
                    name: "Mire Shed - Left",
                    vanillaItem: ItemType.HeartPiece,
                    access: items => items.MoonPearl,
                    memoryAddress: 0x10D,
                    memoryFlag: 0x4);
                Right = new Location(this, 256 + 90, 0x1EA76, LocationType.Regular,
                    name: "Mire Shed - Right",
                    vanillaItem: ItemType.TwentyRupees,
                    access: items => items.MoonPearl,
                    memoryAddress: 0x10D,
                    memoryFlag: 0x5);
            }

            public Location Left { get; }

            public Location Right { get; }
        }
    }
}
