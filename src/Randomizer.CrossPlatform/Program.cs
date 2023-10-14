using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Infrastructure;
using Serilog;

namespace Randomizer.CrossPlatform;

public static class Program
{
    private static ServiceProvider s_services = null!;

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(LogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
            .CreateLogger();

        s_services = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.AddSerilog(dispose: true);
            })
            .ConfigureServices()
            .BuildServiceProvider();

        InitializeMsuRandomizer(s_services.GetRequiredService<IMsuRandomizerInitializationService>());

        var optionsFile = new FileInfo("randomizer-options.yml");
        var randomizerOptions = s_services.GetRequiredService<OptionsFactory>().LoadFromFile(optionsFile.FullName, optionsFile.FullName, true);
        randomizerOptions.Save();
        var launcher = s_services.GetRequiredService<RomLauncherService>();

        if (!ValidateRandomizerOptions(randomizerOptions))
        {
            return;
        }

        Console.WriteLine($"Using Randomizer Options file at {optionsFile.FullName}");

        var result = DisplayMenu("What do you want to do?", new List<string>()
        {
            "Generate & Play a Rom",
            "Play a Rom",
            "Delete Rom(s)"
        });

        if (result == null)
        {
            return;
        }

        // Generates a new rom
        if (result.Value.Item1 == 0)
        {
            // Allow the user to select an MSU
            if (!string.IsNullOrEmpty(randomizerOptions.GeneralOptions.MsuPath))
            {
                var msus = s_services.GetRequiredService<IMsuLookupService>().LookupMsus(randomizerOptions.GeneralOptions.MsuPath)
                    .Where(x => x.ValidTracks.Count > 10).ToList();

                result = DisplayMenu("Which msu do you want to use?",
                    msus.Select(x => $"{x.Name} ({x.MsuTypeName})").ToList());
                if (result != null)
                {
                    randomizerOptions.PatchOptions.MsuPaths = new List<string>() { msus[result.Value.Item1].Path };
                }
            }

            // Generate the rom
            var results = s_services.GetRequiredService<RomGenerationService>().GenerateRandomRomAsync(randomizerOptions).Result;
            if (string.IsNullOrEmpty(results.GenerationError))
            {
                var romPath = Path.Combine(randomizerOptions.RomOutputPath, results.Rom!.RomPath);
                Console.WriteLine($"Rom generated successfully: {romPath}");
                launcher.LaunchRom(romPath, randomizerOptions.GeneralOptions.LaunchApplication,
                    randomizerOptions.GeneralOptions.LaunchArguments);
                _ = s_services.GetRequiredService<ConsoleTrackerDisplayService>().StartTracking(results.Rom, romPath);
            }
            else
            {
                Console.WriteLine($"Error generating rom: {results.GenerationError}");
            }
        }
        // Plays a previously generated rom
        else if (result.Value.Item1 == 1)
        {
            var roms = s_services.GetRequiredService<IGameDbService>().GetGeneratedRomsList()
                .OrderByDescending(x => x.Id)
                .ToList();

            result = DisplayMenu("Which rom do you want to play?",
                roms.Select(x => $"{x.Seed} - {x.Date:G}").ToList());

            if (result != null)
            {
                var selectedRom = roms[result.Value.Item1];
                var romPath = Path.Combine(randomizerOptions.RomOutputPath, selectedRom.RomPath);
                launcher.LaunchRom(romPath, randomizerOptions.GeneralOptions.LaunchApplication,
                    randomizerOptions.GeneralOptions.LaunchArguments);
                _ = s_services.GetRequiredService<ConsoleTrackerDisplayService>().StartTracking(selectedRom, romPath);
            }

        }
        // Deletes rom(s)
        else if (result.Value.Item1 == 2)
        {
            var dbService = s_services.GetRequiredService<IGameDbService>();
            var roms = dbService.GetGeneratedRomsList()
                .OrderByDescending(x => x.Id)
                .ToList();

            while (roms.Count > 0)
            {
                result = DisplayMenu("Which rom do you want to delete?",
                    roms.Select(x => $"{x.Seed} - {x.Date:G}").ToList());

                if (result == null)
                {
                    break;
                }
                else
                {
                    var selectedRom = roms[result.Value.Item1];
                    if (!dbService.DeleteGeneratedRom(selectedRom, out var error))
                    {
                        Log.Error("Could not delete rom: {Message}", error);
                    }
                    else
                    {
                        roms.Remove(selectedRom);
                    }
                }
            }
        }
    }

    private static bool ValidateRandomizerOptions(RandomizerOptions options)
    {
        var sourceRomValidationService = s_services.GetRequiredService<SourceRomValidationService>();

        if (!sourceRomValidationService.ValidateZeldaRom(options.GeneralOptions.Z3RomPath))
        {
            Console.WriteLine("Missing Z3RomPath. Please enter a valid A Link to the Past Japanese v1.0 ROM");
            return false;
        }

        if (!sourceRomValidationService.ValidateMetroidRom(options.GeneralOptions.SMRomPath))
        {
            Console.WriteLine("Missing SMRomPath. Please enter a valid Super Metroid Japanese/US ROM");
            return false;
        }

        return true;
    }

    private static (int, string)? DisplayMenu(string prompt, List<string> options)
    {
        Console.WriteLine(prompt);

        for (var i = 0; i < options.Count; i++)
        {
            Console.WriteLine($"  {i+1}) {options[i]}");
        }

        while (true)
        {
            Console.Write($"Enter a value (1-{options.Count}): ");
            var selectedOption = Console.ReadLine();
            if (string.IsNullOrEmpty(selectedOption))
            {
                return null;
            }

            if (int.TryParse(selectedOption, out var value) && value >= 1 && value <= options.Count)
            {
                return (value - 1, options[value - 1]);
            }
        }

    }

    private static void InitializeMsuRandomizer(IMsuRandomizerInitializationService msuRandomizerInitializationService)
    {
        var settingsStream =  Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Randomizer.CrossPlatform.msu-randomizer-settings.yml");
        var typesStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Randomizer.CrossPlatform.msu-randomizer-types.json");
        var msuInitializationRequest = new MsuRandomizerInitializationRequest()
        {
            MsuAppSettingsStream = settingsStream,
            MsuTypeConfigStream = typesStream,
            LookupMsus = true
        };
        msuRandomizerInitializationService.Initialize(msuInitializationRequest);
    }

#if DEBUG
    private static string LogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "smz3-cas-debug_.log");
#else
    private static string LogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "smz3-cas.log");
#endif
}
