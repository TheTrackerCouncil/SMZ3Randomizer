using System.Collections.Generic;

namespace Randomizer.SMZ3.Regions.Zelda.LightWorld {

    class LegacySouth : LegacyZ3Region {

        public override string Name => "Light World South";
        public override string Area => "Light World";

        public LegacySouth(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            var sphereOne = -10;
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+45, 0x308142, LocationType.Regular, "Maze Race").Weighted(sphereOne),
                new LegacyLocation(this, 256+240, 0x308012, LocationType.Regular, "Library",
                    items => items.Boots),
                new LegacyLocation(this, 256+241, 0x30814A, LocationType.Regular, "Flute Spot",
                    items => items.Shovel),
                new LegacyLocation(this, 256+242, 0x308003, LocationType.Regular, "South of Grove",
                    items => items.Mirror && LegacyWorld.CanEnter("Dark World South", items)),
                new LegacyLocation(this, 256+243, 0x1E9BC, LocationType.Regular, "Link's House").Weighted(sphereOne),
                new LegacyLocation(this, 256+244, 0x1E9F2, LocationType.Regular, "Aginah's Cave").Weighted(sphereOne),
                new LegacyLocation(this, 256+51, 0x1EB42, LocationType.Regular, "Mini Moldorm Cave - Far Left").Weighted(sphereOne),
                new LegacyLocation(this, 256+52, 0x1EB45, LocationType.Regular, "Mini Moldorm Cave - Left").Weighted(sphereOne),
                new LegacyLocation(this, 256+53, 0x308010, LocationType.Regular, "Mini Moldorm Cave - NPC").Weighted(sphereOne),
                new LegacyLocation(this, 256+54, 0x1EB48, LocationType.Regular, "Mini Moldorm Cave - Right").Weighted(sphereOne),
                new LegacyLocation(this, 256+251, 0x1EB4B, LocationType.Regular, "Mini Moldorm Cave - Far Right").Weighted(sphereOne),
                new LegacyLocation(this, 256+252, 0x308143, LocationType.Regular, "Desert Ledge",
                    items => LegacyWorld.CanEnter("Desert Palace", items)),
                new LegacyLocation(this, 256+253, 0x308005, LocationType.Regular, "Checkerboard Cave",
                    items => items.Mirror && (
                        items.Flute && items.CanLiftHeavy() ||
                        items.CanAccessMiseryMirePortal(LegacyConfig)
                    ) && items.CanLiftLight()),
                new LegacyLocation(this, 256+58, 0x308017, LocationType.Bombos, "Bombos Tablet",
                    items => items.Book && items.MasterSword && items.Mirror && LegacyWorld.CanEnter("Dark World South", items)),
                new LegacyLocation(this, 256+59, 0x1E98C, LocationType.Regular, "Floodgate Chest").Weighted(sphereOne),
                new LegacyLocation(this, 256+60, 0x308145, LocationType.Regular, "Sunken Treasure").Weighted(sphereOne),
                new LegacyLocation(this, 256+61, 0x308144, LocationType.Regular, "Lake Hylia Island",
                    items => items.Flippers && items.MoonPearl && items.Mirror && (
                        LegacyWorld.CanEnter("Dark World South", items) ||
                        LegacyWorld.CanEnter("Dark World North East", items))),
                new LegacyLocation(this, 256+62, 0x6BE7D, LocationType.Regular, "Hobo",
                    items => items.Flippers),
                new LegacyLocation(this, 256+63, 0x1EB4E, LocationType.Regular, "Ice Rod Cave").Weighted(sphereOne),
            };

        }

    }

}
