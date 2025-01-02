using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyItemType;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacyEasternPalace : LegacyZ3Region, ILegacyReward {

        public override string Name => "Eastern Palace";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;

        public LegacyEasternPalace(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            RegionItems = new[] { BigKeyEP, MapEP, CompassEP };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+103, 0x1E9B3, LocationType.Regular, "Eastern Palace - Cannonball Chest"),
                new LegacyLocation(this, 256+104, 0x1E9F5, LocationType.Regular, "Eastern Palace - Map Chest"),
                new LegacyLocation(this, 256+105, 0x1E977, LocationType.Regular, "Eastern Palace - Compass Chest"),
                new LegacyLocation(this, 256+106, 0x1E97D, LocationType.Regular, "Eastern Palace - Big Chest",
                    items => items.BigKeyEP),
                new LegacyLocation(this, 256+107, 0x1E9B9, LocationType.Regular, "Eastern Palace - Big Key Chest",
                    items => items.Lamp),
                new LegacyLocation(this, 256+108, 0x308150, LocationType.Regular, "Eastern Palace - Armos Knights",
                    items => items.BigKeyEP && items.Bow && items.Lamp),
            };
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Eastern Palace - Armos Knights").Available(items);
        }

    }

}
