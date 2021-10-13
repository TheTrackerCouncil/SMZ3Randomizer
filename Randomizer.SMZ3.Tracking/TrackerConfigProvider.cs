using System;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Randomizer.SMZ3.Tracking
{
    public class TrackerConfigProvider
    {
        private static readonly JsonSerializerOptions s_options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private readonly string _jsonPath;

        public TrackerConfigProvider(string jsonPath)
        {
            _jsonPath = jsonPath;
        }

        public virtual TrackerConfig GetTrackerConfig()
        {
#if DEBUG
            return GetBuiltInConfig();
#endif
            if (!File.Exists(_jsonPath))
                return GetBuiltInConfig();

            var json = File.ReadAllBytes(_jsonPath);
            return JsonSerializer.Deserialize<TrackerConfig>(json, s_options)
                ?? throw new InvalidOperationException("The tracker configuration could not be loaded.");
        }

        private TrackerConfig GetBuiltInConfig()
        {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Randomizer.SMZ3.Tracking.tracker.json");
            if (stream == null)
                throw new Exception("Could not find embedded stream for tracker.json");

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            File.WriteAllText(_jsonPath, json);
            return JsonSerializer.Deserialize<TrackerConfig>(json, s_options)
                ?? throw new InvalidOperationException("The embedded tracker configuration could not be loaded.");
        }
    }
}
