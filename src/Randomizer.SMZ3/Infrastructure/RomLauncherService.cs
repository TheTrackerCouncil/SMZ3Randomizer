using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.SMZ3.Infrastructure;

public class RomLauncherService(OptionsFactory optionsFactory, ILogger<RomLauncherService> logger)
{
    private readonly RandomizerOptions _options = optionsFactory.Create();

    public Process? LaunchRom(GeneratedRom rom)
    {
        var romPath = Path.Combine(_options.RomOutputPath, rom.RomPath);
        return LaunchRom(romPath, _options.GeneralOptions.LaunchApplication, _options.GeneralOptions.LaunchArguments);
    }

    public Process? LaunchRom(string romPath, string? launchApplication, string? launchArguments)
    {
        if (!File.Exists(romPath))
        {
            throw new FileNotFoundException($"{romPath} not found");
        }

        if (string.IsNullOrEmpty(launchApplication))
        {
            launchApplication = romPath;
        }
        else
        {
            if (string.IsNullOrEmpty(_options.GeneralOptions.LaunchArguments))
            {
                launchArguments = $"\"{romPath}\"";
            }
            else if (_options.GeneralOptions.LaunchArguments.Contains("%rom%"))
            {
                launchArguments = _options.GeneralOptions.LaunchArguments.Replace("%rom%", $"{romPath}");
            }
            else
            {
                launchArguments = $"{_options.GeneralOptions.LaunchArguments} \"{romPath}\"";
            }
        }

        logger.LogInformation("{Application} {Arguments}", launchApplication, launchArguments);

        return Process.Start(new ProcessStartInfo
        {
            FileName = launchApplication,
            Arguments = launchArguments,
            UseShellExecute = true,
        });
    }
}
