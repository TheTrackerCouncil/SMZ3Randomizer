using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld.DeathMountain
{
    public class DarkWorldDeathMountainEast : Z3Region
    {
        public DarkWorldDeathMountainEast(World world, Config config)
            : base(world, config)
        {
            HookshotCave = new(this);
            SuperbunnyCave = new(this);
        }

        public override string Name => "Dark World Death Mountain East";

        public override string Area => "Dark World";

        public HookshotCaveRoom HookshotCave { get; }

        public SuperbunnyCaveRoom SuperbunnyCave { get; }

        public override bool CanEnter(Progression items)
        {
            return items.CanLiftHeavy() && World.LightWorldDeathMountainEast.CanEnter(items);
        }

        public class HookshotCaveRoom : Room
        {
            public HookshotCaveRoom(Region region)
                : base(region, "Hookshot Cave")
            {
                TopRight = new Location(this, 256 + 65, 0x1EB51, LocationType.Regular,
                    "Hookshot Cave - Top Right",
                    ItemType.FiftyRupees,
                    items => items.MoonPearl && items.Hookshot);
                TopLeft = new Location(this, 256 + 66, 0x1EB54, LocationType.Regular,
                    "Hookshot Cave - Top Left",
                    ItemType.FiftyRupees,
                    items => items.MoonPearl && items.Hookshot);
                BottomLeft =new Location(this, 256 + 67, 0x1EB57, LocationType.Regular,
                    "Hookshot Cave - Bottom Left",
                    ItemType.FiftyRupees,
                    items => items.MoonPearl && items.Hookshot);
                BottomRight = new Location(this, 256 + 68, 0x1EB5A, LocationType.Regular,
                    "Hookshot Cave - Bottom Right",
                    ItemType.FiftyRupees,
                    items => items.MoonPearl && (items.Hookshot || items.Boots));
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
                    "Superbunny Cave - Top",
                    items => items.MoonPearl);
                Bottom = new Location(this, 256 + 70, 0x1EA7F, LocationType.Regular,
                    "Superbunny Cave - Bottom",
                    items => items.MoonPearl);
            }

            public Location Top { get; }

            public Location Bottom { get; }
        }
    }

}
