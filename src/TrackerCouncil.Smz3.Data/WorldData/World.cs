using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Brinstar;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Crateria;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Maridia;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Norfair;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld.DeathMountain;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData;

public class World
{
    public World(Config config, string player, int id, string guid, bool isLocalWorld = true, IMetadataService? metadata = null, TrackerState? trackerState = null)
    {
        Config = config;
        Player = player;
        Id = id;
        Guid = guid;
        IsLocalWorld = isLocalWorld;

        Logic = new Smz3.Data.Logic.Logic(this);

        CreateBosses(metadata);
        CreateRewards(metadata);

        CastleTower = new(this, Config, metadata, trackerState);
        EasternPalace = new(this, Config, metadata, trackerState);
        DesertPalace = new(this, Config, metadata, trackerState);
        TowerOfHera = new(this, Config, metadata, trackerState);
        PalaceOfDarkness = new(this, Config, metadata, trackerState);
        SwampPalace = new(this, Config, metadata, trackerState);
        SkullWoods = new(this, Config, metadata, trackerState);
        ThievesTown = new(this, Config, metadata, trackerState);
        IcePalace = new(this, Config, metadata, trackerState);
        MiseryMire = new(this, Config, metadata, trackerState);
        TurtleRock = new(this, Config, metadata, trackerState);
        GanonsTower = new(this, Config, metadata, trackerState);
        LightWorldDeathMountainWest = new(this, Config, metadata, trackerState);
        LightWorldDeathMountainEast = new(this, Config, metadata, trackerState);
        LightWorldNorthWest = new(this, Config, metadata, trackerState);
        LightWorldNorthEast = new(this, Config, metadata, trackerState);
        LightWorldSouth = new(this, Config, metadata, trackerState);
        HyruleCastle = new(this, Config, metadata, trackerState);
        DarkWorldDeathMountainWest = new(this, Config, metadata, trackerState);
        DarkWorldDeathMountainEast = new(this, Config, metadata, trackerState);
        DarkWorldNorthWest = new(this, Config, metadata, trackerState);
        DarkWorldNorthEast = new(this, Config, metadata, trackerState);
        DarkWorldSouth = new(this, Config, metadata, trackerState);
        DarkWorldMire = new(this, Config, metadata, trackerState);
        CentralCrateria = new(this, Config, metadata, trackerState);
        WestCrateria = new(this, Config, metadata, trackerState);
        EastCrateria = new(this, Config, metadata, trackerState);
        BlueBrinstar = new(this, Config, metadata, trackerState);
        GreenBrinstar = new(this, Config, metadata, trackerState);
        KraidsLair = new(this, Config, metadata, trackerState);
        PinkBrinstar = new(this, Config, metadata, trackerState);
        RedBrinstar = new(this, Config, metadata, trackerState);
        OuterMaridia = new(this, Config, metadata, trackerState);
        InnerMaridia = new(this, Config, metadata, trackerState);
        UpperNorfairWest = new(this, Config, metadata, trackerState);
        UpperNorfairEast = new(this, Config, metadata, trackerState);
        UpperNorfairCrocomire = new(this, Config, metadata, trackerState);
        LowerNorfairWest = new(this, Config, metadata, trackerState);
        LowerNorfairEast = new(this, Config, metadata, trackerState);
        WreckedShip = new(this, Config, metadata, trackerState);
        Regions = new List<Region> {
            CastleTower, EasternPalace, DesertPalace, TowerOfHera,
            PalaceOfDarkness, SwampPalace, SkullWoods, ThievesTown,
            IcePalace, MiseryMire, TurtleRock, GanonsTower,
            LightWorldDeathMountainWest, LightWorldDeathMountainEast,
            LightWorldNorthWest, LightWorldNorthEast, LightWorldSouth,
            HyruleCastle, DarkWorldDeathMountainWest,
            DarkWorldDeathMountainEast, DarkWorldNorthWest,
            DarkWorldNorthEast, DarkWorldSouth, DarkWorldMire,
            CentralCrateria, WestCrateria, EastCrateria, BlueBrinstar,
            GreenBrinstar, KraidsLair, PinkBrinstar, RedBrinstar,
            OuterMaridia, InnerMaridia, UpperNorfairWest, UpperNorfairEast,
            UpperNorfairCrocomire, LowerNorfairWest, LowerNorfairEast,
            WreckedShip
        };
        Locations = Regions.SelectMany(x => x.Locations).ToImmutableList();
        LocationMap = Locations.ToImmutableDictionary(x => x.Id, x => x);
        Rooms = Regions.SelectMany(x => x.Rooms).ToImmutableList();
        State = trackerState ?? new TrackerState();
        ItemPools = new WorldItemPools(this);
        TreasureRegions = Regions.OfType<IHasTreasure>().ToImmutableList();
        RewardRegions = Regions.OfType<IHasReward>().ToImmutableList();
        BossRegions = Regions.OfType<IHasBoss>().ToImmutableList();
        PrerequisiteRegions = Regions.OfType<IHasPrerequisite>().ToImmutableList();
    }

    public Config Config { get; }
    public string Player { get; }
    public string Guid { get; }
    public int Id { get; }
    public bool HasCompleted { get; set; }
    public bool IsLocalWorld { get; set; }
    public IEnumerable<IHasReward> RewardRegions { get; set; }
    public IEnumerable<IHasTreasure> TreasureRegions { get; set; }
    public IEnumerable<IHasBoss> BossRegions { get; set; }
    public IEnumerable<IHasPrerequisite> PrerequisiteRegions { get; set; }
    public IEnumerable<Region> Regions { get; }
    public IEnumerable<Room> Rooms { get; }
    public IEnumerable<Location> Locations { get; }
    public IDictionary<LocationId, Location> LocationMap { get; }
    public List<Reward> Rewards = [];
    public List<Boss> Bosses = [];
    public IEnumerable<Item> LocationItems => Locations.Select(l => l.Item);
    public List<Item> CustomItems { get; } = [];
    public IEnumerable<Item> AllItems => CustomItems.Concat(LocationItems);
    public ILogic Logic { get; }
    public List<Boss> CustomBosses { get; } = [];

    public IEnumerable<Boss> GoldenBosses => Bosses.Where(x =>
        x.Type is BossType.Kraid or BossType.Phantoon or BossType.Draygon or BossType.Ridley);
    public IEnumerable<Boss> AllBosses => Bosses.Concat(CustomBosses);
    public CastleTower CastleTower { get; }
    public EasternPalace EasternPalace { get; }
    public DesertPalace DesertPalace { get; }
    public TowerOfHera TowerOfHera { get; }
    public PalaceOfDarkness PalaceOfDarkness { get; }
    public SwampPalace SwampPalace { get; }
    public SkullWoods SkullWoods { get; }
    public ThievesTown ThievesTown { get; }
    public IcePalace IcePalace { get; }
    public MiseryMire MiseryMire { get; }
    public TurtleRock TurtleRock { get; }
    public GanonsTower GanonsTower { get; }
    public LightWorldDeathMountainWest LightWorldDeathMountainWest { get; }
    public LightWorldDeathMountainEast LightWorldDeathMountainEast { get; }
    public LightWorldNorthWest LightWorldNorthWest { get; }
    public LightWorldNorthEast LightWorldNorthEast { get; }
    public LightWorldSouth LightWorldSouth { get; }
    public HyruleCastle HyruleCastle { get; }
    public DarkWorldDeathMountainWest DarkWorldDeathMountainWest { get; }
    public DarkWorldDeathMountainEast DarkWorldDeathMountainEast { get; }
    public DarkWorldNorthWest DarkWorldNorthWest { get; }
    public DarkWorldNorthEast DarkWorldNorthEast { get; }
    public DarkWorldSouth DarkWorldSouth { get; }
    public DarkWorldMire DarkWorldMire { get; }
    public CentralCrateria CentralCrateria { get; }
    public WestCrateria WestCrateria { get; }
    public EastCrateria EastCrateria { get; }
    public BlueBrinstar BlueBrinstar { get; }
    public GreenBrinstar GreenBrinstar { get; }
    public KraidsLair KraidsLair { get; }
    public PinkBrinstar PinkBrinstar { get; }
    public RedBrinstar RedBrinstar { get; }
    public OuterMaridia OuterMaridia { get; }
    public InnerMaridia InnerMaridia { get; }
    public UpperNorfairWest UpperNorfairWest { get; }
    public UpperNorfairEast UpperNorfairEast { get; }
    public UpperNorfairCrocomire UpperNorfairCrocomire { get; }
    public LowerNorfairWest LowerNorfairWest { get; }
    public LowerNorfairEast LowerNorfairEast { get; }
    public WreckedShip WreckedShip { get; }
    public WorldItemPools ItemPools { get; }
    public IEnumerable<PlayerHintTile> HintTiles { get; set; } = [];

    public IEnumerable<LocationId> ActiveHintTileLocations => HintTiles
        .Where(x => x.State?.HintState == HintState.Viewed && x.Locations?.Any() == true && x.WorldId == Id)
        .SelectMany(x => x.Locations!);

    public Location? LastClearedLocation { get; set; }

    public Location? FindLocation(string name, StringComparison comparisonType = StringComparison.Ordinal)
    {
        return Locations.FirstOrDefault(x => x.Name.Equals(name, comparisonType))
               ?? Locations.FirstOrDefault(x => x.ToString().Equals(name, comparisonType));
    }

    public void CreateRewards(IMetadataService? metadata)
    {
        RewardType[] rewardTypes = [ RewardType.PendantGreen, RewardType.PendantRed, RewardType.PendantBlue, RewardType.CrystalRed, RewardType.CrystalRed,
            RewardType.CrystalBlue, RewardType.CrystalBlue, RewardType.CrystalBlue, RewardType.CrystalBlue, RewardType.CrystalBlue, RewardType.Agahnim,
            RewardType.MetroidBoss, RewardType.MetroidBoss, RewardType.MetroidBoss, RewardType.MetroidBoss ];

        foreach (var rewardType in rewardTypes)
        {
            Rewards.Add(new Reward(rewardType, this, metadata));
        }
    }

    public void CreateBosses(IMetadataService? metadata)
    {
        BossType[] bossTypes =
        [
            BossType.Kraid, BossType.Phantoon, BossType.Draygon, BossType.Ridley, BossType.MotherBrain,
            BossType.CastleGuard, BossType.ArmosKnights, BossType.Lanmolas, BossType.Moldorm, BossType.HelmasaurKing,
            BossType.Arrghus, BossType.Blind, BossType.Mothula, BossType.Kholdstare, BossType.Vitreous,
            BossType.Trinexx, BossType.Agahnim, BossType.Ganon
        ];

        foreach (var bossType in bossTypes)
        {
            Bosses.Add(new Boss(bossType, this, metadata));
        }
    }

    /// <summary>
    /// Returns the Location object matching the given ID.
    /// We can be confident this won't throw an exception because we have a
    /// test that ensures every LocationId is used exactly once.
    /// </summary>
    public Location FindLocation(LocationId id)
    {
        return Locations.First(x => x.Id == id);
    }

    public bool CanAquire(Progression items, RewardType reward)
    {
        var dungeonWithReward = Regions.OfType<IHasReward>().FirstOrDefault(x => reward == x.RewardType);
        return dungeonWithReward != null && dungeonWithReward.CanRetrieveReward(items);
    }

    public bool CanAquireAll(Progression items, params RewardType[] rewards)
    {
        return Regions.OfType<IHasReward>().Where(x => rewards.Contains(x.RewardType)).All(x => x.CanRetrieveReward(items));
    }

    public bool CanDefeatAll(Progression items, params BossType[] bosses)
    {
        return BossRegions.Where(x => bosses.Contains(x.BossType)).All(x => x.CanBeatBoss(items));
    }

    public int CanDefeatBossCount(Progression items, params BossType[] bosses)
    {
        return BossRegions.Where(x => bosses.Contains(x.BossType)).Count(x => x.CanBeatBoss(items));
    }

    public bool HasDefeated(params BossType[] bosses)
    {
        return Bosses.Where(x => bosses.Contains(x.Type)).All(x => x.Defeated);
    }

    public Boss GetBossOfType(BossType type)
    {
        return Bosses.First(x => x.Type == type);
    }

    public void Setup(Random rnd)
    {
        SetMedallions(rnd);
        SetRewards(rnd);
        SetBottles(rnd);
    }

    public TrackerState? State { get; set; }

    private void SetMedallions(Random rnd)
    {
        foreach (var region in Regions.OfType<IHasPrerequisite>())
        {
            region.RequiredItem = rnd.Next(3) switch
            {
                0 => ItemType.Bombos,
                1 => ItemType.Ether,
                _ => ItemType.Quake,
            };
        }
    }

    private void SetRewards(Random rnd)
    {
        var rewards = Rewards.Where(x => x.Type.IsInCategory(RewardCategory.Zelda) && !x.Type.IsInCategory(RewardCategory.NonRandomized)).Shuffle(rnd);
        foreach (var region in RewardRegions.Where(x => x.IsShuffledReward))
        {
            region.SetReward(rewards.First());
            rewards.Remove(region.Reward);
        }
    }

    private void SetBottles(Random rnd)
    {
        if (!Config.CasPatches.RandomizedBottles) return;
        var bottleTypes = new List<ItemType>()
        {
            ItemType.Bottle,
            ItemType.BottleWithBee,
            ItemType.BottleWithFairy,
            ItemType.BottleWithBluePotion,
            ItemType.BottleWithGoldBee,
            ItemType.BottleWithGreenPotion,
            ItemType.BottleWithRedPotion
        };
        foreach (var bottleItem in ItemPools.AllItems.Where(x => x.Type == ItemType.Bottle))
        {
            var newType = bottleTypes.Random(rnd);
            bottleItem.UpdateItemType(newType);
        }
    }

    public int CountReceivedReward(Progression items, RewardType reward)
    {
        return CountReceivedRewards(items, [reward]);
    }

    public int CountReceivedRewards(Progression items, IList<RewardType> rewards)
    {
        return RewardRegions
            .Where(x => rewards.Contains(x.MarkedReward))
            .Count(x => x.HasReceivedReward);
    }
}
