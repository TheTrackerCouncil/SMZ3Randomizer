using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TrackerCouncil.Smz3.Data.Configuration;

/// <summary>
/// Provides tracker configuration data.
/// </summary>
public partial class ConfigProvider
{
    public static HashSet<string> DeprecatedConfigProfiles = ["Halloween Tracker Sprites", "Plain Tracker Sprites"];

    private static readonly JsonSerializerOptions s_options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly ILogger<ConfigProvider>? _logger;
    private List<ConfigProfileDetails> _profiles = new();
    private HashSet<string> _moods = [];

    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="ConfigProvider"/> class.
    /// </summary>
    public ConfigProvider(ILogger<ConfigProvider>? logger)
    {
        _logger = logger;
        InitUserConfigFolder();
        LoadConfigProfiles();
    }

    private void InitUserConfigFolder()
    {
        if (Directory.Exists(Directories.UserConfigPath))
        {
           CopySchemaFolder();
            return;
        }

        Directory.CreateDirectory(Directories.UserConfigPath);

        _logger?.LogInformation("Created user config directory {Path}", Directories.UserConfigPath);

        var deprecatedConfigDirectory = Path.Combine(Directories.AppDataFolder, "Configs");
        if (!Directory.Exists(deprecatedConfigDirectory))
        {
            return;
        }

        var defaultConfigNames = Directory
            .GetDirectories(Directories.ConfigPath)
            .Select(x => new DirectoryInfo(x).Name)
            .ToList();

        var toMigrateConfigs = Directory
            .GetDirectories(deprecatedConfigDirectory)
            .Select(x => new DirectoryInfo(x).Name)
            .Where(x => !defaultConfigNames.Contains(x) && !DeprecatedConfigProfiles.Contains(x))
            .ToList();

        foreach (var configName in toMigrateConfigs)
        {
            var oldPath = Path.Combine(deprecatedConfigDirectory, configName);
            var newPath = Path.Combine(Directories.UserConfigPath, configName);
            Directory.Move(oldPath, newPath);
            _logger?.LogInformation("Migrated user config {ConfigName} from {Old} to {New}", configName, oldPath, newPath);
        }

        CopySchemaFolder();
    }

    public void LoadConfigProfiles()
    {
        var newProfiles = new List<ConfigProfileDetails>();

        if (Directory.Exists(Directories.ConfigPath))
        {
            newProfiles.AddRange(Directory
                .GetDirectories(Directories.ConfigPath)
                .Select(GetConfigProfile)
                .NonNull());
        }

        if (Directory.Exists(Directories.UserConfigPath))
        {
            var currentConfigNames = newProfiles.Select(x => x.Name).ToHashSet();
            newProfiles.AddRange(Directory
                .GetDirectories(Directories.UserConfigPath)
                .Select(GetConfigProfile)
                .NonNull()
                .Where(x => !currentConfigNames.Contains(x.Name)));
        }

        _profiles = newProfiles;
        _moods = _profiles.SelectMany(x => x.Moods ?? []).ToHashSet();
    }

    public ConfigProfileDetails? GetConfigProfile(string folder)
    {
        var name = new FileInfo(folder).Name;

        if (name == "Templates")
        {
            return null;
        }

        var files = Directory.GetFiles(folder, "*.yml");

        var moods = new HashSet<string>();
        foreach (var file in files)
        {
            var filename = new FileInfo(file).Name;
            var match = ProfileFileNameWithMoodRegex().Match(filename);
            var mood = "";
            if (match.Success)
            {
                mood = match.Groups["mood"].Value;
                moods.Add(mood);
            }
        }

        return new ConfigProfileDetails(folder, moods);
    }

    /// <summary>
    /// Loads the maps configuration.
    /// </summary>
    /// <returns>A new <see cref="TrackerMapConfig"/> object.</returns>
    public virtual TrackerMapConfig GetMapConfig()
        => LoadJsonConfig<TrackerMapConfig>("maps.json");

    /// <summary>
    /// Loads the specified configuration instance from the specified
    /// filename, falling back to the embedded resource with the same name.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load.</typeparam>
    /// <param name="fileName">The file name to load from.</param>
    /// <returns>The configuration.</returns>
    /// <exception cref="InvalidOperationException">
    /// An unknown error occurred while deserializing the JSON file.
    /// </exception>
    protected virtual T LoadJsonConfig<T>(string fileName)
    {
#if DEBUG
        return GetBuiltInConfig<T>(fileName);
#else
            var jsonPath = Path.Combine(Directories.ConfigPath, fileName);
            if (!File.Exists(jsonPath))
            {
                _logger?.LogWarning("Could not find configuration file '{path}'. Falling back to embedded resource.", jsonPath);
                return GetBuiltInConfig<T>(fileName);
            }

            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<T>(json, s_options)
                ?? throw new InvalidOperationException($"The '{jsonPath}' configuration could not be loaded.");
#endif
    }

    /// <summary>
    /// Returns the configs with additional information for bosses
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual BossConfig GetBossConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<BossConfig, BossInfo>("bosses.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with additional information for items
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual ItemConfig GetItemConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<ItemConfig, ItemData>("items.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with additional information for locations
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual LocationConfig GetLocationConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<LocationConfig, LocationInfo>("locations.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with additional information for regions
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual RegionConfig GetRegionConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<RegionConfig, RegionInfo>("regions.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with additional requests
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual RequestConfig GetRequestConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<RequestConfig, BasicVoiceRequest>("requests.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with tracker responses
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual ResponseConfig GetResponseConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<ResponseConfig, ResponseConfig>("responses.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with additional information for rooms
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual RoomConfig GetRoomConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<RoomConfig, RoomInfo>("rooms.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with additional information for rewards
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual RewardConfig GetRewardConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<RewardConfig, RewardInfo>("rewards.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with UI layouts
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual UIConfig GetUIConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<UIConfig, UILayout>("ui.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with in game text
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual GameLinesConfig GetGameConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<GameLinesConfig, GameLinesConfig>("game.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with msus
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual MsuConfig GetMsuConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<MsuConfig, MsuConfig>("msu.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with hint tiles
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual HintTileConfig GetHintTileConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<HintTileConfig, HintTileConfig>("hint_tiles.yml", profiles, mood);

    /// <summary>
    /// Returns the configs with misc metadata and other configs
    /// </summary>
    /// <param name="profiles">The selected tracker profile(s) to load</param>
    /// <param name="mood">The current tracker mood to pick the specific mood file</param>
    /// <returns></returns>
    public virtual MetadataConfig GetMetadataConfig(IReadOnlyCollection<string> profiles, string? mood) =>
        LoadYamlConfigs<MetadataConfig, MetadataConfig>("metadata.yml", profiles, mood);

    /// <summary>
    /// Returns a collection of all possible config profiles to
    /// select from
    /// </summary>
    /// <returns></returns>
    public virtual ICollection<string> GetAvailableProfiles()
    {
        return _profiles.Select(x => x.Name).ToList();
    }

    /// <summary>
    /// Returns a list of the moods available in the selected profiles.
    /// </summary>
    /// <param name="profiles">The names of profiles to query.</param>
    /// <returns>A list of names of moods that are available in the selected profiles.</returns>
    public virtual IReadOnlyList<string> GetAvailableMoods(IReadOnlyCollection<string> profiles)
    {
        return _profiles.Where(x => profiles.Contains(x.Name))
            .SelectMany(x => x.Moods ?? [])
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .ToImmutableList();
    }

    /// <summary>
    /// Copies the schema folder from the default config folder to the user's config folder
    /// </summary>
    public void CopySchemaFolder()
    {
        var defaultSchemaFolder = Path.Combine(Directories.DefaultDataPath, "Schemas");

        if (!Directory.Exists(defaultSchemaFolder))
        {
            return;
        }

        var userSchemaFolder = Path.Combine(Directories.UserDataPath, "Schemas");

        if (Directory.Exists(userSchemaFolder))
        {
            Directory.Delete(userSchemaFolder, true);
        }

        Directory.CreateDirectory(userSchemaFolder);

        foreach (var file in Directory.EnumerateFiles(defaultSchemaFolder))
        {
            var fileName = new FileInfo(file).Name;
            File.Copy(file, Path.Combine(userSchemaFolder, fileName));
        }
    }

    public IImmutableList<string> GetConfigSpritePaths(List<string?> profiles)
    {
        return _profiles
            .Where(x => x.HasSprites && profiles.Contains(x.Name))
            .OrderBy(x => profiles.IndexOf(x.Name))
            .Select(x => x.SpritePath)
            .NonNull()
            .ToImmutableList();
    }

    private T LoadYamlConfigs<T, T2>(string fileName, IReadOnlyCollection<string>? profiles = null, string? mood = null) where T : new()
    {
        T? config = new();
        var defaultMethod = typeof(T).GetMethod("Default");
        if (defaultMethod != null)
        {
            config = (T?)defaultMethod.Invoke(null, null);
        }

        if (config == null)
        {
            throw new InvalidOperationException($"Could not create default instance of '{{typeof(T).Name}}'");
        }

        var mergeableConfig = (IMergeable<T2>)config
                              ?? throw new InvalidOperationException($"The class '{typeof(T).Name}' does not implement IMergeable.");

        TryMergeYamlFile(fileName, "Default");

        if (profiles == null || profiles.Count == 0) return config;

        foreach (var profile in profiles)
        {
            if (!string.IsNullOrEmpty(profile))
            {
                TryMergeYamlFile(fileName, profile);

                var moodFileName = GetMoodFileName(fileName, mood);
                if (moodFileName != null)
                {
                    TryMergeYamlFile(moodFileName, profile);
                }
            }
        }

        return config;

        void TryMergeYamlFile(string file, string profile)
        {
            var otherConfig = LoadYamlFile<T>(file, profile);
            if (otherConfig == null)
                return;

            var otherMergeableConfig = (IMergeable<T2>)otherConfig;
            if (otherConfig == null)
                throw new InvalidOperationException($"The class '{typeof(T).Name}' does not implement IMergeable.");

            mergeableConfig.MergeFrom(otherMergeableConfig);
        }
    }

    private string? GetMoodFileName(string fileName, string? mood)
    {
        if (string.IsNullOrEmpty(mood))
            return null;

        var extension = Path.GetExtension(fileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        return $"{fileNameWithoutExtension}.{mood}{extension}";
    }

    private T? LoadYamlFile<T>(string fileName, string profile)
    {
        var profileConfig = _profiles.FirstOrDefault(x => x.Name == profile);
        if (profileConfig == null)
        {
            return default;
        }

        var path = Path.Combine(profileConfig.ConfigFolderPath, fileName);
        string yml;
        if (!File.Exists(path))
        {
            return default;
        }
        else
        {
            yml = File.ReadAllText(path);
        }
        T? obj;
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            obj = deserializer.Deserialize<T>(yml);
        }
        catch (Exception ex) when (ex is YamlDotNet.Core.SemanticErrorException or YamlDotNet.Core.YamlException)
        {
            _logger?.LogError(ex, "Unable to load config file {Path}", path);
            throw new YamlDotNet.Core.SemanticErrorException("Unable to load config file " + path, ex);
        }

        if (obj is IHasPostLoadFunction loadFunction)
        {
            loadFunction.OnPostLoad();
        }

        _logger?.LogInformation("Loaded config file {Path}", path);

        return obj;
    }

    private static T GetBuiltInConfig<T>(string fileName)
    {
        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("TrackerCouncil.Smz3.Data." + fileName);
        if (stream == null)
            throw new FileNotFoundException($"Could not find embedded stream in Randomizer.SMZ3.Tracking for '{fileName}'.");

        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        return JsonSerializer.Deserialize<T>(reader.ReadToEnd(), s_options)
               ?? throw new InvalidOperationException("The embedded tracker configuration could not be loaded.");
    }

    [GeneratedRegex("[^\\.]+\\.(?<mood>.+)\\.yml")]
    private static partial Regex ProfileFileNameWithMoodRegex();
}
