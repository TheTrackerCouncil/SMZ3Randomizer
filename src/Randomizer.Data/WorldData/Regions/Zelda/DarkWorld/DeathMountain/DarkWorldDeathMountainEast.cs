using System.Collections.Generic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Options;

namespace Randomizer.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain
{
    public class DarkWorldDeathMountainEast : Z3Region
    {
        public DarkWorldDeathMountainEast(World world, Config config)
            : base(world, config)
        {
            HookshotCave = new HookshotCaveRoom(this);
            SuperbunnyCave = new SuperbunnyCaveRoom(this);
            StartingRooms = new List<int>() { 69, 71 };
            IsOverworld = true;
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
            public HookshotCaveRoom(Region region)
                : base(region, "Hookshot Cave")
            {
                TopRight = new Location(this, 256 + 65, 0x1EB51, LocationType.Regular,
                    name: "Hookshot Cave - Top Right",
                    vanillaItem: ItemType.FiftyRupees,
                    access: items => items.MoonPearl && items.Hookshot,
                    memoryAddress: 0x3C,
                    memoryFlag: 0x4);
                TopLeft = new Location(this, 256 + 66, 0x1EB54, LocationType.Regular,
                    name: "Hookshot Cave - Top Left",
                    vanillaItem: ItemType.FiftyRupees,
                    access: items => items.MoonPearl && items.Hookshot,
                    memoryAddress: 0x3C,
                    memoryFlag: 0x5);
                BottomLeft =new Location(this, 256 + 67, 0x1EB57, LocationType.Regular,
                    name: "Hookshot Cave - Bottom Left",
                    vanillaItem: ItemType.FiftyRupees,
                    access: items => items.MoonPearl && items.Hookshot,
                    memoryAddress: 0x3C,
                    memoryFlag: 0x6);
                BottomRight = new Location(this, 256 + 68, 0x1EB5A, LocationType.Regular,
                    name: "Hookshot Cave - Bottom Right",
                    vanillaItem: ItemType.FiftyRupees,
                    access: items => items.MoonPearl && (items.Hookshot || items.Boots),
                    memoryAddress: 0x3C,
                    memoryFlag: 0x7);
            }

            public Location TopRight { get; }

            public Location TopLeft { get; }

            public Location BottomLeft { get; }

            public Location BottomRight { get; }
        }

        public class SuperbunnyCaveRoom : Room
        {
            public SuperbunnyCaveRoom(Region region)
                : base(region, "Superbunny Cave")
            {
                Top = new Location(this, 256 + 69, 0x1EA7C, LocationType.Regular,
                    name: "Superbunny Cave - Top",
                    access: items => items.MoonPearl,
                    memoryAddress: 0xF8,
                    memoryFlag: 0x4);
                Bottom = new Location(this, 256 + 70, 0x1EA7F, LocationType.Regular,
                    name: "Superbunny Cave - Bottom",
                    access: items => items.MoonPearl,
                    memoryAddress: 0xF8,
                    memoryFlag: 0x5);
            }

            public Location Top { get; }

            public Location Bottom { get; }
        }
    }

}
