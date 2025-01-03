using System.Collections.Generic;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld.DeathMountain {

    class LegacyWest : LegacyZ3Region {

        public override string Name => "Dark World Death Mountain West";
        public override string Area => "Dark World";

        public LegacyWest(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+64, 0x1EA8B, LocationType.Regular, "Spike Cave",
                    items => items.MoonPearl && items.Hammer && items.CanLiftLight() &&
                        (items.CanExtendMagic() && items.Cape || items.Byrna) &&
                        LegacyWorld.CanEnter("Light World Death Mountain West", items)),
            };
        }

    }

}
