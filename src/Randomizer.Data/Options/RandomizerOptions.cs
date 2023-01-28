using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Randomizer.Data.Logic;

namespace Randomizer.Data.Options
{
    public class RandomizerOptions : INotifyPropertyChanged
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        public RandomizerOptions()
        {
            GeneralOptions = new GeneralOptions();
            SeedOptions = new SeedOptions();
            PatchOptions = new PatchOptions();
            LogicConfig = new LogicConfig();
        }

        [JsonConstructor]
        public RandomizerOptions(GeneralOptions generalOptions,
            SeedOptions seedOptions,
            PatchOptions patchOptions,
            LogicConfig logicConfig)
        {
            GeneralOptions = generalOptions ?? new();
            SeedOptions = seedOptions ?? new();
            PatchOptions = patchOptions ?? new();
            LogicConfig = logicConfig ?? new();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonPropertyName("General")]
        public GeneralOptions GeneralOptions { get; }

        [JsonPropertyName("Seed")]
        public SeedOptions SeedOptions { get; }

        [JsonPropertyName("Patch")]
        public PatchOptions PatchOptions { get; }

        [JsonPropertyName("Logic")]
        public LogicConfig LogicConfig { get; set; }

        [JsonIgnore]
        public string? FilePath { get; set; }

        public bool EarlyItemsExpanded { get; set; } = false;

        public bool CustomizationExpanded { get; set; } = false;
        public bool LogicExpanded { get; set; } = false;

        public bool CommonExpanded { get; set; } = false;
        public bool MetroidControlsExpanded { get; set; } = false;

        public double WindowWidth { get; set; } = 500d;

        public double WindowHeight { get; set; } = 600d;

        public string MultiplayerUrl { get; set; } = "";

        public string RomOutputPath
        {
            get => Directory.Exists(GeneralOptions.RomOutputPath)
                    ? GeneralOptions.RomOutputPath
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "Seeds");
        }

        public string AutoTrackerScriptsOutputPath
        {
            get => Directory.Exists(GeneralOptions.AutoTrackerScriptsOutputPath)
                    ? GeneralOptions.AutoTrackerScriptsOutputPath
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "AutoTrackerScripts");
        }

        public EmulatorConnectorType AutoTrackerDefaultConnector
        {
            get => (EmulatorConnectorType)GeneralOptions.AutoTrackerDefaultConnector;
        }

        public static RandomizerOptions Load(string path)
        {
            var json = File.ReadAllText(path);
            var options = JsonSerializer.Deserialize<RandomizerOptions>(json, s_jsonOptions) ?? new();
            options.FilePath = path;
            return options;
        }

        public void Save(string? path = null)
        {
            if (path == null)
            {
                path = FilePath;
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var json = JsonSerializer.Serialize(this, s_jsonOptions);
            File.WriteAllText(path, json);
        }

        public Config ToConfig()
        {
            if (string.IsNullOrWhiteSpace(SeedOptions.ConfigString)) {
                var config = new Config()
                {
                    GameMode = GameMode.Normal,
                    KeysanityMode = SeedOptions.KeysanityMode,
                    Race = SeedOptions.Race,
                    ItemPlacementRule = SeedOptions.ItemPlacementRule,
                    DisableSpoilerLog = SeedOptions.DisableSpoilerLog,
                    DisableTrackerHints = SeedOptions.DisableTrackerHints,
                    DisableTrackerSpoilers = SeedOptions.DisableTrackerSpoilers,
                    DisableCheats = SeedOptions.DisableCheats,
                    ExtendedMsuSupport = PatchOptions.CanEnableExtendedSoundtrack && PatchOptions.EnableExtendedSoundtrack,
                    ShuffleDungeonMusic = PatchOptions.ShuffleDungeonMusic,
                    HeartColor = PatchOptions.HeartColor,
                    LowHealthBeepSpeed = PatchOptions.LowHealthBeepSpeed,
                    DisableLowEnergyBeep = PatchOptions.DisableLowEnergyBeep,
                    CasualSMPatches = PatchOptions.CasualSuperMetroidPatches,
                    MenuSpeed = PatchOptions.MenuSpeed,
                    LinkName = PatchOptions.LinkSprite == Sprite.DefaultLink ? "Link" : PatchOptions.LinkSprite.Name,
                    SamusName = PatchOptions.SamusSprite == Sprite.DefaultSamus ? "Samus" : PatchOptions.SamusSprite.Name,
                    LocationItems = SeedOptions.LocationItems,
                    LogicConfig = LogicConfig.Clone(),
                    CasPatches = PatchOptions.CasPatches.Clone(),
                    CopySeedAndRaceSettings = true,
                    Seed = SeedOptions.Seed,
                    UniqueHintCount = SeedOptions.UniqueHintCount,
                    GanonsTowerCrystalCount = SeedOptions.GanonsTowerCrystalCount,
                    GanonCrystalCount = SeedOptions.GanonCrystalCount,
                    OpenPyramid = SeedOptions.OpenPyramid,
                    TourianBossCount = SeedOptions.TourianBossCount,
                    MetroidControls = PatchOptions.MetroidControls.Clone(),
                    ItemOptions = SeedOptions.ItemOptions,
                };
                return config;
            }
            else
            {
                var oldConfig = Config.FromConfigString(SeedOptions.ConfigString).First();

                var race = SeedOptions.Race;
                var disableSpoilerLog = SeedOptions.DisableSpoilerLog;
                var disableTrackerHints = SeedOptions.DisableTrackerHints;
                var disableTrackerSpoilers = SeedOptions.DisableTrackerSpoilers;
                var disableCheats = SeedOptions.DisableCheats;
                var casPatches = PatchOptions.CasPatches.Clone();
                var seed = SeedOptions.Seed;

                if (SeedOptions.CopySeedAndRaceSettings)
                {
                    race = oldConfig.Race;
                    disableSpoilerLog = oldConfig.DisableSpoilerLog;
                    disableTrackerHints = oldConfig.DisableTrackerHints;
                    disableTrackerSpoilers = oldConfig.DisableTrackerSpoilers;
                    disableCheats = oldConfig.DisableCheats;
                    casPatches = oldConfig.CasPatches;
                    seed = oldConfig.Seed;
                }

                return new Config()
                {
                    GameMode = GameMode.Normal,
                    KeysanityMode = oldConfig.KeysanityMode,
                    ItemPlacementRule = oldConfig.ItemPlacementRule,
                    Race = race,
                    DisableSpoilerLog = disableSpoilerLog,
                    DisableTrackerHints = disableTrackerHints,
                    DisableTrackerSpoilers = disableTrackerSpoilers,
                    DisableCheats = disableCheats,
                    Seed = seed,
                    ExtendedMsuSupport = PatchOptions.CanEnableExtendedSoundtrack && PatchOptions.EnableExtendedSoundtrack,
                    ShuffleDungeonMusic = PatchOptions.ShuffleDungeonMusic,
                    HeartColor = PatchOptions.HeartColor,
                    LowHealthBeepSpeed = PatchOptions.LowHealthBeepSpeed,
                    DisableLowEnergyBeep = PatchOptions.DisableLowEnergyBeep,
                    CasualSMPatches = PatchOptions.CasualSuperMetroidPatches,
                    MenuSpeed = PatchOptions.MenuSpeed,
                    LinkName = PatchOptions.LinkSprite == Sprite.DefaultLink ? "Link" : PatchOptions.LinkSprite.Name,
                    SamusName = PatchOptions.SamusSprite == Sprite.DefaultSamus ? "Samus" : PatchOptions.SamusSprite.Name,
                    LocationItems = oldConfig.LocationItems,
                    LogicConfig = oldConfig.LogicConfig,
                    CasPatches = casPatches,
                    SettingsString = SeedOptions.ConfigString,
                    UniqueHintCount = oldConfig.UniqueHintCount,
                    CopySeedAndRaceSettings = SeedOptions.CopySeedAndRaceSettings,
                    GanonsTowerCrystalCount = oldConfig.GanonsTowerCrystalCount,
                    GanonCrystalCount = oldConfig.GanonCrystalCount,
                    OpenPyramid = oldConfig.OpenPyramid,
                    TourianBossCount = oldConfig.TourianBossCount,
                    MetroidControls = PatchOptions.MetroidControls.Clone(),
                    ItemOptions = oldConfig.ItemOptions,
                };
            }
        }

        public RandomizerOptions Clone()
        {
            return (RandomizerOptions)MemberwiseClone();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }
    }
}
