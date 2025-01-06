using System;
using System.Collections.Generic;
using System.Linq;
using static Randomizer.SMZ3.LegacyRewardType;
using static Randomizer.SMZ3.LegacyWorldState;

namespace Randomizer.SMZ3 {

    public class LegacyWorld {

        internal List<LegacyLocation> Locations { get; set; }
        internal List<LegacyRegion> Regions { get; set; }
        public LegacyConfig LegacyConfig { get; set; }
        public string Player { get; set; }
        public string Guid { get; set; }
        public int Id { get; set; }
        public LegacyWorldState LegacyWorldState { get; set; }

        public int TowerCrystals => LegacyWorldState?.TowerCrystals ?? 7;
        public int GanonCrystals => LegacyWorldState?.GanonCrystals ?? 7;
        public int TourianBossTokens => LegacyWorldState?.TourianBossTokens ?? 4;

        internal IEnumerable<LegacyItem> Items {
            get { return Locations.Select(l => l.LegacyItem).Where(i => i != null); }
        }

        public bool ForwardSearch { get; set; } = false;

        private Dictionary<int, ILegacyReward[]> rewardLookup { get; set; }
        private Dictionary<string, LegacyLocation> locationLookup { get; set; }
        private Dictionary<string, LegacyRegion> regionLookup { get; set; }

        internal LegacyLocation GetLocation(string name) => locationLookup[name];
        internal LegacyRegion GetRegion(string name) => regionLookup[name];

        public LegacyWorld(LegacyConfig legacyConfig, string player, int id, string guid) {
            LegacyConfig = legacyConfig;
            Player = player;
            Id = id;
            Guid = guid;

            Regions = new List<LegacyRegion> {
                new Regions.Zelda.LegacyCastleTower(this, LegacyConfig),
                new Regions.Zelda.LegacyEasternPalace(this, LegacyConfig),
                new Regions.Zelda.LegacyDesertPalace(this, LegacyConfig),
                new Regions.Zelda.LegacyTowerOfHera(this, LegacyConfig),
                new Regions.Zelda.LegacyPalaceOfDarkness(this, LegacyConfig),
                new Regions.Zelda.LegacySwampPalace(this, LegacyConfig),
                new Regions.Zelda.LegacySkullWoods(this, LegacyConfig),
                new Regions.Zelda.LegacyThievesTown(this, LegacyConfig),
                new Regions.Zelda.LegacyIcePalace(this, LegacyConfig),
                new Regions.Zelda.LegacyMiseryMire(this, LegacyConfig),
                new Regions.Zelda.LegacyTurtleRock(this, LegacyConfig),
                new Regions.Zelda.LegacyGanonsTower(this, LegacyConfig),
                new Regions.Zelda.LightWorld.DeathMountain.LegacyWest(this, LegacyConfig),
                new Regions.Zelda.LightWorld.DeathMountain.LegacyEast(this, LegacyConfig),
                new Regions.Zelda.LightWorld.LegacyNorthWest(this, LegacyConfig),
                new Regions.Zelda.LightWorld.LegacyNorthEast(this, LegacyConfig),
                new Regions.Zelda.LightWorld.LegacySouth(this, LegacyConfig),
                new Regions.Zelda.LegacyHyruleCastle(this, LegacyConfig),
                new Regions.Zelda.DarkWorld.DeathMountain.LegacyWest(this, LegacyConfig),
                new Regions.Zelda.DarkWorld.DeathMountain.LegacyEast(this, LegacyConfig),
                new Regions.Zelda.DarkWorld.LegacyNorthWest(this, LegacyConfig),
                new Regions.Zelda.DarkWorld.LegacyNorthEast(this, LegacyConfig),
                new Regions.Zelda.DarkWorld.LegacySouth(this, LegacyConfig),
                new Regions.Zelda.DarkWorld.LegacyMire(this, LegacyConfig),
                new Regions.SuperMetroid.Crateria.LegacyWest(this, LegacyConfig),
                new Regions.SuperMetroid.Crateria.LegacyCentral(this, LegacyConfig),
                new Regions.SuperMetroid.Crateria.LegacyEast(this, LegacyConfig),
                new Regions.SuperMetroid.Brinstar.LegacyBlue(this, LegacyConfig),
                new Regions.SuperMetroid.Brinstar.LegacyGreen(this, LegacyConfig),
                new Regions.SuperMetroid.Brinstar.LegacyPink(this, LegacyConfig),
                new Regions.SuperMetroid.Brinstar.LegacyRed(this, LegacyConfig),
                new Regions.SuperMetroid.Brinstar.LegacyKraid(this, LegacyConfig),
                new Regions.SuperMetroid.LegacyWreckedShip(this, LegacyConfig),
                new Regions.SuperMetroid.Maridia.LegacyOuter(this, LegacyConfig),
                new Regions.SuperMetroid.Maridia.LegacyInner(this, LegacyConfig),
                new Regions.SuperMetroid.NorfairUpper.LegacyWest(this, LegacyConfig),
                new Regions.SuperMetroid.NorfairUpper.LegacyEast(this, LegacyConfig),
                new Regions.SuperMetroid.NorfairUpper.LegacyCrocomire(this, LegacyConfig),
                new Regions.SuperMetroid.NorfairLower.LegacyWest(this, LegacyConfig),
                new Regions.SuperMetroid.NorfairLower.LegacyEast(this, LegacyConfig),
            };

            Locations = Regions.SelectMany(x => x.Locations).ToList();

            regionLookup = Regions.ToDictionary(r => r.Name, r => r);
            locationLookup = Locations.ToDictionary(l => l.Name, l => l);

            foreach(var region in Regions) {
                region.GenerateLocationLookup();
            }
        }

        public bool CanEnter(string regionName, LegacyProgression items) {
            var region = regionLookup[regionName];
            if (region == null)
                throw new ArgumentException($"World.CanEnter: Invalid region name {regionName}", nameof(regionName));
            return region.CanEnter(items);
        }

        public bool CanAcquire(LegacyProgression items, LegacyRewardType legacyReward) {
            // For the purpose of logic unit tests, if no region has the reward then CanAcquire is satisfied
            return Regions.OfType<ILegacyReward>().FirstOrDefault(x => legacyReward == x.LegacyReward)?.CanComplete(items) ?? false;
        }

        public bool CanAcquireAll(LegacyProgression items, LegacyRewardType legacyRewardsMask)
        {
            var count = (legacyRewardsMask & CrystalRed) == CrystalRed ? 2 :
                (legacyRewardsMask & AnyCrystal) == CrystalBlue ? 7 :
                (legacyRewardsMask & AnyPendant) == AnyPendant ? 3 :
                (legacyRewardsMask & AnyBossToken) == AnyBossToken ? 4 :
                1;
            return CanAcquireAtLeast(count, items, legacyRewardsMask);
        }

        public bool CanAcquireAtLeast(int amount, LegacyProgression items, LegacyRewardType legacyRewardsMask) {
            return rewardLookup[(int)legacyRewardsMask].Where(x => x.CanComplete(items)).Count() >= amount;
        }

        public void Setup(LegacyWorldState state) {
            LegacyWorldState = state;
            SetRewards(state.Rewards);
            SetMedallions(state.Medallions);
            SetRewardLookup();
        }

        void SetRewards(IEnumerable<LegacyRewardType> rewards) {
            var regions = Regions.OfType<ILegacyReward>().Where(x => x.LegacyReward != Agahnim);
            foreach (var (region, reward) in regions.Zip(rewards)) {
                region.LegacyReward = reward;
            }
        }

        void SetMedallions(IEnumerable<LegacyMedallion> medallions) {
            (GetRegion("Misery Mire") as ILegacyMedallionAccess).LegacyMedallion = medallions.First();
            (GetRegion("Turtle Rock") as ILegacyMedallionAccess).LegacyMedallion = medallions.Skip(1).First();
        }

        void SetRewardLookup() {
            /* Generate a lookup of all possible regions for any given reward combination for faster lookup later */
            rewardLookup = new Dictionary<int, ILegacyReward[]>();
            for (var i = 0; i < 512; i += 1) {
                rewardLookup.Add(i, Regions.OfType<ILegacyReward>().Where(x => (((int)x.LegacyReward) & i) != 0).ToArray());
            }
        }

        private static Dictionary<int, int> s_locationIdMappings = new()
        {
            { 256 + 196, 256 + 230 }, // GanonsTowerRandomizerRoomTopLeft
            { 256 + 197, 256 + 231 }, // GanonsTowerRandomizerRoomTopRight
            { 256 + 198, 256 + 232 }, // GanonsTowerRandomizerRoomBottomLeft
            { 256 + 199, 256 + 233 }, // GanonsTowerRandomizerRoomBottomRight
            { 256 + 200, 256 + 234 }, // GanonsTowerHopeRoomLeft
            { 256 + 201, 256 + 235 }, // GanonsTowerHopeRoomRight
            { 256 + 202, 256 + 236 }, // GanonsTowerTileRoom
        };

        public bool IsLocationAccessible(int id, LegacyProgression items)
        {
            if (s_locationIdMappings.TryGetValue(id, out var newId))
            {
                id = newId;
            }

            var legacyLocation = Locations.First(x => x.Id == id);

            return legacyLocation.Available(items);
        }

        public bool CanCompleteRegion(string name, int? locationId, LegacyProgression items)
        {
            if (regionLookup.TryGetValue(name, out var regionByName) && regionByName is ILegacyReward rewardRegion)
            {
                return rewardRegion.CanComplete(items);
            }

            var legacyLocation = Locations.FirstOrDefault(x => x.Id == locationId);
            if (legacyLocation?.LegacyRegion is ILegacyReward locationRegion)
            {
                return locationRegion.CanComplete(items);
            }

            return false;
        }

    }

}
