using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;

public class CastleTower : Z3Region, IHasReward, IHasTreasure, IHasBoss
{
    public CastleTower(World world, Config config, IMetadataService? metadata, TrackerState? trackerState)
        : base(world, config, metadata, trackerState)
    {
        RegionItems = new Dictionary<ItemType, ItemType>()
        {
            { ItemType.Key, ItemType.KeyCT }
        };

        Foyer = new FoyerRoom(this, metadata, trackerState);
        DarkMaze = new DarkMazeRoom(this, metadata, trackerState);

        StartingRooms = new List<int>() { 224 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Castle Tower");
        MapName = "Light World";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Castle Tower";

    public RewardType DefaultRewardType => RewardType.Agahnim;

    public BossType DefaultBossType => BossType.Agahnim;

    public bool IsShuffledReward => false;

    public override List<string> AlsoKnownAs { get; } = ["Agahnim's Tower", "Hyrule Castle Tower"];

    public LocationId? BossLocationId => null;

    public bool UnifiedBossAndItemLocation => false;

    public Reward Reward { get; set; } = null!;

    public FoyerRoom Foyer { get; }

    public DarkMazeRoom DarkMaze { get; }

    public TrackerRewardState RewardState { get; set; } = null!;

    public TrackerTreasureState TreasureState { get; set; } = null!;
    public event EventHandler? UpdatedTreasure;
    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public Boss Boss { get; set; } = null!;

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return Logic.CanKillManyEnemies(items) && (items.Cape || items.MasterSword);
    }

    public bool CanBeatBoss(Progression items) => CanRetrieveReward(items);

    public bool CanRetrieveReward(Progression items)
    {
        return CanEnter(items, true) && items.Lamp && items.KeyCT >= 2 && items.Sword;
    }

    public bool CanSeeReward(Progression items) => true;

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
