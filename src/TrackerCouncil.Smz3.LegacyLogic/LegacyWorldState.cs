using System;
using System.Collections.Generic;
using System.Linq;
using static Randomizer.SMZ3.LegacyRewardType;

namespace Randomizer.SMZ3 {

    public class LegacyWorldState {

        public enum LegacyMedallion {
            Unknown,
            Bombos,
            Ether,
            Quake,
        }

        public IEnumerable<LegacyRewardType> Rewards { get; init; }
        public IEnumerable<LegacyMedallion> Medallions { get; init; }

        public int TowerCrystals { get; init; }
        public int GanonCrystals { get; init; }
        public int TourianBossTokens { get; init; }


    }

}
