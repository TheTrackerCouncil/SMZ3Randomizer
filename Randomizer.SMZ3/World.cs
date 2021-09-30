using System;
using System.Collections.Generic;
using System.Linq;

using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Regions.SuperMetroid;
using Randomizer.SMZ3.Regions.SuperMetroid.Brinstar;
using Randomizer.SMZ3.Regions.SuperMetroid.Crateria;
using Randomizer.SMZ3.Regions.SuperMetroid.Maridia;
using Randomizer.SMZ3.Regions.SuperMetroid.NorfairLower;
using Randomizer.SMZ3.Regions.SuperMetroid.NorfairUpper;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld.DeathMountain;
using Randomizer.SMZ3.Regions.Zelda.LightWorld;
using Randomizer.SMZ3.Regions.Zelda.LightWorld.DeathMountain;

using static Randomizer.SMZ3.Reward;

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
            CrateriaCentral = new(this, Config);
            CrateriaWest = new(this, Config);
            CrateriaEast = new(this, Config);
            BrinstarBlue = new(this, Config);
            BrinstarGreen = new(this, Config);
            BrinstarKraid = new(this, Config);
            BrinstarPink = new(this, Config);
            BrinstarRed = new(this, Config);
            MaridiaOuter = new(this, Config);
            MaridiaInner = new(this, Config);
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
                CrateriaCentral, CrateriaWest, CrateriaEast, BrinstarBlue,
                BrinstarGreen, BrinstarKraid, BrinstarPink, BrinstarRed,
                MaridiaOuter, MaridiaInner, UpperNorfairWest, UpperNorfairEast,
                UpperNorfairCrocomire, LowerNorfairWest, LowerNorfairEast,
                WreckedShip
            };
            Locations = Regions.SelectMany(x => x.Locations).ToList();
        }

        public Config Config { get; }
        public string Player { get; }
        public string Guid { get; }
        public int Id { get; }
        public IEnumerable<Region> Regions { get; }
        public IEnumerable<Location> Locations { get; }
        public IEnumerable<Item> Items => Locations.Select(l => l.Item).Where(i => i != null);

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
        public CrateriaCentral CrateriaCentral { get; }
        public CrateriaWest CrateriaWest { get; }
        public CrateriaEast CrateriaEast { get; }
        public BrinstarBlue BrinstarBlue { get; }
        public BrinstarGreen BrinstarGreen { get; }
        public BrinstarKraid BrinstarKraid { get; }
        public BrinstarPink BrinstarPink { get; }
        public BrinstarRed BrinstarRed { get; }
        public MaridiaOuter MaridiaOuter { get; }
        public MaridiaInner MaridiaInner { get; }
        public UpperNorfairWest UpperNorfairWest { get; }
        public UpperNorfairEast UpperNorfairEast { get; }
        public UpperNorfairCrocomire UpperNorfairCrocomire { get; }
        public LowerNorfairWest LowerNorfairWest { get; }
        public LowerNorfairEast LowerNorfairEast { get; }
        public WreckedShip WreckedShip { get; }

        public bool CanAquire(Progression items, Reward reward)
        {
            return Regions.OfType<IHasReward>().First(x => reward == x.Reward).CanComplete(items);
        }

        public bool CanAquireAll(Progression items, params Reward[] rewards)
        {
            return Regions.OfType<IHasReward>().Where(x => rewards.Contains(x.Reward)).All(x => x.CanComplete(items));
        }

        public void Setup(Random rnd)
        {
            SetMedallions(rnd);
            SetRewards(rnd);
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
                PendantGreen, PendantNonGreen, PendantNonGreen, CrystalRed, CrystalRed,
                CrystalBlue, CrystalBlue, CrystalBlue, CrystalBlue, CrystalBlue }.Shuffle(rnd);
            foreach (var region in Regions.OfType<IHasReward>().Where(x => x.Reward == None))
            {
                region.Reward = rewards.First();
                rewards.Remove(region.Reward);
            }
        }
    }
}
