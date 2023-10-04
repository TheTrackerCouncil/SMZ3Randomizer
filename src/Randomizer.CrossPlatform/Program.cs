using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.SMZ3.Generation;
using Serilog;
using Serilog.Events;

namespace Randomizer.CrossPlatform;

public static class Program
{
    public static void Main(string[] args)
    {
        var logLevel = LogEventLevel.Warning;
        if (args.Any(x => x == "-v"))
            logLevel = LogEventLevel.Information;

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(logLevel)
            .WriteTo.File(LogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
            .CreateLogger();

        var services = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.AddSerilog(dispose: true);
            })
            .ConfigureServices()
            .BuildServiceProvider();

        InitializeMsuRandomizer(services.GetRequiredService<IMsuRandomizerInitializationService>());

        var optionsFile = new FileInfo("randomizer-options.yml");
        var randomizerOptions = services.GetRequiredService<OptionsFactory>().LoadFromFile(optionsFile.FullName, optionsFile.FullName, true);
        randomizerOptions.Save();

        if (string.IsNullOrEmpty(randomizerOptions.GeneralOptions.Z3RomPath) || string.IsNullOrEmpty(randomizerOptions
                                                                                 .GeneralOptions.SMRomPath)
                                                                             || !File.Exists(randomizerOptions
                                                                                 .GeneralOptions.Z3RomPath) ||
                                                                             !File.Exists(randomizerOptions
                                                                                 .GeneralOptions.SMRomPath))
        {
            Console.WriteLine($"Please update {optionsFile.FullName} to include the paths to the Super Metroid and A Link to the Past roms and any other desired options for generation.");
            return;
        }

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
                var msus = services.GetRequiredService<IMsuLookupService>().LookupMsus(randomizerOptions.GeneralOptions.MsuPath)
                    .Where(x => x.ValidTracks.Count > 10).ToList();

                result = DisplayMenu("Which msu do you want to use?",
                    msus.Select(x => $"{x.Name} ({x.MsuTypeName})").ToList());
                if (result != null)
                {
                    randomizerOptions.PatchOptions.MsuPaths = new List<string>() { msus[result.Value.Item1].Path };
                }
            }

            // Generate the rom
            var results = services.GetRequiredService<RomGenerationService>().GenerateRandomRomAsync(randomizerOptions).Result;
            if (string.IsNullOrEmpty(results.GenerationError))
            {
                var romPath = Path.Combine(randomizerOptions.RomOutputPath, results.Rom!.RomPath);
                Console.WriteLine($"Rom generated successfully: {romPath}");
                Launch(romPath, randomizerOptions);
            }
            else
            {
                Console.WriteLine($"Error generating rom: {results.GenerationError}");
            }
        }
        // Plays a previously generated rom
        else if (result.Value.Item1 == 1)
        {
            var roms = services.GetRequiredService<IGameDbService>().GetGeneratedRomsList()
                .OrderByDescending(x => x.Id)
                .ToList();

            result = DisplayMenu("Which rom do you want to play?",
                roms.Select(x => $"{x.Seed} - {x.Date:G}").ToList());

            if (result != null)
            {
                var selectedRom = roms[result.Value.Item1];
                var romPath = Path.Combine(randomizerOptions.RomOutputPath, selectedRom.RomPath);
                Launch(romPath, randomizerOptions);
            }
        }
        // Deletes rom(s)
        else if (result.Value.Item1 == 2)
        {
            var dbService = services.GetRequiredService<IGameDbService>();
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

    static (int, string)? DisplayMenu(string prompt, List<string> options)
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

    static void Launch(string romPath, RandomizerOptions options)
    {
        if (!File.Exists(romPath))
        {
            Log.Error("No test rom found at {Path}", romPath);
            return;
        }

        var launchApplication = options.GeneralOptions.LaunchApplication;
        var launchArguments = "";
        if (string.IsNullOrEmpty(launchApplication))
        {
            launchApplication = romPath;
        }
        else
        {
            if (string.IsNullOrEmpty(options.GeneralOptions.LaunchArguments))
            {
                launchArguments = $"\"{romPath}\"";
            }
            else if (options.GeneralOptions.LaunchArguments.Contains("%rom%"))
            {
                launchArguments = options.GeneralOptions.LaunchArguments.Replace("%rom%", $"{romPath}");
            }
            else
            {
                launchArguments = $"{options.GeneralOptions.LaunchArguments} \"{romPath}\"";
            }
        }

        try
        {
            Console.WriteLine($"Executing {launchApplication} {launchArguments}");
            Log.Information("Executing {FileName} {Arguments}", launchApplication, launchArguments);
            Process.Start(new ProcessStartInfo
            {
                FileName = launchApplication,
                Arguments = launchArguments,
                UseShellExecute = true,
            });
        }
        catch (Exception e)
        {
            Log.Error(e, "Unable to launch rom");
        }
    }

    static void InitializeMsuRandomizer(IMsuRandomizerInitializationService msuRandomizerInitializationService)
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
