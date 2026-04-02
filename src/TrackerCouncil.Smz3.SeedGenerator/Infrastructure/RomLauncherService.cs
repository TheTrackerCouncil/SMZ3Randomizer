using System.Diagnostics;
using System.IO;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.Infrastructure;

public class RomLauncherService
{
    private readonly RandomizerOptions _options;

    public RomLauncherService(OptionsFactory optionsFactory)
    {
        _options = optionsFactory.Create();
    }

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
        else if (string.IsNullOrEmpty(launchArguments))
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
        else if (launchArguments.Contains("%rom%"))
        {
            launchArguments = launchArguments.Replace("%rom%", $"{romPath}");
        }

        return Process.Start(new ProcessStartInfo
        {
            FileName = launchApplication,
            Arguments = launchArguments,
            UseShellExecute = true,
        });
    }
}
