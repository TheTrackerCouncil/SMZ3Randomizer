using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyItemType;

namespace Randomizer.SMZ3.Regions.Zelda {

    class LegacyCastleTower : LegacyZ3Region, ILegacyReward {

        public override string Name => "Castle Tower";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.Agahnim;

        public LegacyCastleTower(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            RegionItems = new[] { KeyCT };

            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 256+101, 0x1EAB5, LocationType.Regular, "Castle Tower - Foyer"),
                new LegacyLocation(this, 256+102, 0x1EAB2, LocationType.Regular, "Castle Tower - Dark Maze",
                    items => items.Lamp && items.KeyCT >= 1),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return items.CanKillManyEnemies() && (items.Cape || items.MasterSword);
        }

        public bool CanComplete(LegacyProgression items) {
            return CanEnter(items) && items.Lamp && items.KeyCT >= 2 && items.Sword;
        }

    }

}
