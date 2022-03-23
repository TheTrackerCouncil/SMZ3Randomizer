using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Tracking.Configuration.Providers
{
    /// <summary>
    /// Provides tracker configuration data from the file system.
    /// </summary>
    public class FileJsonConfigProvider : IConfigProvider
    {
        private static readonly JsonSerializerOptions s_options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private readonly string _basePath;
        private readonly ILogger<FileJsonConfigProvider>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="FileJsonConfigProvider"/> class.
        /// </summary>
        public FileJsonConfigProvider(ILogger<FileJsonConfigProvider>? logger)
        {
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer"); ;
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
            var jsonPath = Path.Combine(_basePath, fileName);
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"The '{jsonPath}' configuration file does not exist.");
            }

            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<T>(json, s_options)
                ?? throw new InvalidOperationException($"The '{jsonPath}' configuration could not be loaded.");
        }
    }
}
