using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyItemType;
using static Randomizer.SMZ3.LegacyWorldState;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacyTurtleRock : LegacyZ3Region, ILegacyReward, ILegacyMedallionAccess {

        public override string Name => "Turtle Rock";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;
        public LegacyMedallion LegacyMedallion { get; set; }

        public LegacyTurtleRock(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Weight = 6;
            RegionItems = new[] { KeyTR, BigKeyTR, MapTR, CompassTR };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+177, 0x1EA22, LocationType.Regular, "Turtle Rock - Compass Chest"),
                new LegacyLocation(this, 256+178, 0x1EA1C, LocationType.Regular, "Turtle Rock - Roller Room - Left",
                    items => items.Firerod),
                new LegacyLocation(this, 256+179, 0x1EA1F, LocationType.Regular, "Turtle Rock - Roller Room - Right",
                    items => items.Firerod),
                new LegacyLocation(this, 256+180, 0x1EA16, LocationType.Regular, "Turtle Rock - Chain Chomps",
                    items => items.KeyTR >= 1),
                new LegacyLocation(this, 256+181, 0x1EA25, LocationType.Regular, "Turtle Rock - Big Key Chest",
                    items => items.KeyTR >=
                        (!LegacyConfig.Keysanity || GetLocation("Turtle Rock - Big Key Chest").ItemIs(BigKeyTR, LegacyWorld) ? 2 :
                            GetLocation("Turtle Rock - Big Key Chest").ItemIs(KeyTR, LegacyWorld) ? 3 : 4))
                    .AlwaysAllow((item, items) => item.Is(KeyTR, LegacyWorld) && items.KeyTR >= 3),
                new LegacyLocation(this, 256+182, 0x1EA19, LocationType.Regular, "Turtle Rock - Big Chest",
                    items => items.BigKeyTR && items.KeyTR >= 2)
                    .Allow((item, items) => item.IsNot(BigKeyTR, LegacyWorld)),
                new LegacyLocation(this, 256+183, 0x1EA34, LocationType.Regular, "Turtle Rock - Crystaroller Room",
                    items => items.BigKeyTR && items.KeyTR >= 2),
                new LegacyLocation(this, 256+184, 0x1EA28, LocationType.Regular, "Turtle Rock - Eye Bridge - Top Right", LaserBridge),
                new LegacyLocation(this, 256+185, 0x1EA2B, LocationType.Regular, "Turtle Rock - Eye Bridge - Top Left", LaserBridge),
                new LegacyLocation(this, 256+186, 0x1EA2E, LocationType.Regular, "Turtle Rock - Eye Bridge - Bottom Right", LaserBridge),
                new LegacyLocation(this, 256+187, 0x1EA31, LocationType.Regular, "Turtle Rock - Eye Bridge - Bottom Left", LaserBridge),
                new LegacyLocation(this, 256+188, 0x308159, LocationType.Regular, "Turtle Rock - Trinexx",
                    items => items.BigKeyTR && items.KeyTR >= 4 && items.Lamp && CanBeatBoss(items)),
            };
        }

        bool LaserBridge(LegacyProgression items) {
            return items.BigKeyTR && items.KeyTR >= 3 && items.Lamp && (items.Cape || items.Byrna || items.CanBlockLasers);
        }

        bool CanBeatBoss(LegacyProgression items) {
            return items.Firerod && items.Icerod;
        }

        public override bool CanEnter(LegacyProgression items) {
            if (LegacyMedallion == LegacyMedallion.Unknown && (!items.Bombos || !items.Ether || !items.Quake))
            {
                return false;
            }

            return LegacyMedallion switch {
                    LegacyMedallion.Bombos => items.Bombos,
                    LegacyMedallion.Ether => items.Ether,
                    _ => items.Quake,
                } && items.Sword &&
                items.MoonPearl && items.CanLiftHeavy() && items.Hammer && items.Somaria &&
                LegacyWorld.CanEnter("Light World Death Mountain East", items);
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Turtle Rock - Trinexx").Available(items);
        }

    }

}
