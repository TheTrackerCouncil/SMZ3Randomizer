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
using Randomizer.Shared.Enums;

namespace Randomizer.Data.WorldData
{
    public class World
    {
        public World(Config config, string player, int id, string guid)
        {
            Config = config;
            Player = player;
            Id = id;
            Guid = guid;

            Logic = new Logic.Logic(this);

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
        public IEnumerable<Item> LocationItems => Locations.Select(l => l.Item).Where(i => i != null);
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

        public Location? LastClearedLocation { get; set; }

        public Location FindLocation(string name, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return Locations.FirstOrDefault(x => x.Name.Equals(name, comparisonType))
                ?? Locations.FirstOrDefault(x => x.AlternateNames.Contains(name, StringComparer.FromComparison(comparisonType)))
                ?? Locations.FirstOrDefault(x => x.ToString().Equals(name, comparisonType));
        }

        public bool CanAquire(Progression items, RewardType reward)
        {
            var dungeonWithReward = Regions.OfType<IHasReward>().FirstOrDefault(x => reward == x.RewardType);
            if (dungeonWithReward == null)
                return false;
            return dungeonWithReward.CanComplete(items);
        }

        public bool CanAquireAll(Progression items, params RewardType[] rewards)
        {
            return Regions.OfType<IHasReward>().Where(x => rewards.Contains(x.RewardType)).All(x => x.CanComplete(items));
        }

        public bool CanDefeatAll(Progression items, params BossType[] bosses)
        {
            return Regions.OfType<IHasBoss>().Where(x => bosses.Contains(x.BossType)).All(x => x.CanBeatBoss(items));
        }

        public void Setup(Random rnd)
        {
            SetMedallions(rnd);
            SetRewards(rnd);
        }

        public TrackerState State { get; set; }

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
                region.RewardType = rewards.First();
                rewards.Remove(region.RewardType);
            }

            foreach (var region in Regions.OfType<IHasReward>())
            {
                region.Reward = new Reward(region.RewardType, this, region);
            }
        }
    }
}
