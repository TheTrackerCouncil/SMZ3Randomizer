using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyItemType;
using static Randomizer.SMZ3.LegacyWorldState;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacyMiseryMire : LegacyZ3Region, ILegacyReward, ILegacyMedallionAccess {

        public override string Name => "Misery Mire";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;
        public LegacyMedallion LegacyMedallion { get; set; }

        public LegacyMiseryMire(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Weight = 2;
            RegionItems = new[] { KeyMM, BigKeyMM, MapMM, CompassMM };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+169, 0x1EA5E, LocationType.Regular, "Misery Mire - Main Lobby",
                    items => items.BigKeyMM || items.KeyMM >= 1),
                new LegacyLocation(this, 256+170, 0x1EA6A, LocationType.Regular, "Misery Mire - Map Chest",
                    items => items.BigKeyMM || items.KeyMM >= 1),
                new LegacyLocation(this, 256+171, 0x1EA61, LocationType.Regular, "Misery Mire - Bridge Chest"),
                new LegacyLocation(this, 256+172, 0x1E9DA, LocationType.Regular, "Misery Mire - Spike Chest"),
                new LegacyLocation(this, 256+173, 0x1EA64, LocationType.Regular, "Misery Mire - Compass Chest",
                    items => items.CanLightTorches() &&
                        items.KeyMM >= (GetLocation("Misery Mire - Big Key Chest").ItemIs(BigKeyMM, LegacyWorld) ? 2 : 3)),
                new LegacyLocation(this, 256+174, 0x1EA6D, LocationType.Regular, "Misery Mire - Big Key Chest",
                    items => items.CanLightTorches() &&
                        items.KeyMM >= (GetLocation("Misery Mire - Compass Chest").ItemIs(BigKeyMM, LegacyWorld) ? 2 : 3)),
                new LegacyLocation(this, 256+175, 0x1EA67, LocationType.Regular, "Misery Mire - Big Chest",
                    items => items.BigKeyMM),
                new LegacyLocation(this, 256+176, 0x308158, LocationType.Regular, "Misery Mire - Vitreous",
                    items => items.BigKeyMM && items.Lamp && items.Somaria),
            };
        }

        // Need "CanKillManyEnemies" if implementing swordless
        public override bool CanEnter(LegacyProgression items) {
            return LegacyMedallion switch {
                    LegacyMedallion.Bombos => items.Bombos,
                    LegacyMedallion.Ether => items.Ether,
                    _ => items.Quake,
                } && items.Sword &&
                items.MoonPearl && (items.Boots || items.Hookshot) &&
                LegacyWorld.CanEnter("Dark World Mire", items);
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Misery Mire - Vitreous").Available(items);
        }

    }

}
