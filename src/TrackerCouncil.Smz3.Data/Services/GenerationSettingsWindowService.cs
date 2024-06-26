using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicForms.Library.Core.Attributes;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Services;
using TrackerCouncil.Data;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Interfaces;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ViewModels;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TrackerCouncil.Smz3.Data.Services;

public class GenerationSettingsWindowService(SpriteService spriteService, OptionsFactory optionsFactory, IRomGenerationService romGenerator, LocationConfig locations, ILogger<GenerationSettingsWindowService> logger, IMsuLookupService msuLookupService)
{
    private RandomizerOptions _options = null!;
    private GenerationWindowViewModel _model = null!;
    private static readonly char[] s_invalidFileNameChars = Path.GetInvalidFileNameChars();

    public GenerationWindowViewModel GetViewModel()
    {
        _options = optionsFactory.Create();
        _model = new GenerationWindowViewModel();

        _model.Basic.Presets = GetPresets();
        _model.Basic.SelectedPreset = _model.Basic.Presets.First();
        _model.Basic.MsuShuffleStyle = _options.PatchOptions.MsuShuffleStyle;
        _model.Basic.MsuRandomizationStyle = _options.PatchOptions.MsuRandomizationStyle;

        _model.GameSettings.KeysanityMode = _options.SeedOptions.KeysanityMode;
        _model.GameSettings.CrystalsNeededForGT = _options.SeedOptions.GanonsTowerCrystalCount;
        _model.GameSettings.CrystalsNeededForGanon = _options.SeedOptions.GanonCrystalCount;
        _model.GameSettings.BossesNeededForTourian = _options.SeedOptions.TourianBossCount;
        _model.GameSettings.OpenPyramid = _options.SeedOptions.OpenPyramid;
        _model.GameSettings.Race = _options.SeedOptions.Race;
        _model.GameSettings.DisableSpoilerLog = _options.SeedOptions.DisableSpoilerLog;
        _model.GameSettings.DisableTrackerHints = _options.SeedOptions.DisableTrackerHints;
        _model.GameSettings.DisableTrackerSpoilers = _options.SeedOptions.DisableTrackerSpoilers;
        _model.GameSettings.DisableCheats = _options.SeedOptions.DisableCheats;

        _model.Logic.LogicConfig = _options.LogicConfig.Clone();
        _model.Logic.CasPatches = _options.PatchOptions.CasPatches.Clone();

        if (_options.PatchOptions.ZeldaDrops != null)
        {
            _model.Logic.CasPatches.ZeldaDrops = _options.PatchOptions.ZeldaDrops.Value;
            _options.PatchOptions.ZeldaDrops = null;
        }

        if (_options.SeedOptions.UniqueHintCount != null)
        {
            _model.Logic.CasPatches.HintTiles = _options.SeedOptions.UniqueHintCount.Value;
            _options.SeedOptions.UniqueHintCount = null;
        }

        _model.Items.Init(_options, locations);

        _model.Customizations.HeartColor = _options.PatchOptions.HeartColor;
        _model.Customizations.MenuSpeed = _options.PatchOptions.MenuSpeed;
        _model.Customizations.LowHealthBeepSpeed = _options.PatchOptions.LowHealthBeepSpeed;
        _model.Customizations.DisableLowEnergyBeep = _options.PatchOptions.DisableLowEnergyBeep;
        _model.Customizations.RunButtonBehavior = _options.PatchOptions.MetroidControls.RunButtonBehavior;
        _model.Customizations.ItemCancelBehavior = _options.PatchOptions.MetroidControls.ItemCancelBehavior;
        _model.Customizations.AimButtonBehavior = _options.PatchOptions.MetroidControls.AimButtonBehavior;
        _model.Customizations.MoonWalk = _options.PatchOptions.MetroidControls.MoonWalk;
        _model.Customizations.ShootButton = _options.PatchOptions.MetroidControls.Shoot;
        _model.Customizations.JumpButton = _options.PatchOptions.MetroidControls.Jump;
        _model.Customizations.DashButton = _options.PatchOptions.MetroidControls.Dash;
        _model.Customizations.ItemSelectButton = _options.PatchOptions.MetroidControls.ItemSelect;
        _model.Customizations.ItemCancelButton = _options.PatchOptions.MetroidControls.ItemCancel;
        _model.Customizations.AimUpButton = _options.PatchOptions.MetroidControls.AimUp;
        _model.Customizations.AimDownButton = _options.PatchOptions.MetroidControls.AimDown;

        UpdateMsuText();
        _ = LoadSprites();
        return _model;
    }

    public void ApplyConfig(string config, bool copySeed)
    {
        try
        {
            var configs = Config.FromConfigString(config.Trim());
            if (!configs.Any())
            {
                ConfigError?.Invoke(this, EventArgs.Empty);
                return;
            }
            ApplyConfig(Config.FromConfigString(config).First(), copySeed);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to parse config string");
            ConfigError?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ApplyConfig(Config config, bool copySeed)
    {
        if (copySeed)
        {
            _model.Basic.Seed = config.Seed;
        }

        _model.GameSettings.KeysanityMode = config.KeysanityMode;
        _model.GameSettings.CrystalsNeededForGT = config.GanonsTowerCrystalCount;
        _model.GameSettings.CrystalsNeededForGanon = config.GanonCrystalCount;
        _model.GameSettings.BossesNeededForTourian = config.TourianBossCount;
        _model.GameSettings.OpenPyramid = config.OpenPyramid;
        _model.GameSettings.Race = config.Race;
        _model.GameSettings.DisableSpoilerLog = config.DisableSpoilerLog;
        _model.GameSettings.DisableTrackerHints = config.DisableTrackerHints;
        _model.GameSettings.DisableTrackerSpoilers = config.DisableTrackerSpoilers;
        _model.GameSettings.DisableCheats = config.DisableCheats;
        _model.GameSettings.RefreshAll();

        _model.Logic.LogicConfig.Copy(config.LogicConfig);
        _model.Logic.CasPatches.Copy(config.CasPatches);

        if (config.UniqueHintCount != null)
        {
            _model.Logic.CasPatches.HintTiles = config.UniqueHintCount.Value;
        }

        if (config.ZeldaDrops != null)
        {
            _model.Logic.CasPatches.ZeldaDrops = config.ZeldaDrops.Value;
        }

        _model.Logic.RefreshAll();

        _model.Items.ApplyConfig(config);

        UpdateSummaryText();
    }

    public bool VerifyConfigVersion(string config, out bool versionMismatch)
    {
        try
        {
            var configObject = Config.FromConfigString(config.Trim()).FirstOrDefault();
            if (configObject == null)
            {
                ConfigError?.Invoke(this, EventArgs.Empty);
                versionMismatch = false;
                return false;
            }

            versionMismatch = !VerifyConfigVersion(configObject);
            return !versionMismatch;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to parse config string");
            ConfigError?.Invoke(this, EventArgs.Empty);
            versionMismatch = false;
            return false;
        }
    }

    public bool VerifyConfigVersion(Config config)
    {
        return config.RandomizerVersion == RandomizerVersion.VersionString;
    }

    public void SaveSettings()
    {
        _options.SeedOptions.Seed = _model.Basic.Seed;
        _options.SeedOptions.KeysanityMode = _model.GameSettings.KeysanityMode;
        _options.SeedOptions.GanonsTowerCrystalCount = _model.GameSettings.CrystalsNeededForGT;
        _options.SeedOptions.GanonCrystalCount = _model.GameSettings.CrystalsNeededForGanon;
        _options.SeedOptions.TourianBossCount = _model.GameSettings.BossesNeededForTourian;
        _options.SeedOptions.OpenPyramid = _model.GameSettings.OpenPyramid;
        _options.SeedOptions.Race = _model.GameSettings.Race;
        _options.SeedOptions.DisableSpoilerLog = _model.GameSettings.DisableSpoilerLog;
        _options.SeedOptions.DisableTrackerHints = _model.GameSettings.DisableTrackerHints;
        _options.SeedOptions.DisableTrackerSpoilers = _model.GameSettings.DisableTrackerSpoilers;
        _options.SeedOptions.DisableCheats = _model.GameSettings.DisableCheats;

        _options.LogicConfig = _model.Logic.LogicConfig.Clone();
        _options.PatchOptions.CasPatches = _model.Logic.CasPatches.Clone();
        _options.PatchOptions.MsuShuffleStyle = _model.Basic.MsuShuffleStyle;

        _model.Items.ApplySettings(_options);

        _options.PatchOptions.HeartColor = _model.Customizations.HeartColor;
        _options.PatchOptions.MenuSpeed = _model.Customizations.MenuSpeed;
        _options.PatchOptions.LowHealthBeepSpeed = _model.Customizations.LowHealthBeepSpeed;
        _options.PatchOptions.DisableLowEnergyBeep = _model.Customizations.DisableLowEnergyBeep;
        _options.PatchOptions.MetroidControls.RunButtonBehavior = _model.Customizations.RunButtonBehavior;
        _options.PatchOptions.MetroidControls.ItemCancelBehavior = _model.Customizations.ItemCancelBehavior;
        _options.PatchOptions.MetroidControls.AimButtonBehavior = _model.Customizations.AimButtonBehavior;
        _options.PatchOptions.MetroidControls.MoonWalk = _model.Customizations.MoonWalk;
        _options.PatchOptions.MetroidControls.Shoot = _model.Customizations.ShootButton;
        _options.PatchOptions.MetroidControls.Jump = _model.Customizations.JumpButton;
        _options.PatchOptions.MetroidControls.Dash = _model.Customizations.DashButton;
        _options.PatchOptions.MetroidControls.ItemSelect = _model.Customizations.ItemSelectButton;
        _options.PatchOptions.MetroidControls.ItemCancel = _model.Customizations.ItemCancelButton;
        _options.PatchOptions.MetroidControls.AimUp = _model.Customizations.AimUpButton;
        _options.PatchOptions.MetroidControls.AimDown = _model.Customizations.AimDownButton;

        _options.Save();
    }

    public bool LoadPlando(string file, out string? error)
    {
        error = null;
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            var configString = File.ReadAllText(file);
            LoadPlando(deserializer.Deserialize<PlandoConfig>(configString));
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to parse plando file");
            error = $"Unable to parse plando file: {e.Message}";
            return false;
        }
    }

    public void LoadPlando(PlandoConfig config)
    {
        _model.PlandoConfig = config;

        _model.GameSettings.KeysanityMode = _model.PlandoConfig.KeysanityMode;
        _model.GameSettings.CrystalsNeededForGT = _model.PlandoConfig.GanonsTowerCrystalCount;
        _model.GameSettings.CrystalsNeededForGanon = _model.PlandoConfig.GanonCrystalCount;
        _model.GameSettings.OpenPyramid = _model.PlandoConfig.OpenPyramid;
        _model.GameSettings.BossesNeededForTourian = _model.PlandoConfig.TourianBossCount;

        _model.Logic.LogicConfig.Copy(_model.PlandoConfig.Logic);
    }

    public void SaveSpriteSettings(bool userAccepted, Sprite sprite, Dictionary<string, SpriteOptions> selectedSpriteOptions, string searchText, SpriteFilter filter)
    {
        if (sprite.SpriteType == SpriteType.Link)
        {
            _options.GeneralOptions.LinkSpriteOptions = selectedSpriteOptions;

            if (userAccepted)
            {
                _options.PatchOptions.SelectedLinkSprite = sprite;
                _options.GeneralOptions.LinkSpriteSearchText = searchText;
                _options.GeneralOptions.LinkSpriteFilter = filter;
            }
        }
        else if (sprite.SpriteType == SpriteType.Samus)
        {
            _options.GeneralOptions.SamusSpriteOptions = selectedSpriteOptions;

            if (userAccepted)
            {
                _options.PatchOptions.SelectedSamusSprite = sprite;
                _options.GeneralOptions.SamusSpriteSearchText = searchText;
                _options.GeneralOptions.SamusSpriteFilter = filter;
            }
        }
        else if (sprite.SpriteType == SpriteType.Ship)
        {
            _options.GeneralOptions.ShipSpriteOptions = selectedSpriteOptions;

            if (userAccepted)
            {
                _options.PatchOptions.SelectedShipSprite = sprite;
                _options.GeneralOptions.ShipSpriteSearchText = searchText;
                _options.GeneralOptions.ShipSpriteFilter = filter;
            }
        }

        _options.Save();

        if (userAccepted)
        {
            UpdateSpriteDetails(sprite.SpriteType, sprite);
        }
    }

    public void UpdateMsuText()
    {
        if (_options.PatchOptions.MsuRandomizationStyle == null && _options.PatchOptions.MsuPaths.Any())
        {
            if (!string.IsNullOrEmpty(_options.GeneralOptions.MsuPath))
            {
                _model.Basic.MsuText = Path.GetRelativePath(_options.GeneralOptions.MsuPath, _options.PatchOptions.MsuPaths.First());
            }
            else
            {
                _model.Basic.MsuText = _options.PatchOptions.MsuPaths.First();
            }
        }
        else if (_options.PatchOptions.MsuRandomizationStyle == MsuRandomizationStyle.Single)
        {
            _model.Basic.MsuText = $"Random MSU from {_options.PatchOptions.MsuPaths.Count} MSUs";
        }
        else if (_options.PatchOptions.MsuRandomizationStyle == MsuRandomizationStyle.Shuffled)
        {
            _model.Basic.MsuText = $"Shuffled MSU from {_options.PatchOptions.MsuPaths.Count} MSUs";
        }
        else if (_options.PatchOptions.MsuRandomizationStyle == MsuRandomizationStyle.Continuous)
        {
            _model.Basic.MsuText = $"Continuously Shuffle {_options.PatchOptions.MsuPaths.Count} MSUs";
        }
        else
        {
            _model.Basic.MsuText = "None";
        }
    }

    public bool IsUserMsuPathValid => !string.IsNullOrEmpty(_options.GeneralOptions.MsuPath) &&
                                       Directory.Exists(_options.GeneralOptions.MsuPath);

    public void UpdateUserMsuPath(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        _options.GeneralOptions.MsuPath = path;
        _options.Save();

        Task.Run(() =>
        {
            msuLookupService.LookupMsus(path);
        });
    }


    public void SetMsuPaths(List<string> paths, MsuRandomizationStyle? randomizationStyle)
    {
        _options.PatchOptions.MsuPaths = paths;
        _options.PatchOptions.MsuRandomizationStyle = randomizationStyle;
        UpdateMsuText();
        _options.Save();
        _model.Basic.MsuRandomizationStyle = randomizationStyle;
    }

    public void SetMsuPath(string path)
    {
        SetMsuPaths([path], null);
    }

    public void ClearMsu()
    {
        SetMsuPaths([], null);
    }

    public async Task<GeneratedRomResult> GenerateRom()
    {
        return _model.IsPlando
            ? await romGenerator.GeneratePlandoRomAsync(_options, _model.PlandoConfig!)
            : await romGenerator.GenerateRandomRomAsync(_options);
    }

    public List<RandomizerPreset> GetPresets()
    {
        List<RandomizerPreset> presets =
        [
            new RandomizerPreset() { PresetName = "Select a preset to apply its settings", Config = null }
        ];

        presets.AddRange(RandomizerPreset.GetDefaultPresets());

        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SMZ3CasRandomizer", "Presets");

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        if (Directory.Exists(folder))
        {
            foreach (var file in Directory.EnumerateFiles(folder))
            {
                try
                {

                    presets.Add(deserializer.Deserialize<RandomizerPreset>(File.ReadAllText(file)));
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Could not load preset {Path} file", file);
                }
            }
        }

        return presets.OrderBy(x => x.Config != null)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.PresetName)
            .ToList();
    }

    public bool ApplySelectedPreset()
    {
        if (_model.Basic.SelectedPreset?.Config == null)
        {
            return false;
        }

        ApplyConfig(_model.Basic.SelectedPreset.Config, false);
        return true;
    }

    public bool DeleteSelectedPreset(out string? error)
    {
        error = null;
        if (string.IsNullOrEmpty(_model.Basic.SelectedPreset?.FilePath))
        {
            error = "Cannot delete default presets";
            return false;
        }

        File.Delete(_model.Basic.SelectedPreset.FilePath);
        _model.Basic.Presets = GetPresets();
        _model.Basic.SelectedPreset = _model.Basic.Presets.First();
        return true;
    }

    public bool CreatePreset(string name, out string? error)
    {
        error = null;

        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SMZ3CasRandomizer", "Presets");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var fileSafeName = new string(name.Where(ch => !s_invalidFileNameChars.Contains(ch)).ToArray());

        var path = _model.Basic.Presets.FirstOrDefault()?.FilePath
                   ?? Path.Combine(folder, $"{fileSafeName}-{Guid.NewGuid()}.yml");

        var newPreset = new RandomizerPreset() { PresetName = name, Config = GetConfig(), FilePath = path };
        var serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        var yamlText = serializer.Serialize(newPreset);
        File.WriteAllText(path, yamlText);
        _model.Basic.Presets = GetPresets();

        logger.LogInformation("Created preset at {Path}", path);

        return true;
    }

    public event EventHandler? ConfigError;

    public Config GetConfig()
    {
        SaveSettings();
        return _options.ToConfig();
    }

    public void UpdateSummaryText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Game Settings");
        sb.AppendLine(GetObjectSummary(_model.GameSettings, "  - ", Environment.NewLine));
        sb.AppendLine("Logic");
        sb.AppendLine(GetObjectSummary(_model.Logic.LogicConfig, "  - ", Environment.NewLine));
        sb.AppendLine("Cas' Patches");
        sb.AppendLine(GetObjectSummary(_model.Logic.CasPatches, "  - ", Environment.NewLine));

        if (_model.Items.MetroidItemOptions.Any(x => x.SelectedOption != x.Options.First()))
        {
            sb.AppendLine("Metroid Items");
            foreach (var item in _model.Items.MetroidItemOptions.Where(x => x.SelectedOption != x.Options.First()))
            {
                sb.AppendLine($"  - {item.Title} = {item.SelectedOption.Display}");
            }
        }

        if (_model.Items.ZeldaItemOptions.Any(x => x.SelectedOption != x.Options.First()))
        {
            sb.AppendLine("Zelda Items");
            foreach (var item in _model.Items.ZeldaItemOptions.Where(x => x.SelectedOption != x.Options.First()))
            {
                sb.AppendLine($"  - {item.Title} = {item.SelectedOption.Display}");
            }
        }

        if (_model.Items.LocationOptions.Any(x => x.SelectedOption != x.Options.First()))
        {
            sb.AppendLine("Locations");
            foreach (var item in _model.Items.LocationOptions.Where(x => x.SelectedOption != x.Options.First()))
            {
                sb.AppendLine($"  - {item.LocationName} = {item.SelectedOption.Text}");
            }
        }

        _model.Basic.Summary = sb.ToString().Trim();
    }

    public string? GetMsuDirectory()
    {
        return _options.GeneralOptions.MsuPath;
    }

    private string GetObjectSummary(object obj, string prefix, string separator)
    {
        var properties = obj.GetType().GetProperties()
            .Select(x => (Property: x,
                Attributes: x.GetCustomAttributes(true).FirstOrDefault(a => a is DynamicFormFieldAttribute) as DynamicFormFieldAttribute))
            .Where(x => x is { Attributes: not null, Property.CanWrite: true });

        var results = new List<string>();

        foreach (var property in properties)
        {
            var value = property.Property.GetValue(obj);
            if (property.Attributes is DynamicFormFieldCheckBoxAttribute checkBoxAttribute)
            {
                var valueText = (bool?)value == true ? "Yes" : "No";
                results.Add($"{prefix}{checkBoxAttribute.CheckBoxText} = {valueText}");
            }
            else
            {
                results.Add($"{prefix}{property.Attributes?.Label.Replace(":", "")} = {value}");
            }
        }

        return string.Join(separator, results);
    }

    private async Task LoadSprites()
    {
        try
        {
            await spriteService.LoadSpritesAsync();
            UpdateSprites();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading sprites");
        }
    }

    private void UpdateSprites()
    {
        UpdateSpriteDetails(SpriteType.Link);
        UpdateSpriteDetails(SpriteType.Samus);
        UpdateSpriteDetails(SpriteType.Ship);
    }

    private void UpdateSpriteDetails(SpriteType type, Sprite? sprite = null)
    {
        if (sprite == null)
        {
            if (type == SpriteType.Link)
            {
                sprite = _options.PatchOptions.SelectedLinkSprite;
            }
            else if (type == SpriteType.Samus)
            {
                sprite = _options.PatchOptions.SelectedSamusSprite;
            }
            else
            {
                sprite = _options.PatchOptions.SelectedShipSprite;
            }

            if (sprite is { IsDefault: false, IsRandomSprite: false })
            {
                sprite = spriteService.GetSprite(type);
            }
        }

        string spriteName;
        string spritePath;

        if (sprite.IsDefault)
        {
            sprite = type switch
            {
                SpriteType.Link => Sprite.DefaultLink,
                SpriteType.Samus => Sprite.DefaultSamus,
                _ => Sprite.DefaultShip
            };
            spriteName = sprite.Name;
            spritePath = sprite.PreviewPath;
        }
        else if (sprite.IsRandomSprite)
        {
            sprite = type switch
            {
                SpriteType.Link => Sprite.RandomLink,
                SpriteType.Samus => Sprite.RandomSamus,
                _ => Sprite.RandomShip
            };
            spriteName = sprite.Name;
            spritePath = Sprite.RandomSamus.PreviewPath;
        }
        else
        {
            spriteName = sprite.ToString();
            spritePath = sprite.PreviewPath;
        }

        if (type == SpriteType.Link)
        {
            _model.Basic.LinkSpriteName = spriteName;
            _model.Basic.LinkSpritePath = spritePath;
        }
        else if (type == SpriteType.Samus)
        {
            _model.Basic.SamusSpriteName = spriteName;
            _model.Basic.SamusSpritePath = spritePath;
        }
        else if (type == SpriteType.Ship)
        {
            _model.Basic.ShipSpriteName = spriteName;
            _model.Basic.ShipSpritePath = spritePath;
        }
    }


}
