using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyItemType;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacyTowerOfHera : LegacyZ3Region, ILegacyReward {

        public override string Name => "Tower of Hera";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;

        public LegacyTowerOfHera(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            RegionItems = new[] { KeyTH, BigKeyTH, MapTH, CompassTH };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+115, 0x308162, LocationType.HeraStandingKey, "Tower of Hera - Basement Cage"),
                new LegacyLocation(this, 256+116, 0x1E9AD, LocationType.Regular, "Tower of Hera - Map Chest"),
                new LegacyLocation(this, 256+117, 0x1E9E6, LocationType.Regular, "Tower of Hera - Big Key Chest",
                    items => items.KeyTH && items.CanLightTorches())
                    .AlwaysAllow((item, items) => item.Is(KeyTH, LegacyWorld)),
                new LegacyLocation(this, 256+118, 0x1E9FB, LocationType.Regular, "Tower of Hera - Compass Chest",
                    items => items.BigKeyTH),
                new LegacyLocation(this, 256+119, 0x1E9F8, LocationType.Regular, "Tower of Hera - Big Chest",
                    items => items.BigKeyTH),
                new LegacyLocation(this, 256+120, 0x308152, LocationType.Regular, "Tower of Hera - Moldorm",
                    items => items.BigKeyTH && CanBeatBoss(items)),
            };
        }

        private bool CanBeatBoss(LegacyProgression items) {
            return items.Sword || items.Hammer;
        }

        public override bool CanEnter(LegacyProgression items) {
            return (items.Mirror || items.Hookshot && items.Hammer) && LegacyWorld.CanEnter("Light World Death Mountain West", items);
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Tower of Hera - Moldorm").Available(items);
        }

    }

}
