using System;
using System.IO;

using Microsoft.Extensions.Logging;

using Randomizer.App.ViewModels;

namespace Randomizer.App
{
    public class OptionsFactory
    {
        private readonly ILogger<OptionsFactory> _logger;

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
            var optionsPath = GetFilePath();

            try
            {
                if (!File.Exists(optionsPath))
                {
                    _logger.LogInformation("Options file '{path}' does not exist, using default options", optionsPath);
                    return new RandomizerOptions();
                }

                return RandomizerOptions.Load(optionsPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to load '{path}', using default options", optionsPath);
                return new RandomizerOptions();
            }
        }
    }
}
