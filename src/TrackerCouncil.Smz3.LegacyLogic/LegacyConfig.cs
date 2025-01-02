using System.Collections.Generic;
using System.ComponentModel;

namespace Randomizer.SMZ3 {

    [DefaultValue(Normal)]
    public enum LegacyGameMode {
        [Description("Single player")]
        Normal,
        [Description("Multiworld")]
        Multiworld,
    }

    [DefaultValue(Normal)]
    public enum LegacyZ3Logic {
        [Description("Normal")]
        Normal,
        [Description("No major glitches")]
        Nmg,
        [Description("Overworld glitches")]
        Owg,
    }

    [DefaultValue(Normal)]
    public enum LegacySMLogic {
        [Description("Normal")]
        Normal,
        [Description("Hard")]
        Hard,
    }

    [DefaultValue(Randomized)]
    enum SwordLocation {
        [Description("Randomized")]
        Randomized,
        [Description("Early")]
        Early,
        [Description("Uncle assured")]
        Uncle,
    }

    [DefaultValue(Randomized)]
    enum MorphLocation {
        [Description("Randomized")]
        Randomized,
        [Description("Early")]
        Early,
        [Description("Original location")]
        Original,
    }

    [DefaultValue(DefeatBoth)]
    public enum LegacyGoal {
        [Description("Defeat Ganon and Mother Brain")]
        DefeatBoth,
        [Description("Fast Ganon and Defeat Mother Brain")]
        FastGanonDefeatMotherBrain,
        [Description("All Dungeons and Defeat Mother Brain")]
        AllDungeonsDefeatMotherBrain,
    }

    [DefaultValue(None)]
    public enum LegacyKeyShuffle {
        [Description("None")]
        None,
        [Description("Keysanity")]
        Keysanity
    }

    [DefaultValue(SevenCrystals)]
    public enum LegacyOpenTower {
        [Description("Random")]
        Random = -1,
        [Description("No Crystals")]
        NoCrystals = 0,
        [Description("One Crystal")]
        OneCrystal = 1,
        [Description("Two Crystals")]
        TwoCrystals = 2,
        [Description("Three Crystals")]
        ThreeCrystals = 3,
        [Description("Four Crystals")]
        FourCrystals = 4,
        [Description("Five Crystals")]
        FiveCrystals = 5,
        [Description("Six Crystals")]
        SixCrystals = 6,
        [Description("Seven Crystals")]
        SevenCrystals = 7,
    }

    [DefaultValue(SevenCrystals)]
    public enum LegacyGanonVulnerable {
        [Description("Random")]
        Random = -1,
        [Description("No Crystals")]
        NoCrystals = 0,
        [Description("One Crystal")]
        OneCrystal = 1,
        [Description("Two Crystals")]
        TwoCrystals = 2,
        [Description("Three Crystals")]
        ThreeCrystals = 3,
        [Description("Four Crystals")]
        FourCrystals = 4,
        [Description("Five Crystals")]
        FiveCrystals = 5,
        [Description("Six Crystals")]
        SixCrystals = 6,
        [Description("Seven Crystals")]
        SevenCrystals = 7,
    }

    [DefaultValue(FourBosses)]
    public enum LegacyOpenTourian {
        [Description("Random")]
        Random = -1,
        [Description("No Bosses")]
        NoBosses = 0,
        [Description("One Boss")]
        OneBoss = 1,
        [Description("Two Bosses")]
        TwoBosses = 2,
        [Description("Three Bosses")]
        ThreeBosses = 3,
        [Description("Four Bosses")]
        FourBosses = 4,
    }

    public class LegacyConfig {

        public LegacyGameMode LegacyGameMode { get; set; } = LegacyGameMode.Normal;
        public LegacyZ3Logic LegacyZ3Logic { get; set; } = LegacyZ3Logic.Normal;
        public LegacySMLogic LegacySmLogic { get; set; } = LegacySMLogic.Normal;
        internal SwordLocation SwordLocation { get; set; } = SwordLocation.Randomized;
        internal MorphLocation MorphLocation { get; set; } = MorphLocation.Randomized;
        public LegacyGoal LegacyGoal { get; set; } = LegacyGoal.DefeatBoth;
        public LegacyKeyShuffle LegacyKeyShuffle { get; set; } = LegacyKeyShuffle.None;
        public bool Race { get; set; } = false;
        public LegacyOpenTower LegacyOpenTower { get; set; } = LegacyOpenTower.SevenCrystals;
        public LegacyGanonVulnerable LegacyGanonVulnerable { get; set; } = LegacyGanonVulnerable.SevenCrystals;
        public LegacyOpenTourian LegacyOpenTourian { get; set; } = LegacyOpenTourian.FourBosses;
        public Dictionary<LegacyItemType, int> InitialItems = new Dictionary<LegacyItemType, int>();

        public bool SingleWorld => LegacyGameMode == LegacyGameMode.Normal;
        public bool MultiWorld => LegacyGameMode == LegacyGameMode.Multiworld;
        public bool Keysanity => LegacyKeyShuffle != LegacyKeyShuffle.None;

    }

}
