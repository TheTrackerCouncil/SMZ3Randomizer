using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyRewardType;

namespace Randomizer.SMZ3.Regions.Zelda.DarkWorld {

    class LegacyNorthWest : LegacyZ3Region {

        public override string Name => "Dark World North West";
        public override string Area => "Dark World";

        public LegacyNorthWest(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+71, 0x308146, LocationType.Regular, "Bumper Cave",
                    items => items.CanLiftLight() && items.Cape),
                new LegacyLocation(this, 256+72, 0x1EDA8, LocationType.Regular, "Chest Game"),
                new LegacyLocation(this, 256+73, 0x1E9EF, LocationType.Regular, "C-Shaped House"),
                new LegacyLocation(this, 256+74, 0x1E9EC, LocationType.Regular, "Brewery"),
                new LegacyLocation(this, 256+75, 0x308006, LocationType.Regular, "Hammer Pegs",
                    items => items.CanLiftHeavy() && items.Hammer),
                new LegacyLocation(this, 256+76, 0x30802A, LocationType.Regular, "Blacksmith",
                    items => items.CanLiftHeavy()),
                new LegacyLocation(this, 256+77, 0x6BD68, LocationType.Regular, "Purple Chest",
                    items => items.CanLiftHeavy()),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.MoonPearl && ((
                    LegacyWorld.CanAcquire(items, Agahnim) ||
                    items.CanAccessDarkWorldPortal(LegacyConfig) && items.Flippers
                ) && items.Hookshot && (items.Flippers || items.CanLiftLight() || items.Hammer) ||
                items.Hammer && items.CanLiftLight() ||
                items.CanLiftHeavy()
            );
        }

    }

}
