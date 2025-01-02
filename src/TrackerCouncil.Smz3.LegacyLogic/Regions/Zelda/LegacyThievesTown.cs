using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyItemType;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacyThievesTown : LegacyZ3Region, ILegacyReward {

        public override string Name => "Thieves' Town";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;

        public LegacyThievesTown(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            RegionItems = new[] { KeyTT, BigKeyTT, MapTT, CompassTT };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+153, 0x1EA01, LocationType.Regular, "Thieves' Town - Map Chest"),
                new LegacyLocation(this, 256+154, 0x1EA0A, LocationType.Regular, "Thieves' Town - Ambush Chest"),
                new LegacyLocation(this, 256+155, 0x1EA07, LocationType.Regular, "Thieves' Town - Compass Chest"),
                new LegacyLocation(this, 256+156, 0x1EA04, LocationType.Regular, "Thieves' Town - Big Key Chest"),
                new LegacyLocation(this, 256+157, 0x1EA0D, LocationType.Regular, "Thieves' Town - Attic",
                    items => items.BigKeyTT && items.KeyTT),
                new LegacyLocation(this, 256+158, 0x1EA13, LocationType.Regular, "Thieves' Town - Blind's Cell",
                    items => items.BigKeyTT),
                new LegacyLocation(this, 256+159, 0x1EA10, LocationType.Regular, "Thieves' Town - Big Chest",
                    items => items.BigKeyTT && items.Hammer &&
                        (GetLocation("Thieves' Town - Big Chest").ItemIs(KeyTT, LegacyWorld) || items.KeyTT))
                    .AlwaysAllow((item, items) => item.Is(KeyTT, LegacyWorld) && items.Hammer),
                new LegacyLocation(this, 256+160, 0x308156, LocationType.Regular, "Thieves' Town - Blind",
                    items => items.BigKeyTT && items.KeyTT && CanBeatBoss(items)),
            };
        }

        private bool CanBeatBoss(LegacyProgression items) {
            return items.Sword || items.Hammer ||
                items.Somaria || items.Byrna;
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.MoonPearl && LegacyWorld.CanEnter("Dark World North West", items);
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Thieves' Town - Blind").Available(items);
        }

    }

}
