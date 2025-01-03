using System.Collections.Generic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar {

    class LegacyKraid : LegacySMRegion, ILegacyReward {

        public override string Name => "Brinstar Kraid";
        public override string Area => "Brinstar";

        public LegacyRewardType LegacyReward { get; set; } = LegacyRewardType.None;

        public LegacyKraid(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) {
            Locations = new List<LegacyLocation> {
                new LegacyLocation(this, 43, 0x8F899C, LocationType.Hidden, "Energy Tank, Kraid",
                    items => items.CardBrinstarBoss),
                new LegacyLocation(this, 48, 0x8F8ACA, LocationType.Chozo, "Varia Suit",
                    items => items.CardBrinstarBoss),
                new LegacyLocation(this, 44, 0x8F89EC, LocationType.Hidden, "Missile (Kraid)", Logic switch {
                    _ => new Requirement(items => items.CanUsePowerBombs())
                }),
            };
        }

        public override bool CanEnter(LegacyProgression items) {
            return (items.CanDestroyBombWalls() || items.SpeedBooster || items.CanAccessNorfairUpperPortal()) &&
                items.Super && items.CanPassBombPassages();
        }

        public bool CanComplete(LegacyProgression items) {
            return GetLocation("Varia Suit").Available(items);
        }

    }

}
