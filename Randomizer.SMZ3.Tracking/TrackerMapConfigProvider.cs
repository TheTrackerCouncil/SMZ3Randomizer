using System;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides tracker map configuration data.
    /// </summary>
    public class TrackerMapConfigProvider
    {
        private static readonly JsonSerializerOptions s_options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private readonly string _jsonPath;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="TrackerMapConfigProvider"/> class using the specified JSON file
        /// name.
        /// </summary>
        /// <param name="jsonPath">
        /// The path to the JSON configuration file to load.
        /// </param>
        public TrackerMapConfigProvider(string jsonPath)
        {
            _jsonPath = jsonPath;
        }

        /// <summary>
        /// Loads the tracker map configuration.
        /// </summary>
        /// <returns>A new <see cref="TrackerMapConfig"/> object.</returns>
        public virtual TrackerMapConfig GetTrackerMapConfig()
        {
#if DEBUG
            return GetBuiltInConfig();
#else
            if (string.IsNullOrEmpty(_jsonPath) || !File.Exists(_jsonPath))
                return GetBuiltInConfig();

            var json = File.ReadAllBytes(_jsonPath);
            return JsonSerializer.Deserialize<TrackerMapConfig>(json, s_options)
                ?? throw new InvalidOperationException("The tracker configuration could not be loaded.");
#endif
        }

        /// <summary>
        /// Loads a copy of the config from the Randomizer.SMZ3.Tracking project when debugging
        /// </summary>
        /// <returns>A new <see cref="TrackerMapConfig"/> object.</returns>
        private TrackerMapConfig GetBuiltInConfig()
        {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Randomizer.SMZ3.Tracking.maps.json");
            if (stream == null)
                throw new FileNotFoundException("Could not find embedded stream for tracker.json");

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            if (!string.IsNullOrEmpty(_jsonPath))
                File.WriteAllText(_jsonPath, json);
            return JsonSerializer.Deserialize<TrackerMapConfig>(json, s_options)
                ?? throw new InvalidOperationException("The embedded tracker configuration could not be loaded.");
        }
    }
}
