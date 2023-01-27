using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Newtonsoft.Json;
using Randomizer.Data.Logic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Multiplayer;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Randomizer.Data.Options
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
        [Description("Any")]
        Any = 0,

        [Description("Progression items")]
        Progression,

        /*[Description("Non-junk items")]
        NonJunk,*/

        [Description("Junk items")]
        Junk,
    }

    [DefaultValue(DefeatBoth)]
    public enum Goal
    {
        [Description("Defeat Ganon and Mother Brain")]
        DefeatBoth,
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

    public enum HeartColor
    {
        Red = 0,
        Yellow,
        Green,
        Blue
    }

    public enum LowHealthBeepSpeed
    {
        Normal = 0,
        Double,
        Half,
        Quarter,
        Off,
    }

    public enum MenuSpeed
    {
        Default = 0,
        Fast,
        Instant,
        Slow
    }

    public class Config
    {
        private static readonly JsonSerializerOptions s_options = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public GameMode GameMode { get; set; } = GameMode.Normal;
        public KeysanityMode KeysanityMode { get; set; } = KeysanityMode.None;
        public bool Race { get; set; } = false;
        public bool DisableSpoilerLog { get; set; } = false;
        public bool DisableTrackerSpoilers { get; set; } = false;
        public bool DisableTrackerHints { get; set; } = false;
        public bool DisableCheats { get; set; } = false;
        public GanonInvincible GanonInvincible { get; set; } = GanonInvincible.BeforeCrystals;
        public bool ExtendedMsuSupport { get; set; } = false;
        public MusicShuffleMode ShuffleDungeonMusic { get; set; } = MusicShuffleMode.Default;
        public HeartColor HeartColor { get; set; } = HeartColor.Red;
        public LowHealthBeepSpeed LowHealthBeepSpeed { get; set; } = LowHealthBeepSpeed.Normal;
        public bool DisableLowEnergyBeep { get; set; } = false;
        public MenuSpeed MenuSpeed { get; set; } = MenuSpeed.Default;
        public bool CasualSMPatches { get; set; } = false;

        public bool GenerateSeedOnly { get; private set; } = false;

        public string LinkName { get; set; } = "Link";

        public string SamusName { get; set; } = "Samus";

        public bool SingleWorld => GameMode == GameMode.Normal;
        public bool MultiWorld => GameMode == GameMode.Multiworld;
        public bool Keysanity => KeysanityMode != KeysanityMode.None;
        public int Id { get; set; }
        public string PlayerName { get; set; } = "";
        public string PhoneticName { get; set; } = "";
        public string PlayerGuid { get; set; } = "";
        public string Seed { get; set; } = "";
        public string SettingsString { get; set; } = "";
        public bool CopySeedAndRaceSettings { get; set; }
        public IDictionary<int, int> LocationItems { get; set; } = new Dictionary<int, int>();
        public LogicConfig LogicConfig { get; set; } = new LogicConfig();
        public CasPatches CasPatches { get; set; } = new();
        public MetroidControlOptions MetroidControls { get; set; } = new();
        public PlandoConfig? PlandoConfig { get; set; }
        public MultiplayerPlayerGenerationData? MultiplayerPlayerGenerationData { get; set; }
        public ItemPlacementRule ItemPlacementRule { get; set; }
        public int UniqueHintCount { get; set; } = 8;
        public bool ZeldaKeysanity => KeysanityMode == KeysanityMode.Both || KeysanityMode == KeysanityMode.Zelda;
        public bool MetroidKeysanity => KeysanityMode == KeysanityMode.Both || KeysanityMode == KeysanityMode.SuperMetroid;
        public bool KeysanityForRegion(Region region) => KeysanityMode == KeysanityMode.Both || (region is Z3Region && ZeldaKeysanity) || (region is SMRegion && MetroidKeysanity);
        public int GanonsTowerCrystalCount { get; set; } = 7;
        public int GanonCrystalCount { get; set; } = 7;
        public bool OpenPyramid { get; set; }
        public int TourianBossCount { get; set; }  = 4;
        public IDictionary<string, int> ItemOptions { get; set; } = new Dictionary<string, int>();

        [System.Text.Json.Serialization.JsonIgnore, JsonIgnore]
        public bool IsLocalConfig { get; set; } = true;

        public Config SeedOnly()
        {
            var clone = (Config)MemberwiseClone();
            clone.GenerateSeedOnly = true;
            return clone;
        }

        /// <summary>
        /// Converts the config into a compressed string of the json
        /// </summary>
        /// <param name="config">The config to convert</param>
        /// <param name="compress">If the config should be compressed</param>
        /// <returns>The string representation</returns>
        public static string ToConfigString(Config config, bool compress)
        {
            var json = JsonSerializer.Serialize(config, s_options);
            if (!compress) return json;
            var buffer = Encoding.UTF8.GetBytes(json);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        /// <summary>
        /// Takes in a compressed json config string and converts it into a config
        /// </summary>
        /// <param name="configString"></param>
        /// <returns>The converted json data</returns>
        public static IEnumerable<Config> FromConfigString(string configString)
        {
            if (configString.Contains("{"))
                return new List<Config>() { JsonSerializer.Deserialize<Config>(configString, s_options) ?? new Config() };

            if (configString.StartsWith("["))
            {
                var configs = new List<Config>();
                var configStrings = JsonSerializer.Deserialize<List<string>>(configString, s_options) ?? new List<string>();
                return configStrings.SelectMany(x => FromConfigString(x));
            }

            var gZipBuffer = Convert.FromBase64String(configString);
            using (var memoryStream = new MemoryStream())
            {
                var dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                var json = Encoding.UTF8.GetString(buffer);
                return new List<Config>() { JsonSerializer.Deserialize<Config>(json, s_options) ?? new Config() };
            }
        }

        /// <summary>
        /// Takes a series of config files and generates them into a combined json array of
        /// their compressed config strings
        /// </summary>
        /// <param name="configs"></param>
        /// <returns></returns>
        public static string ToConfigString(IEnumerable<Config> configs)
        {
            var configStrings = new List<string>();
            foreach (var config in configs)
            {
                configStrings.Add(ToConfigString(config, true));
            }
            return JsonSerializer.Serialize(configStrings, s_options);
        }

        /// <summary>
        /// Takes a config file and generates it into a compressed config string
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static string ToConfigString(Config config)
        {
            return ToConfigString(new[] { config } );
        }
    }
}
