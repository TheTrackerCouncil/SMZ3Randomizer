﻿using System.Collections.Generic;
using System.Linq;
using static Randomizer.SMZ3.LegacyItemType;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacyIcePalace : LegacyZ3Region, ILegacyReward {

        public override string Name => "Ice Palace";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;

        public LegacyIcePalace(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Weight = 4;
            RegionItems = new[] { KeyIP, BigKeyIP, MapIP, CompassIP };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+161, 0x1E9D4, LocationType.Regular, "Ice Palace - Compass Chest"),
                new LegacyLocation(this, 256+162, 0x1E9E0, LocationType.Regular, "Ice Palace - Spike Room",
                    items => items.Hookshot || items.KeyIP >= 1 && CanNotWasteKeysBeforeAccessible(items, new[] {
                        GetLocation("Ice Palace - Map Chest"),
                        GetLocation("Ice Palace - Big Key Chest")
                    })),
                new LegacyLocation(this, 256+163, 0x1E9DD, LocationType.Regular, "Ice Palace - Map Chest",
                    items => items.Hammer && items.CanLiftLight() && (
                        items.Hookshot || items.KeyIP >= 1 && CanNotWasteKeysBeforeAccessible(items, new[] {
                            GetLocation("Ice Palace - Spike Room"),
                            GetLocation("Ice Palace - Big Key Chest")
                        })
                    )),
                new LegacyLocation(this, 256+164, 0x1E9A4, LocationType.Regular, "Ice Palace - Big Key Chest",
                    items => items.Hammer && items.CanLiftLight() && (
                        items.Hookshot || items.KeyIP >= 1 && CanNotWasteKeysBeforeAccessible(items, new[] {
                            GetLocation("Ice Palace - Spike Room"),
                            GetLocation("Ice Palace - Map Chest")
                        })
                    )),
                new LegacyLocation(this, 256+165, 0x1E9E3, LocationType.Regular, "Ice Palace - Iced T Room"),
                new LegacyLocation(this, 256+166, 0x1E995, LocationType.Regular, "Ice Palace - Freezor Chest"),
                new LegacyLocation(this, 256+167, 0x1E9AA, LocationType.Regular, "Ice Palace - Big Chest",
                    items => items.BigKeyIP),
                new LegacyLocation(this, 256+168, 0x308157, LocationType.Regular, "Ice Palace - Kholdstare",
                    items => items.BigKeyIP && items.Hammer && items.CanLiftLight() &&
                        items.KeyIP >= (items.Somaria ? 1 : 2)),
            };
        }

        bool CanNotWasteKeysBeforeAccessible(LegacyProgression items, IList<LegacyLocation> locations) {
            return LegacyWorld.ForwardSearch || !items.BigKeyIP || locations.Any(l => l.ItemIs(BigKeyIP, LegacyWorld));
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.MoonPearl && items.Flippers && items.CanLiftHeavy() && items.CanMeltFreezors();
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Ice Palace - Kholdstare").Available(items);
        }

    }

}
