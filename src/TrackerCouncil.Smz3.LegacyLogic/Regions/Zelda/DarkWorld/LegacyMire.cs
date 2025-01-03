using System.Collections.Generic;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld {

    class LegacyMire : LegacyZ3Region {

        public override string Name => "Dark World Mire";
        public override string Area => "Dark World";

        public LegacyMire(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+89, 0x1EA73, LocationType.Regular, "Mire Shed - Left",
                    items => items.MoonPearl),
                new LegacyLocation(this, 256+90, 0x1EA76, LocationType.Regular, "Mire Shed - Right",
                    items => items.MoonPearl),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.Flute && items.CanLiftHeavy() || items.CanAccessMiseryMirePortal(LegacyConfig);
        }

    }

}
