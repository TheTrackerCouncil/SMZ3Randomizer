using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class CastleTower : Z3Region, IHasReward
    {
        public CastleTower(World world, Config config)
            : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyCT };

            Foyer = new(this);
            DarkMaze = new(this);

            StartingRooms = new List<int>() { 224 };
        }

        public override string Name => "Castle Tower";

        public override List<string> AlsoKnownAs { get; }
            = new() { "Agahnim's Tower", "Hyrule Castle Tower" };

        public Reward Reward { get; set; } = Reward.Agahnim;

        public FoyerRoom Foyer { get; }

        public DarkMazeRoom DarkMaze { get; }

        public override bool CanEnter(Progression items)
        {
            return Logic.CanKillManyEnemies(items) && (items.Cape || items.MasterSword);
        }

        public bool CanComplete(Progression items)
        {
            return CanEnter(items) && items.Lamp && items.KeyCT >= 2 && items.Sword;
        }

        public class FoyerRoom : Room
        {
            public FoyerRoom(Region region)
                : base(region, "Foyer")
            {
                Chest = new(region, 256 + 101, 0x1EAB5, LocationType.Regular, "Castle Tower - Foyer", 0xE0, 0x4);
            }

            public Location Chest { get; }
        }

        public class DarkMazeRoom : Room
        {
            public DarkMazeRoom(Region region)
                : base(region, "Dark Maze")
            {
                Chest = new(region, 256 + 102, 0x1EAB2, LocationType.Regular, "Castle Tower - Dark Maze",
                    items => items.Lamp && items.KeyCT >= 1, 0xD0, 0x4);
            }

            public Location Chest { get; }
        }
    }
}
