using System.ComponentModel;

namespace Randomizer.SMZ3
{
    [DefaultValue(Normal)]
    public enum GameMode
    {
        [Description("Single player")]
        Normal,

        [Description("Multiworld")]
        Multiworld,
    }

    [DefaultValue(Normal)]
    public enum Z3Logic
    {
        [Description("Normal")]
        Normal,

        [Description("No major glitches")]
        Nmg,

        [Description("Overworld glitches")]
        Owg,
    }

    [DefaultValue(Normal)]
    public enum SMLogic
    {
        [Description("Normal")]
        Normal,

        [Description("Hard")]
        Hard,
    }

    [DefaultValue(Randomized)]
    public enum ItemPlacement
    {
        [Description("Randomized")]
        Randomized,

        [Description("Early")]
        Early,

        [Description("Original location")]
        Original,
    }

    [DefaultValue(Any)]
    public enum ItemPool
    {
        Any = 0,

        [Description("Progression items")]
        Progression,

        [Description("Non-junk items")]
        NonJunk,

        [Description("Junk items")]
        Junk,
    }

    [DefaultValue(DefeatBoth)]
    public enum Goal
    {
        [Description("Defeat Ganon and Mother Brain")]
        DefeatBoth,
    }

    [DefaultValue(None)]
    public enum KeyShuffle
    {
        [Description("None")]
        None,

        [Description("Keysanity")]
        Keysanity
    }

    public enum GanonInvincible
    {
        [Description("Never")]
        Never,

        [Description("Before Crystals")]
        BeforeCrystals,

        [Description("Before All Dungeons")]
        BeforeAllDungeons,

        [Description("Always")]
        Always,
    }

    /// <summary>
    /// Specifies how dungeon music in A Link to the Past is selected.
    /// </summary>
    public enum MusicShuffleMode
    {
        /// <summary>
        /// Specifies music should not be shuffled.
        /// </summary>
        /// <remarks>
        /// Dungeons play the light/dark world dungeon theme depending on the
        /// type of crystal. In extended mode, dungeons have their own theme.
        /// </remarks>
        [Description("Dungeons play the normal dungeon themes")]
        Default,

        /// <summary>
        /// Specifies music should be shuffled between dungeons.
        /// </summary>
        /// <remarks>
        /// Dungeons play the light/dark world theme randomly. In extended mode,
        /// all dungeon themes are shuffled.
        /// </remarks>
        [Description("Dungeons can play any dungeon theme")]
        ShuffleDungeons,

        /// <summary>
        /// Specifies dungeon music can be replaced by any track in the game.
        /// </summary>
        /// <remarks>
        /// Dungeons can play any track from the game soundtrack. In extended
        /// mode, all extended soundtracks are included.
        /// </remarks>
        [Description("Dungeons can play any track")]
        ShuffleAll
    }

    public class Config
    {
        public GameMode GameMode { get; set; } = GameMode.Normal;
        public Z3Logic Z3Logic { get; set; } = Z3Logic.Normal;
        public SMLogic SMLogic { get; set; } = SMLogic.Normal;
        public ItemPlacement SwordLocation { get; set; } = ItemPlacement.Randomized;
        public ItemPlacement MorphLocation { get; set; } = ItemPlacement.Randomized;
        public ItemPlacement MorphBombsLocation { get; set; } = ItemPlacement.Randomized;
        public ItemPool ShaktoolItemPool { get; set; } = ItemPool.Any;
        public Goal Goal { get; set; } = Goal.DefeatBoth;
        public KeyShuffle KeyShuffle { get; set; } = KeyShuffle.None;
        public bool Race { get; set; } = false;
        public GanonInvincible GanonInvincible { get; set; } = GanonInvincible.BeforeCrystals;
        public bool ExtendedMsuSupport { get; set; } = false;
        public MusicShuffleMode ShuffleDungeonMusic { get; set; } = MusicShuffleMode.Default;

        public bool SingleWorld => GameMode == GameMode.Normal;
        public bool MultiWorld => GameMode == GameMode.Multiworld;
        public bool Keysanity => KeyShuffle != KeyShuffle.None;
    }
}
