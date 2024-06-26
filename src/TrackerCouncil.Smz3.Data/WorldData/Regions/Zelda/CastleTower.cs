using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda
{
    public class CastleTower : Z3Region, IHasReward, IDungeon
    {
        public CastleTower(World world, Config config, IMetadataService? metadata, TrackerState? trackerState)
            : base(world, config, metadata, trackerState)
        {
            RegionItems = new[] { ItemType.KeyCT };

            Foyer = new FoyerRoom(this, metadata, trackerState);
            DarkMaze = new DarkMazeRoom(this, metadata, trackerState);

            StartingRooms = new List<int>() { 224 };
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Castle Tower");
            DungeonMetadata = metadata?.Dungeon(GetType()) ?? new DungeonInfo("Castle Tower", "Agahnim");
            DungeonState = trackerState?.DungeonStates.First(x => x.WorldId == world.Id && x.Name == GetType().Name) ?? new TrackerDungeonState();
            Reward = new Reward(RewardType.Agahnim, world, this, metadata, DungeonState);
            MapName = "Light World";
        }

        public override string Name => "Castle Tower";

        public RewardType DefaultRewardType => RewardType.Agahnim;

        public override List<string> AlsoKnownAs { get; }
            = new List<string>() { "Agahnim's Tower", "Hyrule Castle Tower" };

        public int SongIndex { get; init; } = 2;
        public string Abbreviation => "AT";
        public LocationId? BossLocationId => null;

        public Reward Reward { get; set; }

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
            public FoyerRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Foyer", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(region, LocationId.CastleTowerFoyer, 0x1EAB5, LocationType.Regular,
                        name: "Castle Tower - Foyer",
                        memoryAddress: 0xE0,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class DarkMazeRoom : Room
        {
            public DarkMazeRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Dark Maze", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(region, LocationId.CastleTowerDarkMaze, 0x1EAB2, LocationType.Regular,
                        name: "Castle Tower - Dark Maze",
                        access: items => items.Lamp && items.KeyCT >= 1,
                        memoryAddress: 0xD0,
                        memoryFlag: 0x4,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }
    }
}
