using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyItemType;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacySkullWoods : LegacyZ3Region, ILegacyReward {

        public override string Name => "Skull Woods";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;

        public LegacySkullWoods(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            RegionItems = new[] { KeySW, BigKeySW, MapSW, CompassSW };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+145, 0x1E9A1, LocationType.Regular, "Skull Woods - Pot Prison"),
                new LegacyLocation(this, 256+146, 0x1E992, LocationType.Regular, "Skull Woods - Compass Chest"),
                new LegacyLocation(this, 256+147, 0x1E998, LocationType.Regular, "Skull Woods - Big Chest",
                    items => items.BigKeySW)
                    .AlwaysAllow((item, items) => item.Is(BigKeySW, LegacyWorld)),
                new LegacyLocation(this, 256+148, 0x1E99B, LocationType.Regular, "Skull Woods - Map Chest"),
                new LegacyLocation(this, 256+149, 0x1E9C8, LocationType.Regular, "Skull Woods - Pinball Room")
                    .Allow((item, items) => item.Is(KeySW, LegacyWorld)),
                new LegacyLocation(this, 256+150, 0x1E99E, LocationType.Regular, "Skull Woods - Big Key Chest"),
                new LegacyLocation(this, 256+151, 0x1E9FE, LocationType.Regular, "Skull Woods - Bridge Room",
                    items => items.Firerod),
                new LegacyLocation(this, 256+152, 0x308155, LocationType.Regular, "Skull Woods - Mothula",
                    items => items.Firerod && items.Sword && items.KeySW >= 3),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.MoonPearl && LegacyWorld.CanEnter("Dark World North West", items);
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Skull Woods - Mothula").Available(items);
        }

    }

}
