using System;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Randomizer.SMZ3.Tracking.Configuration.Providers
{
    /// <summary>
    /// Provides tracker configuration data from the embedded resources.
    /// </summary>
    public class EmbeddedJsonConfigProvider : IConfigProvider
    {
        private static readonly JsonSerializerOptions s_options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="EmbeddedJsonConfigProvider"/> class.
        /// </summary>
        public EmbeddedJsonConfigProvider()
        {
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
            return GetBuiltInConfig<T>(fileName);
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
    }
}
