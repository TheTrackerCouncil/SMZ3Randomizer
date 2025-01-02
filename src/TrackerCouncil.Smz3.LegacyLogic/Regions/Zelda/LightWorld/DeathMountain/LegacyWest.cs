using System.Collections.Generic;

namespace Randomizer.SMZ3.Regions.Zelda.LightWorld.DeathMountain {

    class LegacyWest : LegacyZ3Region {

        public override string Name => "Light World Death Mountain West";
        public override string Area => "Death Mountain";

        public LegacyWest(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+0, 0x308016, LocationType.Ether, "Ether Tablet",
                    items => items.Book && items.MasterSword && (items.Mirror || items.Hammer && items.Hookshot)),
                new LegacyLocation(this, 256+1, 0x308140, LocationType.Regular, "Spectacle Rock",
                    items => items.Mirror),
                new LegacyLocation(this, 256+2, 0x308002, LocationType.Regular, "Spectacle Rock Cave"),
                new LegacyLocation(this, 256+3, 0x1EE9FA, LocationType.Regular, "Old Man",
                    items => items.Lamp),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.Flute || items.CanLiftLight() && items.Lamp || items.CanAccessDeathMountainPortal();
        }

    }

}
