using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Randomizer.SMZ3.Tracking.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Provides tracker configuration data.
    /// </summary>
    public class TrackerConfigProvider
    {
        private static readonly JsonSerializerOptions s_options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private static readonly IDeserializer s_deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();

        private readonly string _basePath;
        private readonly ILogger<TrackerConfigProvider>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="TrackerConfigProvider"/> class.
        /// </summary>
        public TrackerConfigProvider(ILogger<TrackerConfigProvider>? logger)
        {
#if DEBUG
            _basePath = Path.Combine(GetSourceDirectory(), "Randomizer.SMZ3.Tracking", "Configuration", "Yaml");
#else
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "Configs");
#endif
            _logger = logger;
        }

        /// <summary>
        /// Loads the tracker configuration.
        /// </summary>
        /// <returns>A new <see cref="TrackerConfig"/> object.</returns>
        public virtual TrackerConfig GetTrackerConfig()
            => LoadConfig<TrackerConfig>("tracker.json");

        /// <summary>
        /// Loads the locations configuration.
        /// </summary>
        /// <returns>A new <see cref="LocationConfig"/> object.</returns>
        public virtual LocationConfig GetLocationConfig()
            => LoadConfig<LocationConfig>("locations.json");

        /// <summary>
        /// Loads the maps configuration.
        /// </summary>
        /// <returns>A new <see cref="TrackerMapConfig"/> object.</returns>
        public virtual TrackerMapConfig GetMapConfig()
            => LoadConfig<TrackerMapConfig>("maps.json");

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
        protected virtual T LoadConfig<T>(string fileName)
        {
            var profiles = GetAvailableProfiles();
#if DEBUG
            return GetBuiltInConfig<T>(fileName);
#else
            var jsonPath = Path.Combine(_basePath, fileName);
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

        public virtual BossConfig GetBossConfig(ICollection<string>? profiles = null) =>
            LoadYamlConfigs<BossConfig, BossInfo>("bosses.yml", profiles);

        public virtual DungeonConfig GetDungeonConfig(ICollection<string>? profiles = null) =>
            LoadYamlConfigs<DungeonConfig, DungeonInfo>("dungeons.yml", profiles);

        public virtual ItemConfig GetItemConfig(ICollection<string>? profiles = null) =>
            LoadYamlConfigs<ItemConfig, ItemData>("items.yml", profiles);

        public virtual ConfigFiles.LocationConfig GetLocationsConfig(ICollection<string>? profiles = null) =>
            LoadYamlConfigs<ConfigFiles.LocationConfig, LocationInfo>("locations.yml", profiles);

        public virtual RegionConfig GetRegionConfig(ICollection<string>? profiles = null) =>
            LoadYamlConfigs<RegionConfig, RegionInfo>("regions.yml", profiles);

        public virtual RequestConfig GetRequestConfig(ICollection<string>? profiles = null) =>
            LoadYamlConfigs<RequestConfig, BasicVoiceRequest>("requests.yml", profiles);

        public virtual ResponseConfig GetResponseConfig(ICollection<string>? profiles = null) =>
            LoadYamlConfigs<ResponseConfig, ResponseConfig>("responses.yml", profiles);

        public virtual RoomConfig GetRoomConfig(ICollection<string>? profiles = null) =>
            LoadYamlConfigs<RoomConfig, RoomInfo>("rooms.yml", profiles);

        public virtual ICollection<string> GetAvailableProfiles()
        {
            return Directory.GetDirectories(_basePath);
        }

        private T LoadYamlConfigs<T, T2>(string fileName, ICollection<string>? profiles = null) where T : new()
        {
            var defaultMethod = typeof(T).GetMethod("Default");
            if (defaultMethod == null)
            {
                throw new InvalidOperationException($"The class '{typeof(T).Name}' does not implement IConfigFile.");
            }
            var config = (T)(defaultMethod.Invoke(null, null) ?? new T());
            if (config == null)
            {
                throw new InvalidOperationException($"The class '{typeof(T).Name}' does not implement IConfigFile.");
            }
            
            var mergeableConfig = (IMergeable<T2>)config;
            if (mergeableConfig == null)
            {
                throw new InvalidOperationException($"The class '{typeof(T).Name}' does not implement IMergeable.");
            }

            if (profiles == null || profiles.Count == 0) return config;
            
            foreach (var profile in profiles)
            {
                var otherConfig = LoadYamlFile<T>(fileName, profile);
                if (otherConfig == null) continue;
                var otherMergeableConfig = (IMergeable<T2>)otherConfig;
                if (otherConfig == null) throw new InvalidOperationException($"The class '{typeof(T).Name}' does not implement IMergeable.");
                mergeableConfig.MergeFrom(otherMergeableConfig);
            }

            return config;
        }

        private T LoadYamlFile<T>(string fileName, string profile)
        {
            var path = Path.Combine(_basePath, profile, fileName);
            if (!File.Exists(path))
            {
                return default;
            }
            var yml = File.ReadAllText(path);
            var obj = s_deserializer.Deserialize<T>(yml);
            if (obj == null)
            {
                return default;
            }
            return obj;
        }

        private static T GetBuiltInConfig<T>(string fileName)
        {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Randomizer.SMZ3.Tracking." + fileName);
            if (stream == null)
                throw new FileNotFoundException($"Could not find embedded stream in Randomizer.SMZ3.Tracking for '{fileName}'.");

            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
            return JsonSerializer.Deserialize<T>(reader.ReadToEnd(), s_options)
                ?? throw new InvalidOperationException("The embedded tracker configuration could not be loaded.");
        }

        private string GetSourceDirectory()
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            var directory = Directory.GetParent(currentDirectory);
            while (directory != null && directory.Name != "src")
            {
                directory = Directory.GetParent(directory.FullName);
            }
            return directory?.FullName ?? "";
        }
    }
}
