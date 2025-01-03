using System.Collections.Generic;
using System.Linq;
using static Randomizer.SMZ3.LegacyItemType;
using static Randomizer.SMZ3.LegacyRewardType;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacyGanonsTower : LegacyZ3Region {

        public override string Name => "Ganon's Tower";

        public LegacyGanonsTower(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            RegionItems = new[] { KeyGT, BigKeyGT, MapGT, CompassGT };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+189, 0x308161, LocationType.Regular, "Ganon's Tower - Bob's Torch",
                    items => items.Boots),
                new LegacyLocation(this, 256+190, 0x1EAB8, LocationType.Regular, "Ganon's Tower - DMs Room - Top Left",
                    items => items.Hammer && items.Hookshot),
                new LegacyLocation(this, 256+191, 0x1EABB, LocationType.Regular, "Ganon's Tower - DMs Room - Top Right",
                    items => items.Hammer && items.Hookshot),
                new LegacyLocation(this, 256+192, 0x1EABE, LocationType.Regular, "Ganon's Tower - DMs Room - Bottom Left",
                    items => items.Hammer && items.Hookshot),
                new LegacyLocation(this, 256+193, 0x1EAC1, LocationType.Regular, "Ganon's Tower - DMs Room - Bottom Right",
                    items => items.Hammer && items.Hookshot),
                new LegacyLocation(this, 256+194, 0x1EAD3, LocationType.Regular, "Ganon's Tower - Map Chest",
                    items => items.Hammer && (items.Hookshot || items.Boots) && items.KeyGT >=
                        (new[] { BigKeyGT, KeyGT }.Any(type => GetLocation("Ganon's Tower - Map Chest").ItemIs(type, LegacyWorld)) ? 3 : 4))
                    .AlwaysAllow((item, items) => item.Is(KeyGT, LegacyWorld) && items.KeyGT >= 3),
                new LegacyLocation(this, 256+195, 0x1EAD0, LocationType.Regular, "Ganon's Tower - Firesnake Room",
                    items => items.Hammer && items.Hookshot && items.KeyGT >= (new[] {
                            GetLocation("Ganon's Tower - Randomizer Room - Top Right"),
                            GetLocation("Ganon's Tower - Randomizer Room - Top Left"),
                            GetLocation("Ganon's Tower - Randomizer Room - Bottom Left"),
                            GetLocation("Ganon's Tower - Randomizer Room - Bottom Right")
                        }.Any(l => l.ItemIs(BigKeyGT, LegacyWorld)) ||
                        GetLocation("Ganon's Tower - Firesnake Room").ItemIs(KeyGT, LegacyWorld) ? 2 : 3)),
                new LegacyLocation(this, 256+230, 0x1EAC4, LocationType.Regular, "Ganon's Tower - Randomizer Room - Top Left",
                    items => LeftSide(items, new[] {
                        GetLocation("Ganon's Tower - Randomizer Room - Top Right"),
                        GetLocation("Ganon's Tower - Randomizer Room - Bottom Left"),
                        GetLocation("Ganon's Tower - Randomizer Room - Bottom Right")
                    })),
                new LegacyLocation(this, 256+231, 0x1EAC7, LocationType.Regular, "Ganon's Tower - Randomizer Room - Top Right",
                    items => LeftSide(items, new[] {
                        GetLocation("Ganon's Tower - Randomizer Room - Top Left"),
                        GetLocation("Ganon's Tower - Randomizer Room - Bottom Left"),
                        GetLocation("Ganon's Tower - Randomizer Room - Bottom Right")
                    })),
                new LegacyLocation(this, 256+232, 0x1EACA, LocationType.Regular, "Ganon's Tower - Randomizer Room - Bottom Left",
                    items => LeftSide(items, new[] {
                        GetLocation("Ganon's Tower - Randomizer Room - Top Right"),
                        GetLocation("Ganon's Tower - Randomizer Room - Top Left"),
                        GetLocation("Ganon's Tower - Randomizer Room - Bottom Right")
                    })),
                new LegacyLocation(this, 256+233, 0x1EACD, LocationType.Regular, "Ganon's Tower - Randomizer Room - Bottom Right",
                    items => LeftSide(items, new[] {
                        GetLocation("Ganon's Tower - Randomizer Room - Top Right"),
                        GetLocation("Ganon's Tower - Randomizer Room - Top Left"),
                        GetLocation("Ganon's Tower - Randomizer Room - Bottom Left")
                    })),
                new LegacyLocation(this, 256+234, 0x1EAD9, LocationType.Regular, "Ganon's Tower - Hope Room - Left"),
                new LegacyLocation(this, 256+235, 0x1EADC, LocationType.Regular, "Ganon's Tower - Hope Room - Right"),
                new LegacyLocation(this, 256+236, 0x1EAE2, LocationType.Regular, "Ganon's Tower - Tile Room",
                    items => items.Somaria),
                new LegacyLocation(this, 256+203, 0x1EAE5, LocationType.Regular, "Ganon's Tower - Compass Room - Top Left",
                    items => RightSide(items, new[] {
                        GetLocation("Ganon's Tower - Compass Room - Top Right"),
                        GetLocation("Ganon's Tower - Compass Room - Bottom Left"),
                        GetLocation("Ganon's Tower - Compass Room - Bottom Right")
                    })),
                new LegacyLocation(this, 256+204, 0x1EAE8, LocationType.Regular, "Ganon's Tower - Compass Room - Top Right",
                    items => RightSide(items, new[] {
                        GetLocation("Ganon's Tower - Compass Room - Top Left"),
                        GetLocation("Ganon's Tower - Compass Room - Bottom Left"),
                        GetLocation("Ganon's Tower - Compass Room - Bottom Right")
                    })),
                new LegacyLocation(this, 256+205, 0x1EAEB, LocationType.Regular, "Ganon's Tower - Compass Room - Bottom Left",
                    items => RightSide(items, new[] {
                        GetLocation("Ganon's Tower - Compass Room - Top Right"),
                        GetLocation("Ganon's Tower - Compass Room - Top Left"),
                        GetLocation("Ganon's Tower - Compass Room - Bottom Right")
                    })),
                new LegacyLocation(this, 256+206, 0x1EAEE, LocationType.Regular, "Ganon's Tower - Compass Room - Bottom Right",
                    items => RightSide(items, new[] {
                        GetLocation("Ganon's Tower - Compass Room - Top Right"),
                        GetLocation("Ganon's Tower - Compass Room - Top Left"),
                        GetLocation("Ganon's Tower - Compass Room - Bottom Left")
                    })),
                new LegacyLocation(this, 256+207, 0x1EADF, LocationType.Regular, "Ganon's Tower - Bob's Chest",
                    items => items.KeyGT >= 3 && (
                        items.Hammer && items.Hookshot ||
                        items.Somaria && items.Firerod)),
                new LegacyLocation(this, 256+208, 0x1EAD6, LocationType.Regular, "Ganon's Tower - Big Chest",
                    items => items.BigKeyGT && items.KeyGT >= 3 && (
                        items.Hammer && items.Hookshot ||
                        items.Somaria && items.Firerod))
                    .Allow((item, items) => item.IsNot(BigKeyGT, LegacyWorld)),
                new LegacyLocation(this, 256+209, 0x1EAF1, LocationType.Regular, "Ganon's Tower - Big Key Chest", BigKeyRoom),
                new LegacyLocation(this, 256+210, 0x1EAF4, LocationType.Regular, "Ganon's Tower - Big Key Room - Left", BigKeyRoom),
                new LegacyLocation(this, 256+211, 0x1EAF7, LocationType.Regular, "Ganon's Tower - Big Key Room - Right", BigKeyRoom),
                new LegacyLocation(this, 256+212, 0x1EAFD, LocationType.Regular, "Ganon's Tower - Mini Helmasaur Room - Left", TowerAscend)
                    .Allow((item, items) => item.IsNot(BigKeyGT, LegacyWorld)),
                new LegacyLocation(this, 256+213, 0x1EB00, LocationType.Regular, "Ganon's Tower - Mini Helmasaur Room - Right", TowerAscend)
                    .Allow((item, items) => item.IsNot(BigKeyGT, LegacyWorld)),
                new LegacyLocation(this, 256+214, 0x1EB03, LocationType.Regular, "Ganon's Tower - Pre-Moldorm Chest", TowerAscend)
                    .Allow((item, items) => item.IsNot(BigKeyGT, LegacyWorld)),
                new LegacyLocation(this, 256+215, 0x1EB06, LocationType.Regular, "Ganon's Tower - Moldorm Chest",
                    items => items.BigKeyGT && items.KeyGT >= 4 &&
                        items.Bow && items.CanLightTorches() &&
                        CanBeatMoldorm(items) && items.Hookshot)
                    .Allow((item, items) => new[] { KeyGT, BigKeyGT }.All(type => item.IsNot(type, LegacyWorld))),
            };
        }

        bool LeftSide(LegacyProgression items, IList<LegacyLocation> locations) {
            return items.Hammer && items.Hookshot && items.KeyGT >= (locations.Any(l => l.ItemIs(BigKeyGT, LegacyWorld)) ? 3 : 4);
        }

        bool RightSide(LegacyProgression items, IList<LegacyLocation> locations) {
            return items.Somaria && items.Firerod && items.KeyGT >= (locations.Any(l => l.ItemIs(BigKeyGT, LegacyWorld)) ? 3 : 4);
        }

        bool BigKeyRoom(LegacyProgression items) {
            return items.KeyGT >= 3 && (
                items.Hammer && items.Hookshot ||
                items.Firerod && items.Somaria
            ) && CanBeatArmos(items);
        }

        bool TowerAscend(LegacyProgression items) {
            return items.BigKeyGT && items.KeyGT >= 3 && items.Bow && items.CanLightTorches();
        }

        bool CanBeatArmos(LegacyProgression items) {
            return items.Sword || items.Hammer || items.Bow ||
                items.CanExtendMagic(2) && (items.Somaria || items.Byrna) ||
                items.CanExtendMagic(4) && (items.Firerod || items.Icerod);
        }

        bool CanBeatMoldorm(LegacyProgression items) {
            return items.Sword || items.Hammer;
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.MoonPearl && LegacyWorld.CanEnter("Dark World Death Mountain East", items) &&
                LegacyWorld.CanAcquireAtLeast(LegacyWorld.TowerCrystals, items, AnyCrystal) &&
                LegacyWorld.CanAcquireAtLeast((LegacyWorld.TourianBossTokens * LegacyWorld.TowerCrystals) / 7, items, AnyBossToken);
        }

        public override bool CanFill(LegacyItem legacyItem, LegacyProgression items) {
            if (LegacyConfig.MultiWorld) {
                if (legacyItem.LegacyWorld != LegacyWorld || legacyItem.Progression) {
                    return false;
                }

                if (LegacyConfig.Keysanity && !((legacyItem.Type == BigKeyGT || legacyItem.Type == KeyGT) && legacyItem.LegacyWorld == LegacyWorld) && (legacyItem.IsKey || legacyItem.IsBigKey || legacyItem.IsKeycard)) {
                    return false;
                }
            }

            return base.CanFill(legacyItem, items);
        }

    }

}
