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

        public static string GetFilePath()
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SMZ3CasRandomizer");
            Directory.CreateDirectory(basePath);
            return Path.Combine(basePath, "options.json");
        }

        public RandomizerOptions Create()
        {
            if (_options != null) return _options;

            var optionsPath = GetFilePath();

            try
            {
                if (!File.Exists(optionsPath))
                {
                    _logger.LogInformation("Options file '{path}' does not exist, using default options", optionsPath);
                    _options = new RandomizerOptions();
                    _options.FilePath = optionsPath;
                    return _options;
                }

                _options = RandomizerOptions.Load(optionsPath);
                return _options;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load '{path}', using default options", optionsPath);
                _options = new RandomizerOptions();
                _options.FilePath = optionsPath;
                return _options;
            }
        }
    }
}
