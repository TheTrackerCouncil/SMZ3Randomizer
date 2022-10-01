using System.Collections.Generic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class CastleTower : Z3Region, IHasReward, IDungeon
    {
        public CastleTower(World world, Config config)
            : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyCT };

            Foyer = new FoyerRoom(this);
            DarkMaze = new DarkMazeRoom(this);

            StartingRooms = new List<int>() { 224 };

            Reward = new Reward(RewardType.Agahnim, world, this);
        }

        public override string Name => "Castle Tower";

        public override List<string> AlsoKnownAs { get; }
            = new List<string>() { "Agahnim's Tower", "Hyrule Castle Tower" };

        public Reward Reward { get; set; }

        public RewardType RewardType { get; set; } = RewardType.Agahnim;

        public FoyerRoom Foyer { get; }

        public DarkMazeRoom DarkMaze { get; }

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public Region ParentRegion => World.LightWorldNorthEast;

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return Logic.CanKillManyEnemies(items) && (items.Cape || items.MasterSword);
        }

        public bool CanComplete(Progression items)
        {
            return CanEnter(items, true) && items.Lamp && items.KeyCT >= 2 && items.Sword;
        }

        public class FoyerRoom : Room
        {
            public FoyerRoom(Region region)
                : base(region, "Foyer")
            {
                Chest = new Location(region, 256 + 101, 0x1EAB5, LocationType.Regular, 
                    name: "Castle Tower - Foyer",
                    memoryAddress: 0xE0, 
                    memoryFlag: 0x4);
            }

            public Location Chest { get; }
        }

        public class DarkMazeRoom : Room
        {
            public DarkMazeRoom(Region region)
                : base(region, "Dark Maze")
            {
                Chest = new Location(region, 256 + 102, 0x1EAB2, LocationType.Regular, 
                    name: "Castle Tower - Dark Maze",
                    access: items => items.Lamp && items.KeyCT >= 1, 
                    memoryAddress: 0xD0, 
                    memoryFlag: 0x4);
            }

            public Location Chest { get; }
        }
    }
}
