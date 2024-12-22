using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using MSURandomizerLibrary;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Shared.Enums;
using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Options;

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
        GeneralOptions = generalOptions;
        SeedOptions = seedOptions;
        PatchOptions = patchOptions;
        LogicConfig = logicConfig;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [JsonPropertyName("General")]
    public GeneralOptions GeneralOptions { get; set; }

    [JsonPropertyName("Seed")]
    public SeedOptions SeedOptions { get; set; }

    [JsonPropertyName("Patch")]
    public PatchOptions PatchOptions { get; set;  }

    [JsonPropertyName("Logic")]
    public LogicConfig LogicConfig { get; set; }

    [JsonIgnore, YamlIgnore]
    public string? FilePath { get; set; }

    public string? ApplicationVersion { get; set; }

    public bool IsAdvancedMode { get; set; }

    public double WindowWidth { get; set; } = 500d;

    public double WindowHeight { get; set; } = 600d;
    public string MultiplayerUrl { get; set; } = "";

    [YamlIgnore]
    public string RomOutputPath
    {
        get => Directory.Exists(GeneralOptions.RomOutputPath)
            ? GeneralOptions.RomOutputPath
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "Seeds");
    }

    [YamlIgnore]
    public string AutoTrackerScriptsOutputPath
    {
        get => Directory.Exists(GeneralOptions.AutoTrackerScriptsOutputPath)
            ? GeneralOptions.AutoTrackerScriptsOutputPath
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "AutoTrackerScripts");
    }

    public static RandomizerOptions Load(string loadPath, string savePath, bool isYaml)
    {
        var fileText = File.ReadAllText(loadPath);

        if (isYaml)
        {
            var serializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            var options = serializer.Deserialize<RandomizerOptions>(fileText);
            options.FilePath = savePath;

            var settingsUpdated = false;

            // Update from AutoTracker connector settings to SnesConnector settings
            if (options.GeneralOptions.AutoTrackerDefaultConnectionType != EmulatorConnectorType.None)
            {
                options.GeneralOptions.SnesConnectorSettings.ConnectorType =
                    options.GeneralOptions.AutoTrackerDefaultConnectionType == EmulatorConnectorType.Lua
                        ? SnesConnectorType.Lua
                        : SnesConnectorType.Usb2Snes;
                options.GeneralOptions.SnesConnectorSettings.Usb2SnesAddress =
                    options.GeneralOptions.AutoTrackerQUsb2SnesIp ?? "";
                options.GeneralOptions.AutoTrackerDefaultConnectionType = EmulatorConnectorType.None;
                settingsUpdated = true;
            }

            if (options.GeneralOptions.MsuTrackDisplayStyle != null)
            {
                options.GeneralOptions.TrackDisplayFormat = options.GeneralOptions.MsuTrackDisplayStyle.Value
                    switch
                    {
                        MsuTrackDisplayStyle.Horizontal => TrackDisplayFormat.Horizontal,
                        MsuTrackDisplayStyle.Vertical => TrackDisplayFormat.Vertical,
                        MsuTrackDisplayStyle.HorizonalWithMsu => TrackDisplayFormat.HorizonalWithMsu,
                        MsuTrackDisplayStyle.SentenceStyle => TrackDisplayFormat.SentenceStyle,
                        _ => TrackDisplayFormat.Vertical
                    };
                options.GeneralOptions.MsuTrackDisplayStyle = null;
                settingsUpdated = true;
            }

            // Update AutoTrackerChangeMap to AutoMapUpdateBehavior
            if (options.GeneralOptions.AutoMapUpdateBehavior == null)
            {
                options.GeneralOptions.AutoMapUpdateBehavior = options.GeneralOptions.AutoTrackerChangeMap
                    ? AutoMapUpdateBehavior.UpdateOnRegionChange
                    : AutoMapUpdateBehavior.Disabled;
                settingsUpdated = true;
            }

            // Remove deprecated config profiles
            if (options.GeneralOptions.SelectedProfiles.Any(p => p != null && ConfigProvider.DeprecatedConfigProfiles.Contains(p)))
            {
                options.GeneralOptions.SelectedProfiles = options.GeneralOptions.SelectedProfiles
                    .Where(p => p != null && !ConfigProvider.DeprecatedConfigProfiles.Contains(p)).ToList();
                settingsUpdated = true;
            }

            // Update HasOpenedSetupWindow if the Z3 rom path is populated
            if (!options.GeneralOptions.HasOpenedSetupWindow && !string.IsNullOrEmpty(options.GeneralOptions.Z3RomPath))
            {
                options.GeneralOptions.HasOpenedSetupWindow = true;
                settingsUpdated = true;
            }

            if (settingsUpdated)
            {
                options.Save();
            }

            return options;
        }
        else
        {
            var options = JsonSerializer.Deserialize<RandomizerOptions>(fileText, s_jsonOptions) ?? new();
            options.FilePath = savePath;
            options.GeneralOptions.TrackerVoiceFrequency =
                (TrackerVoiceFrequency)options.GeneralOptions.VoiceFrequency;
            options.GeneralOptions.LaunchButtonOption =
                (LaunchButtonOptions)options.GeneralOptions.LaunchButton;
            return options;
        }
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

        var serializer = new Serializer();
        var yamlText = serializer.Serialize(this);
        File.WriteAllText(path, yamlText);
    }

    public Config ToConfig()
    {
        if (SeedOptions.UniqueHintCount != null)
        {
            PatchOptions.CasPatches.HintTiles = SeedOptions.UniqueHintCount.Value;
        }

        if (PatchOptions.ZeldaDrops != null)
        {
            PatchOptions.CasPatches.ZeldaDrops = PatchOptions.ZeldaDrops.Value;
        }

        return new Config()
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
            LocationItems = SeedOptions.LocationItems,
            LogicConfig = LogicConfig.Clone(),
            CasPatches = PatchOptions.CasPatches.Clone(),
            CopySeedAndRaceSettings = true,
            Seed = SeedOptions.Seed,
            GanonsTowerCrystalCount = SeedOptions.GanonsTowerCrystalCount,
            GanonCrystalCount = SeedOptions.GanonCrystalCount,
            OpenPyramid = SeedOptions.OpenPyramid,
            TourianBossCount = SeedOptions.TourianBossCount,
            MetroidControls = PatchOptions.MetroidControls.Clone(),
            ItemOptions = SeedOptions.ItemOptions,
        };
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
