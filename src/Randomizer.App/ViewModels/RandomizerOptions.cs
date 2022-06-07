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

        public EmulatorConnectorType AutotrackerAutoStart
        {
            get => (EmulatorConnectorType)GeneralOptions.AutoTrackerConnectorType;
        }

        public static RandomizerOptions Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<RandomizerOptions>(json, s_jsonOptions);
        }

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this, s_jsonOptions);
            File.WriteAllText(path, json);
        }

        public Config ToConfig()
        {
            if (string.IsNullOrWhiteSpace(SeedOptions.ConfigString)) {
                return new()
                {
                    GameMode = GameMode.Normal,
                    Z3Logic = Z3Logic.Normal,
                    SMLogic = SMLogic.Normal,
                    ItemLocations =
                    {
                        [ItemType.ProgressiveSword] = SeedOptions.SwordLocation,
                        [ItemType.Morph] = SeedOptions.MorphLocation,
                        [ItemType.Bombs] = SeedOptions.MorphBombsLocation,
                        [ItemType.Boots] = SeedOptions.PegasusBootsLocation,
                        [ItemType.SpaceJump] = SeedOptions.SpaceJumpLocation,
                    },
                    ShaktoolItemPool = SeedOptions.ShaktoolItem,
                    PegWorldItemPool = SeedOptions.PegWorldItem,
                    KeyShuffle = SeedOptions.Keysanity ? KeyShuffle.Keysanity : KeyShuffle.None,
                    Race = SeedOptions.Race,
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
                    LogicConfig = LogicConfig.Clone()
                };
            }
            else
            {
                var oldConfig = Config.FromConfigString(SeedOptions.ConfigString);
                return new Config()
                {
                    GameMode = GameMode.Normal,
                    Z3Logic = Z3Logic.Normal,
                    SMLogic = SMLogic.Normal,
                    ItemLocations =
                    {
                        [ItemType.ProgressiveSword] = SeedOptions.SwordLocation,
                        [ItemType.Morph] = SeedOptions.MorphLocation,
                        [ItemType.Bombs] = SeedOptions.MorphBombsLocation,
                        [ItemType.Boots] = SeedOptions.PegasusBootsLocation,
                        [ItemType.SpaceJump] = SeedOptions.SpaceJumpLocation,
                    },
                    ShaktoolItemPool = SeedOptions.ShaktoolItem,
                    PegWorldItemPool = SeedOptions.PegWorldItem,
                    KeyShuffle = SeedOptions.Keysanity ? KeyShuffle.Keysanity : KeyShuffle.None,
                    Race = SeedOptions.Race,
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
                    LogicConfig = oldConfig.LogicConfig
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
