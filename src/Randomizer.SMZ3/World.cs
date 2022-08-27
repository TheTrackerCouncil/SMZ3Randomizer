using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Randomizer.Shared;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Regions.SuperMetroid;
using Randomizer.SMZ3.Regions.SuperMetroid.Brinstar;
using Randomizer.SMZ3.Regions.SuperMetroid.Crateria;
using Randomizer.SMZ3.Regions.SuperMetroid.Maridia;
using Randomizer.SMZ3.Regions.SuperMetroid.Norfair;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld.DeathMountain;
using Randomizer.SMZ3.Regions.Zelda.LightWorld;
using Randomizer.SMZ3.Regions.Zelda.LightWorld.DeathMountain;

namespace Randomizer.SMZ3
{
    public class World
    {
        public World(Config config, string player, int id, string guid)
        {
            Config = config;
            Player = player;
            Id = id;
            Guid = guid;

            Logic = new Logic(this);

            CastleTower = new(this, Config);
            EasternPalace = new(this, Config);
            DesertPalace = new(this, Config);
            TowerOfHera = new(this, Config);
            PalaceOfDarkness = new(this, Config);
            SwampPalace = new(this, Config);
            SkullWoods = new(this, Config);
            ThievesTown = new(this, Config);
            IcePalace = new(this, Config);
            MiseryMire = new(this, Config);
            TurtleRock = new(this, Config);
            GanonsTower = new(this, Config);
            LightWorldDeathMountainWest = new(this, Config);
            LightWorldDeathMountainEast = new(this, Config);
            LightWorldNorthWest = new(this, Config);
            LightWorldNorthEast = new(this, Config);
            LightWorldSouth = new(this, Config);
            HyruleCastle = new(this, Config);
            DarkWorldDeathMountainWest = new(this, Config);
            DarkWorldDeathMountainEast = new(this, Config);
            DarkWorldNorthWest = new(this, Config);
            DarkWorldNorthEast = new(this, Config);
            DarkWorldSouth = new(this, Config);
            DarkWorldMire = new(this, Config);
            CentralCrateria = new(this, Config);
            WestCrateria = new(this, Config);
            EastCrateria = new(this, Config);
            BlueBrinstar = new(this, Config);
            GreenBrinstar = new(this, Config);
            KraidsLair = new(this, Config);
            PinkBrinstar = new(this, Config);
            RedBrinstar = new(this, Config);
            OuterMaridia = new(this, Config);
            InnerMaridia = new(this, Config);
            UpperNorfairWest = new(this, Config);
            UpperNorfairEast = new(this, Config);
            UpperNorfairCrocomire = new(this, Config);
            LowerNorfairWest = new(this, Config);
            LowerNorfairEast = new(this, Config);
            WreckedShip = new(this, Config);
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
        }

        public Config Config { get; }
        public string Player { get; }
        public string Guid { get; }
        public int Id { get; }
        public IEnumerable<Region> Regions { get; }
        public IEnumerable<Room> Rooms { get; }
        public IEnumerable<Location> Locations { get; }
        public IEnumerable<Item> Items => Locations.Select(l => l.Item).Where(i => i != null);
        public ILogic Logic { get; }

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

        public Location FindLocation(string name, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return Locations.FirstOrDefault(x => x.Name.Equals(name, comparisonType))
                ?? Locations.FirstOrDefault(x => x.AlternateNames.Contains(name, StringComparer.FromComparison(comparisonType)))
                ?? Locations.FirstOrDefault(x => x.ToString().Equals(name, comparisonType));
        }

        public bool CanAquire(Progression items, RewardType reward)
        {
            var dungeonWithReward = Regions.OfType<IHasReward>().FirstOrDefault(x => reward == x.Reward);
            if (dungeonWithReward == null)
                return false;
            return dungeonWithReward.CanComplete(items);
        }

        public bool CanAquireAll(Progression items, params RewardType[] rewards)
        {
            return Regions.OfType<IHasReward>().Where(x => rewards.Contains(x.Reward)).All(x => x.CanComplete(items));
        }

        public void Setup(Random rnd)
        {
            SetMedallions(rnd);
            SetRewards(rnd);
        }

        /// <summary>
        /// Creates a new empty <see cref="TrackerState"/> for this world
        /// instance.
        /// </summary>
        /// <returns>
        /// A new <see cref="TrackerState"/> with the items, rewards and
        /// medallions from this world.
        /// </returns>
        public TrackerState CreateTrackerState()
        {
            var locationStates = Locations
                .Select(x => new TrackerLocationState
                {
                    LocationId = x.Id,
                    Item = x.Item?.Type,
                    Cleared = false
                })
                .ToList();

            var regionStates = Regions
                .Select(x => new TrackerRegionState
                {
                    TypeName = x.GetType().Name,
                    Reward = x is IHasReward rewardRegion ? rewardRegion.Reward : null,
                    Medallion = x is INeedsMedallion medallionRegion ? medallionRegion.Medallion : null
                })
                .ToList();

            return new TrackerState
            {
                //ItemStates = new List<TrackerItemState>(),
                LocationStates = locationStates,
                RegionStates = regionStates,
                //DungeonStates = new List<TrackerDungeonState>(),
                //MarkedLocations = new List<TrackerMarkedLocation>(),
                //BossStates = new List<TrackerBossState>(),
                //History = new List<TrackerHistoryEvent>(),
                StartDateTime = DateTimeOffset.Now,
                UpdatedDateTime = DateTimeOffset.Now
            };
        }

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
            foreach (var region in Regions.OfType<IHasReward>().Where(x => x.Reward == RewardType.None))
            {
                region.Reward = rewards.First();
                rewards.Remove(region.Reward);
            }
        }
    }
}
