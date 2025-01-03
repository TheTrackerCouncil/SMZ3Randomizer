using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyItemType;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacyDesertPalace : LegacyZ3Region, ILegacyReward {

        public override string Name => "Desert Palace";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;

        public LegacyDesertPalace(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            RegionItems = new[] { KeyDP, BigKeyDP, MapDP, CompassDP };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+109, 0x1E98F, LocationType.Regular, "Desert Palace - Big Chest",
                    items => items.BigKeyDP),
                new LegacyLocation(this, 256+110, 0x308160, LocationType.Regular, "Desert Palace - Torch",
                    items => items.Boots),
                new LegacyLocation(this, 256+111, 0x1E9B6, LocationType.Regular, "Desert Palace - Map Chest"),
                new LegacyLocation(this, 256+112, 0x1E9C2, LocationType.Regular, "Desert Palace - Big Key Chest",
                    items => items.KeyDP),
                new LegacyLocation(this, 256+113, 0x1E9CB, LocationType.Regular, "Desert Palace - Compass Chest",
                    items => items.KeyDP),
                new LegacyLocation(this, 256+114, 0x308151, LocationType.Regular, "Desert Palace - Lanmolas",
                    items => (
                        items.CanLiftLight() ||
                        items.CanAccessMiseryMirePortal(LegacyConfig) && items.Mirror
                    ) && items.BigKeyDP && items.KeyDP && items.CanLightTorches() && CanBeatBoss(items)),
            };
        }

        private bool CanBeatBoss(LegacyProgression items) {
            return items.Sword || items.Hammer || items.Bow ||
                items.Firerod || items.Icerod ||
                items.Byrna || items.Somaria;
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.Book ||
                items.Mirror && items.CanLiftHeavy() && items.Flute ||
                items.CanAccessMiseryMirePortal(LegacyConfig) && items.Mirror;
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Desert Palace - Lanmolas").Available(items);
        }

    }

}
