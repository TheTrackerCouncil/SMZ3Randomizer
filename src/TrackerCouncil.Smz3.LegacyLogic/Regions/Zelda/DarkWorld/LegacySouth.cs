using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyRewardType;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld {

    class LegacySouth : LegacyZ3Region {

        public override string Name => "Dark World South";
        public override string Area => "Dark World";

        public LegacySouth(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+82, 0x308148, LocationType.Regular, "Digging Game"),
                new LegacyLocation(this, 256+83, 0x6B0C7, LocationType.Regular, "Stumpy"),
                new LegacyLocation(this, 256+84, 0x1EB1E, LocationType.Regular, "Hype Cave - Top"),
                new LegacyLocation(this, 256+85, 0x1EB21, LocationType.Regular, "Hype Cave - Middle Right"),
                new LegacyLocation(this, 256+86, 0x1EB24, LocationType.Regular, "Hype Cave - Middle Left"),
                new LegacyLocation(this, 256+87, 0x1EB27, LocationType.Regular, "Hype Cave - Bottom"),
                new LegacyLocation(this, 256+88, 0x308011, LocationType.Regular, "Hype Cave - NPC"),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.MoonPearl && ((
                    LegacyWorld.CanAcquire(items, Agahnim) ||
                    items.CanAccessDarkWorldPortal(LegacyConfig) && items.Flippers
                ) && (items.Hammer || items.Hookshot && (items.Flippers || items.CanLiftLight())) ||
                items.Hammer && items.CanLiftLight() ||
                items.CanLiftHeavy()
            );
        }

    }

}
