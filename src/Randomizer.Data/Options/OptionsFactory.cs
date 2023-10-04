using System;
using System.IO;

using Microsoft.Extensions.Logging;

namespace Randomizer.Data.Options
{
    public class OptionsFactory
    {
        private readonly ILogger<OptionsFactory> _logger;
        private RandomizerOptions? _options;

        public OptionsFactory(ILogger<OptionsFactory> logger)
        {
            _logger = logger;
        }

        public static string GetFileFolder()
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SMZ3CasRandomizer");
            Directory.CreateDirectory(basePath);
            return basePath;
        }

        public static string GetFilePath()
        {
#if DEBUG
            return Path.Combine(GetFileFolder(), "randomizer-options-debug.yml");
#else
            return Path.Combine(GetFileFolder(), "randomizer-options.yml");
#endif
        }

        public RandomizerOptions Create()
        {
            if (_options != null) return _options;

            var yamlFilePath = GetFilePath();
            var jsonFilePath = Path.Combine(GetFileFolder(), "options.json");

            if (File.Exists(yamlFilePath))
            {
                _logger.LogInformation("YAML options file {Path} found", yamlFilePath);
                return LoadFromFile(yamlFilePath, yamlFilePath, true);
            }
            else if (File.Exists(jsonFilePath))
            {
                _logger.LogInformation("JSON options file {Path} found", jsonFilePath);
                return LoadFromFile(jsonFilePath, yamlFilePath, false);
            }
            else
            {
                _logger.LogInformation("Options file '{Path}' does not exist, using default options", yamlFilePath);
                _options = new RandomizerOptions();
                _options.FilePath = yamlFilePath;
                return _options;
            }
        }

        public RandomizerOptions LoadFromFile(string loadPath, string savePath, bool isYaml)
        {
            if (!File.Exists(loadPath))
            {
                _logger.LogInformation("Options file '{Path}' does not exist, using default options", loadPath);
                _options = new RandomizerOptions();
                _options.FilePath = savePath;
                return _options;
            }

            try
            {
                _options = RandomizerOptions.Load(loadPath, savePath, isYaml);
                _logger.LogInformation("Loaded options from file '{Path}'", loadPath);
                return _options;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load '{Path}', using default options", loadPath);
                _options = new RandomizerOptions();
                _options.FilePath = savePath;
                return _options;
            }
        }
    }
}
