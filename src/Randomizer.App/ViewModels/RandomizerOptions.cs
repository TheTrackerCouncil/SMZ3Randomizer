using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using Randomizer.Shared;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking.AutoTracking;

namespace Randomizer.App.ViewModels
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

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonPropertyName("General")]
        public GeneralOptions GeneralOptions { get; }

        [JsonPropertyName("Seed")]
        public SeedOptions SeedOptions { get; }

        [JsonPropertyName("Patch")]
        public PatchOptions PatchOptions { get; }

        [JsonPropertyName("Logic")]
        public LogicConfig LogicConfig { get; set; }

        [JsonIgnore]
        public string FilePath { get; set; }

        public bool EarlyItemsExpanded { get; set; } = false;

        public bool CustomizationExpanded { get; set; } = false;
        public bool LogicExpanded { get; set; } = false;

        public bool CommonExpanded { get; set; } = true;

        public double WindowWidth { get; set; } = 500d;

        public double WindowHeight { get; set; } = 600d;


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
            var options = JsonSerializer.Deserialize<RandomizerOptions>(json, s_jsonOptions);
            options.FilePath = path;
            return options;
        }

        public void Save(string path = null)
        {
            if (path == null)
            {
                path = FilePath;
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
                    EarlyItems = SeedOptions.EarlyItems,
                    LogicConfig = LogicConfig.Clone(),
                    CopySeedAndRaceSettings = true,
                    Seed = SeedOptions.Seed,
                };
                return config;
            }
            else
            {
                var oldConfig = Config.FromConfigString(SeedOptions.ConfigString);

                var keysanity = SeedOptions.KeysanityMode;
                var itemPlacement = SeedOptions.ItemPlacementRule;
                var race = SeedOptions.Race;
                var disableSpoilerLog = SeedOptions.DisableSpoilerLog;
                var disableTrackerHints = SeedOptions.DisableTrackerHints;
                var disableTrackerSpoilers = SeedOptions.DisableTrackerSpoilers;
                var disableCheats = SeedOptions.DisableCheats;
                var seed = SeedOptions.Seed;

                if (SeedOptions.CopySeedAndRaceSettings)
                {
                    keysanity = oldConfig.KeysanityMode;
                    itemPlacement = oldConfig.ItemPlacementRule;
                    race = oldConfig.Race;
                    disableSpoilerLog = oldConfig.DisableSpoilerLog;
                    disableTrackerHints = oldConfig.DisableTrackerHints;
                    disableTrackerSpoilers = oldConfig.DisableTrackerSpoilers;
                    disableCheats = oldConfig.DisableCheats;
                    seed = oldConfig.Seed;
                }


                return new Config()
                {
                    GameMode = GameMode.Normal,
                    KeysanityMode = keysanity,
                    ItemPlacementRule = itemPlacement,
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
                    EarlyItems = oldConfig.EarlyItems,
                    LogicConfig = oldConfig.LogicConfig,
                    SettingsString = SeedOptions.ConfigString,
                    CopySeedAndRaceSettings = SeedOptions.CopySeedAndRaceSettings
                };
            }
        }

        public RandomizerOptions Clone()
        {
            return (RandomizerOptions)MemberwiseClone();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }
    }
}
