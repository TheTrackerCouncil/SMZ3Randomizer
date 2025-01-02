using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyRewardType;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld {

    class LegacyNorthEast : LegacyZ3Region {

        public override string Name => "Dark World North East";
        public override string Area => "Dark World";

        public LegacyNorthEast(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+78, 0x1DE185, LocationType.Regular, "Catfish",
                    items => items.MoonPearl && items.CanLiftLight()),
                new LegacyLocation(this, 256+79, 0x308147, LocationType.Regular, "Pyramid"),
                new LegacyLocation(this, 256+80, 0x1E980, LocationType.Regular, "Pyramid Fairy - Left",
                    items => LegacyWorld.CanAcquireAll(items, CrystalRed) && items.MoonPearl && LegacyWorld.CanEnter("Dark World South", items) &&
                        (items.Hammer || items.Mirror && LegacyWorld.CanAcquire(items, Agahnim))),
                new LegacyLocation(this, 256+81, 0x1E983, LocationType.Regular, "Pyramid Fairy - Right",
                    items => LegacyWorld.CanAcquireAll(items, CrystalRed) && items.MoonPearl && LegacyWorld.CanEnter("Dark World South", items) &&
                        (items.Hammer || items.Mirror && LegacyWorld.CanAcquire(items, Agahnim)))
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return LegacyWorld.CanAcquire(items, Agahnim) || items.MoonPearl && (
                items.Hammer && items.CanLiftLight() ||
                items.CanLiftHeavy() && items.Flippers ||
                items.CanAccessDarkWorldPortal(LegacyConfig) && items.Flippers
            );
        }

    }

}
