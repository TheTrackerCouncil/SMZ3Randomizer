using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Options;
using Randomizer.SMZ3.Generation;

namespace Randomizer.PatchBuilder;

public class PatchBuilderService
{
    private readonly ILogger<PatchBuilderService> _logger;
    private readonly RomGenerationService _romGenerationService;
    private readonly OptionsFactory _optionsFactory;
    private readonly string _solutionPath;
    private readonly string _randomizerRomPath;

    public PatchBuilderService(ILogger<PatchBuilderService> logger, RomGenerationService romGenerationService, OptionsFactory optionsFactory)
    {
        _logger = logger;
        _romGenerationService = romGenerationService;
        _solutionPath = SolutionPath;
        _randomizerRomPath = Path.Combine(_solutionPath, "alttp_sm_combo_randomizer_rom");
        _optionsFactory = optionsFactory;
    }

    public void CreatePatches(PatchBuilderConfig config)
    {
        _logger.LogInformation("CreatePatches Start");
        try
        {
            var options = _optionsFactory.Create();
            options.PatchOptions = config.PatchOptions;

            BuildPatches(config);
            CopyPatches(config);
            var romPath = GenerateTestRom(config);
            Launch(config, romPath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with CreatePatches");
        }

    }

    private void BuildPatches(PatchBuilderConfig config)
    {
        if (!config.PatchFlags.CreatePatches) return;

        _logger.LogInformation("Building patches");
        var scriptFile = config.EnvironmentSettings.PatchBuildScriptPath;
        if (string.IsNullOrEmpty(scriptFile))
        {
            scriptFile = Path.Combine(_randomizerRomPath, OperatingSystem.IsWindows() ? "build.bat" : "build.sh");
        }

        var info = new FileInfo(scriptFile);

        // Run the patch build.bat file
        var process = Process.Start(new ProcessStartInfo()
        {
            FileName = info.Name,
            WorkingDirectory = info.DirectoryName,
            UseShellExecute = true
        });
        if (process == null)
        {
            throw new NullReferenceException("Unable to create build process");
        }
        process.WaitForExit();
        process.Close();

        // Verify that the main IPS file was created
        info = new FileInfo(Path.Combine(_randomizerRomPath, "build", "zsm.ips"));

        var length = info.Exists ? info.Length : 0L;
        if (length < 1000)
        {
            _logger.LogError("Invalid zsm.ips file found out {Path}", info.FullName);
            throw new InvalidOperationException("Invalid zsm.ips file created");
        }
    }

    private void CopyPatches(PatchBuilderConfig config)
    {
        if (!config.PatchFlags.CopyPatchesToProject) return;
        var patchBuildFolder = new DirectoryInfo(Path.Combine(_randomizerRomPath, "build"));
        foreach (var file in patchBuildFolder.EnumerateFiles().Where(x => x.Extension.Contains("ips")))
        {
            var destinationFile = new FileInfo(Path.Combine(_solutionPath, "src", "Randomizer.SMZ3", "FileData", "IpsPatches", file.Name));
            file.CopyTo(destinationFile.FullName, true);
            _logger.LogInformation("Copying {Source} to {Destination}", file, destinationFile.FullName);
        }
    }

    private string? GenerateTestRom(PatchBuilderConfig config)
    {
        if (!config.PatchFlags.GenerateTestRom) return null;
        var smRom = new FileInfo(string.IsNullOrEmpty(config.EnvironmentSettings.MetroidRomPath)
            ? Path.Combine(_randomizerRomPath, "resources", "sm.sfc")
            : config.EnvironmentSettings.MetroidRomPath);
        var z3Rom = new FileInfo(string.IsNullOrEmpty(config.EnvironmentSettings.Z3RomPath)
            ? Path.Combine(_randomizerRomPath, "resources", "z3.sfc")
            : config.EnvironmentSettings.Z3RomPath);

        if (!smRom.Exists || !z3Rom.Exists)
        {
            throw new FileNotFoundException("Super Metroid or Zelda rom not found");
        }

        var outputFolder = config.EnvironmentSettings.OutputPath;
        if (string.IsNullOrEmpty(outputFolder))
        {
            outputFolder = Path.Combine(_randomizerRomPath, "build");
        }

        var outputFileName = config.EnvironmentSettings.TestRomFileName;
        if (string.IsNullOrEmpty(outputFileName))
        {
            outputFileName = "test-rom";
        }

        var randomizerOptions = new RandomizerOptions()
        {
            GeneralOptions =
            {
                SMRomPath = smRom.FullName,
                Z3RomPath = z3Rom.FullName
            },
            PatchOptions = config.PatchOptions
        };

        var seedData = _romGenerationService.GeneratePlandoSeed(randomizerOptions, config.PlandoConfig);
        var bytes = _romGenerationService.GenerateRomBytes(randomizerOptions, seedData);

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        var romPath = Path.Combine(outputFolder, $"{outputFileName}.sfc");
        File.WriteAllBytes(romPath, bytes);

        _logger.LogInformation("Wrote test rom to {Path}", romPath);

        return romPath;
    }

    private void Launch(PatchBuilderConfig config, string? romPath)
    {
        if (!config.PatchFlags.LaunchTestRom) return;
        if (string.IsNullOrEmpty(romPath))
        {
            _logger.LogError("No rom path specified. GenerateTestRom: true is required");
            return;
        }
        var launchApplication = config.EnvironmentSettings.LaunchApplication;
        var launchArguments = "";
        if (string.IsNullOrEmpty(launchApplication))
        {
            launchApplication = romPath;
        }
        else
        {
            if (string.IsNullOrEmpty(config.EnvironmentSettings.LaunchArguments))
            {
                launchArguments = $"\"{romPath}\"";
            }
            else if (config.EnvironmentSettings.LaunchArguments.Contains("%rom%"))
            {
                launchArguments = config.EnvironmentSettings.LaunchArguments.Replace("%rom%", $"{romPath}");
            }
            else
            {
                launchArguments = $"{config.EnvironmentSettings.LaunchArguments} \"{romPath}\"";
            }
        }

        try
        {
            _logger.LogInformation("Executing {FileName} {Arguments}", launchApplication, launchArguments);
            Process.Start(new ProcessStartInfo
            {
                FileName = launchApplication,
                Arguments = launchArguments,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to launch rom");
        }
    }

    private static string SolutionPath
    {
        get
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return Path.Combine(directory!.FullName);
        }
    }

}
