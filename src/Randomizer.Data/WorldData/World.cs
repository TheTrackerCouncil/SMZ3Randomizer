using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Randomizer.Shared;
using Randomizer.Shared.Models;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.SuperMetroid;
using Randomizer.Data.WorldData.Regions.SuperMetroid.Brinstar;
using Randomizer.Data.WorldData.Regions.SuperMetroid.Crateria;
using Randomizer.Data.WorldData.Regions.SuperMetroid.Maridia;
using Randomizer.Data.WorldData.Regions.SuperMetroid.Norfair;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Data.WorldData.Regions.Zelda.DarkWorld;
using Randomizer.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain;
using Randomizer.Data.WorldData.Regions.Zelda.LightWorld;
using Randomizer.Data.WorldData.Regions.Zelda.LightWorld.DeathMountain;
using Randomizer.Data.Logic;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Enums;

namespace Randomizer.Data.WorldData
{
    public class World
    {
        public World(Config config, string player, int id, string guid, bool isLocalWorld = true, IMetadataService? metadata = null, TrackerState? trackerState = null)
        {
            Config = config;
            Player = player;
            Id = id;
            Guid = guid;
            IsLocalWorld = isLocalWorld;

            Logic = new Logic.Logic(this);

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
            Rooms = Regions.SelectMany(x => x.Rooms).ToImmutableList();
            State = trackerState ?? new TrackerState();
            ItemPools = new WorldItemPools(this);

            /*if (metadata != null && trackerState != null)
            {
                LoadAdditionalData(metadata, trackerState);
            }*/
        }

        public Config Config { get; }
        public string Player { get; }
        public string Guid { get; }
        public int Id { get; }
        public bool IsLocalWorld { get; set; }
        public IEnumerable<Region> Regions { get; }
        public IEnumerable<Room> Rooms { get; }
        public IEnumerable<Location> Locations { get; }
        public IEnumerable<Item> LocationItems => Locations.Select(l => l.Item);
        public List<Item> TrackerItems { get; } = new List<Item>();
        public IEnumerable<Item> AllItems => TrackerItems.Concat(LocationItems);
        public ILogic Logic { get; }
        public IEnumerable<Reward> Rewards => Regions.OfType<IHasReward>().Select(x => x.Reward);
        public List<Boss> TrackerBosses { get; } = new List<Boss>();
        public IEnumerable<Boss> GoldenBosses => Regions.OfType<IHasBoss>().Select(x => x.Boss);
        public IEnumerable<Boss> AllBosses => GoldenBosses.Concat(TrackerBosses);
        public IEnumerable<IDungeon> Dungeons => Regions.OfType<IDungeon>();
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

        public Location? LastClearedLocation { get; set; }

        public Location? FindLocation(string name, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return Locations.FirstOrDefault(x => x.Name.Equals(name, comparisonType))
                ?? Locations.FirstOrDefault(x => x.AlternateNames.Contains(name, StringComparer.FromComparison(comparisonType)))
                ?? Locations.FirstOrDefault(x => x.ToString().Equals(name, comparisonType));
        }

        public bool CanAquire(Progression items, RewardType reward)
        {
            var dungeonWithReward = Regions.OfType<IHasReward>().FirstOrDefault(x => reward == x.RewardType);
            return dungeonWithReward != null && dungeonWithReward.CanComplete(items);
        }

        public bool CanAquireAll(Progression items, params RewardType[] rewards)
        {
            return Regions.OfType<IHasReward>().Where(x => rewards.Contains(x.RewardType)).All(x => x.CanComplete(items));
        }

        public bool CanDefeatAll(Progression items, params BossType[] bosses)
        {
            return Regions.OfType<IHasBoss>().Where(x => bosses.Contains(x.BossType)).All(x => x.CanBeatBoss(items));
        }

        public int CanDefeatBossCount(Progression items, params BossType[] bosses)
        {
            return Regions.OfType<IHasBoss>().Where(x => bosses.Contains(x.BossType)).Count(x => x.CanBeatBoss(items));
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
            foreach (var region in Regions.OfType<INeedsMedallion>())
            {
                region.Medallion = rnd.Next(3) switch
                {
                    0 => ItemType.Bombos,
                    1 => ItemType.Ether,
                    _ => ItemType.Quake,
                };
            }
        }

        private void SetRewards(Random rnd)
        {
            var rewards = new[] {
                RewardType.PendantGreen, RewardType.PendantRed, RewardType.PendantBlue, RewardType.CrystalRed, RewardType.CrystalRed,
                RewardType.CrystalBlue, RewardType.CrystalBlue, RewardType.CrystalBlue, RewardType.CrystalBlue, RewardType.CrystalBlue }.Shuffle(rnd);
            foreach (var region in Regions.OfType<IHasReward>().Where(x => x.RewardType == RewardType.None))
            {
                region.Reward = new Reward(rewards.First(), this, region);
                rewards.Remove(region.RewardType);
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

        private void LoadAdditionalData(IMetadataService metadata, TrackerState trackerState)
        {
            foreach (var itemMetadata in metadata.Items.Where(m => !LocationItems.Any(i => i.Is(m.InternalItemType, m.Item))))
            {
                var itemState = trackerState.ItemStates.FirstOrDefault(s =>
                    s.ItemName == itemMetadata.Item && s.Type == itemMetadata.InternalItemType);

                if (itemState == null)
                {
                    itemState = new TrackerItemState
                    {
                        ItemName = itemMetadata.Item,
                        Type = itemMetadata.InternalItemType,
                        TrackerState = trackerState,
                        WorldId = Id
                    };
                    trackerState.ItemStates.Add(itemState);
                }

                TrackerItems.Add(new Item(itemMetadata.InternalItemType, this, itemMetadata.Item, itemMetadata, itemState));
            }

            foreach (var bossMetadata in metadata.Bosses.Where(m => !GoldenBosses.Any(b => b.Is(m.Type, m.Boss))))
            {
                var bossState = trackerState.BossStates.FirstOrDefault(s =>
                    s.BossName == bossMetadata.Boss && s.Type == bossMetadata.Type);

                if (bossState == null)
                {
                    bossState = new TrackerBossState
                    {
                        BossName = bossMetadata.Boss,
                        Type = bossMetadata.Type,
                        TrackerState = trackerState,
                        WorldId = Id,
                    };
                    trackerState.BossStates.Add(bossState);
                }

                TrackerBosses.Add(new Boss(bossMetadata.Type, this, bossMetadata.Boss, bossMetadata, bossState));
            }
        }
    }
}
