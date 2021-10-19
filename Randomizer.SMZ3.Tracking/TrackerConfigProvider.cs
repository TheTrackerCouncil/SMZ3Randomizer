using System;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Randomizer.SMZ3.Tracking
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

        private readonly string _jsonPath;

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="TrackerConfigProvider"/> class using the specified JSON file
        /// name.
        /// </summary>
        /// <param name="jsonPath">
        /// The path to the JSON configuration file to load.
        /// </param>
        public TrackerConfigProvider(string jsonPath)
        {
            _jsonPath = jsonPath;
        }

        /// <summary>
        /// Loads the tracker configuration.
        /// </summary>
        /// <returns>A new <see cref="TrackerConfig"/> object.</returns>
        public virtual TrackerConfig GetTrackerConfig()
        {
#if DEBUG
            return GetBuiltInConfig();
#else
            if (string.IsNullOrEmpty(_jsonPath) || !File.Exists(_jsonPath))
                return GetBuiltInConfig();

            var json = File.ReadAllBytes(_jsonPath);
            return JsonSerializer.Deserialize<TrackerConfig>(json, s_options)
                ?? throw new InvalidOperationException("The tracker configuration could not be loaded.");
#endif
        }

        private TrackerConfig GetBuiltInConfig()
        {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Randomizer.SMZ3.Tracking.tracker.json");
            if (stream == null)
                throw new FileNotFoundException("Could not find embedded stream for tracker.json");

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            if (!string.IsNullOrEmpty(_jsonPath))
                File.WriteAllText(_jsonPath, json);
            return JsonSerializer.Deserialize<TrackerConfig>(json, s_options)
                ?? throw new InvalidOperationException("The embedded tracker configuration could not be loaded.");
        }
    }
}
