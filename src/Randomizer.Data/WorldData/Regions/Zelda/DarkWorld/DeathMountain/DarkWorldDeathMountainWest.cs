using System.Collections.Generic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Data.Options;

namespace Randomizer.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain
{
    public class DarkWorldDeathMountainWest : Z3Region
    {
        public DarkWorldDeathMountainWest(World world, Config config)
            : base(world, config)
        {
            SpikeCave = new SpikeCaveRoom(this);
            StartingRooms = new List<int>() { 67 };
            IsOverworld = true;
        }

        public override string Name => "Dark World Death Mountain West";

        public override string Area => "Dark World";

        public SpikeCaveRoom SpikeCave { get; }

        public class SpikeCaveRoom : Room
        {
            public SpikeCaveRoom(Region region)
                : base(region, "Spike Cave")
            {
                Chest = new Location(this, 256 + 64, 0x1EA8B, LocationType.Regular,
                    name: "Spike Cave",
                    access: items => items.MoonPearl && items.Hammer && Logic.CanLiftLight(items) &&
                        ((Logic.CanExtendMagic(items, 2) && items.Cape) || items.Byrna) &&
                        World.LightWorldDeathMountainWest.CanEnter(items, true),
                    memoryAddress: 0x117,
                    memoryFlag: 0x4);
            }

            public Location Chest { get; }
        }
    }
}
