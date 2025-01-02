using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Randomizer.SMZ3.LegacyWorldState;

namespace Randomizer.SMZ3 {

    [Flags]
    public enum LegacyRewardType {
        [Description("None")]
        None,
        [Description("Agahnim")]
        Agahnim = 1 << 0,
        [Description("Green Pendant")]
        PendantGreen = 1 << 1,
        [Description("Blue/Red Pendant")]
        PendantNonGreen = 1 << 2,
        [Description("Blue Crystal")]
        CrystalBlue = 1 << 3,
        [Description("Red Crystal")]
        CrystalRed = 1 << 4,
        [Description("Kraid Boss Token")]
        BossTokenKraid = 1 << 5,
        [Description("Phantoon Boss Token")]
        BossTokenPhantoon = 1 << 6,
        [Description("Draygon Boss Token")]
        BossTokenDraygon = 1 << 7,
        [Description("Ridley Boss Token")]
        BossTokenRidley = 1 << 8,

        AnyPendant = PendantGreen | PendantNonGreen,
        AnyCrystal = CrystalBlue | CrystalRed,
        AnyBossToken = BossTokenKraid | BossTokenPhantoon
            | BossTokenDraygon | BossTokenRidley,
    }

    interface ILegacyReward {
        LegacyRewardType LegacyReward { get; set; }
        bool CanComplete(LegacyProgression items);
    }

    interface ILegacyMedallionAccess {
        LegacyMedallion LegacyMedallion { get; set; }
    }

    abstract class LegacySMRegion : LegacyRegion {
        public LegacySMLogic Logic => LegacyConfig.LegacySmLogic;
        public LegacySMRegion(LegacyWorld legacyWorld, LegacyConfig legacyConfig) : base(legacyWorld, legacyConfig) { }
    }

    abstract class LegacyZ3Region : LegacyRegion {
        public LegacyZ3Region(LegacyWorld legacyWorld, LegacyConfig legacyConfig)
            : base(legacyWorld, legacyConfig) { }
    }

    abstract class LegacyRegion {

        public virtual string Name { get; }
        public virtual string Area => Name;

        public List<LegacyLocation> Locations { get; set; }
        public LegacyWorld LegacyWorld { get; set; }
        public int Weight { get; set; } = 0;

        protected LegacyConfig LegacyConfig { get; set; }
        protected IList<LegacyItemType> RegionItems { get; set; } = new List<LegacyItemType>();

        private Dictionary<string, LegacyLocation> locationLookup { get; set; }
        public LegacyLocation GetLocation(string name) => locationLookup[name];

        public LegacyRegion(LegacyWorld legacyWorld, LegacyConfig legacyConfig) {
            LegacyConfig = legacyConfig;
            LegacyWorld = legacyWorld;
            locationLookup = new Dictionary<string, LegacyLocation>();
        }

        public void GenerateLocationLookup() {
            locationLookup = Locations.ToDictionary(l => l.Name, l => l);
        }

        public bool IsRegionItem(LegacyItem legacyItem) {
            return RegionItems.Contains(legacyItem.Type);
        }

        public virtual bool CanFill(LegacyItem legacyItem, LegacyProgression items) {
            return LegacyConfig.Keysanity || !legacyItem.IsDungeonItem || IsRegionItem(legacyItem);
        }

        public virtual bool CanEnter(LegacyProgression items) {
            return true;
        }

    }

}
