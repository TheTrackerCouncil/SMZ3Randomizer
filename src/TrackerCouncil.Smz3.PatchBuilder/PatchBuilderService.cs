using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Data.Interfaces;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;

namespace TrackerCouncil.Smz3.PatchBuilder;

public class PatchBuilderService
{
    private readonly ILogger<PatchBuilderService> _logger;
    private readonly IRomGenerationService _romGenerationService;
    private readonly OptionsFactory _optionsFactory;
    private readonly IMsuTypeService _msuTypeService;
    private readonly IMsuLookupService _msuLookupService;
    private readonly IMsuSelectorService _msuSelectorService;
    private readonly RomLauncherService _romLauncherService;
    private readonly string _solutionPath;
    private readonly string _randomizerRomPath;

    public PatchBuilderService(ILogger<PatchBuilderService> logger, IRomGenerationService romGenerationService, OptionsFactory optionsFactory, IMsuLookupService msuLookupService, IMsuSelectorService msuSelectorService, IMsuTypeService msuTypeService, RomLauncherService romLauncherService)
    {
        _logger = logger;
        _romGenerationService = romGenerationService;
        _solutionPath = RandomizerDirectories.SolutionPath;
        _randomizerRomPath = Path.Combine(_solutionPath, "alttp_sm_combo_randomizer_rom");
        _optionsFactory = optionsFactory;
        _msuLookupService = msuLookupService;
        _msuSelectorService = msuSelectorService;
        _msuTypeService = msuTypeService;
        _romLauncherService = romLauncherService;
    }

    public void CreatePatches(PatchBuilderConfig config)
    {
        _logger.LogInformation("CreatePatches Start");
        try
        {
            var options = _optionsFactory.Create();
            options.PatchOptions = config.PatchOptions;

            Initialize(config);
            BuildPatches(config);
            CopyPatches(config);
            GenerateTestRom(config);
            AssignMsu(config);
            Launch(config);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error with CreatePatches");
        }

    }

    private void Initialize(PatchBuilderConfig config)
    {
        if (string.IsNullOrEmpty(config.EnvironmentSettings.PatchBuildScriptPath))
        {
            config.EnvironmentSettings.PatchBuildScriptPath = Path.Combine(_randomizerRomPath, OperatingSystem.IsWindows() ? "build.bat" : "build.sh");
        }

        if (string.IsNullOrEmpty(config.EnvironmentSettings.OutputPath))
        {
            config.EnvironmentSettings.OutputPath = Path.Combine(_randomizerRomPath, "build", "rom");
        }

        if (!Directory.Exists(config.EnvironmentSettings.OutputPath))
        {
            Directory.CreateDirectory(config.EnvironmentSettings.OutputPath);
        }

        if (string.IsNullOrEmpty(config.EnvironmentSettings.TestRomFileName))
        {
            config.EnvironmentSettings.TestRomFileName = "test-rom";
        }
    }

    private void BuildPatches(PatchBuilderConfig config)
    {
        if (!config.PatchFlags.CreatePatches) return;

        _logger.LogInformation("Building patches");

        var asarPath = Path.Combine(_randomizerRomPath, "resources",
            OperatingSystem.IsWindows() ? "asar.exe" : "asar");
        if (!File.Exists(asarPath))
        {
            throw new FileNotFoundException(
                $"{asarPath} not found. Please download from https://github.com/RPGHacker/asar");
        }

        var ipsPatcher = Path.Combine(_randomizerRomPath, "resources",
            OperatingSystem.IsWindows() ? "Lunar IPS.exe" : "flips-linux");
        if (!File.Exists(ipsPatcher))
        {
            var downloadUrl = OperatingSystem.IsWindows() ? "https://www.romhacking.net/utilities/240/" : "https://www.romhacking.net/utilities/1040/";
            throw new FileNotFoundException(
                $"{ipsPatcher} not found. Please download from {downloadUrl}");
        }

        var smRom = Path.Combine(_randomizerRomPath, "resources", "sm.sfc");
        if (!File.Exists(smRom))
        {
            throw new FileNotFoundException(
                $"{smRom} not found.");
        }

        var info = new FileInfo(config.EnvironmentSettings.PatchBuildScriptPath);

        _logger.LogInformation("Running build script {Path}", info.FullName);

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
            var destinationFile = new FileInfo(Path.Combine(_solutionPath, "src", "TrackerCouncil.Smz3.SeedGenerator", "FileData", "IpsPatches", file.Name));
            file.CopyTo(destinationFile.FullName, true);
            _logger.LogInformation("Copying {Source} to {Destination}", file, destinationFile.FullName);
        }
    }

    private void GenerateTestRom(PatchBuilderConfig config)
    {
        if (!config.PatchFlags.GenerateTestRom) return;
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
        var bytes = _romGenerationService.GenerateRomBytes(randomizerOptions, seedData, null);

        var romPath = Path.Combine(config.EnvironmentSettings.OutputPath, $"{config.EnvironmentSettings.TestRomFileName}.sfc");

        File.WriteAllBytes(romPath, bytes);

        _logger.LogInformation("Wrote test rom to {Path}", romPath);
    }

    private void AssignMsu(PatchBuilderConfig config)
    {
        if (!config.PatchFlags.AssignMsu) return;

        if (!config.PatchOptions.MsuPaths.Any() || !File.Exists(config.PatchOptions.MsuPaths.First()))
        {
            _logger.LogError("No valid MSU entered");
            throw new InvalidOperationException("No valid MSU entered");
        }

        var romPath = Path.Combine(config.EnvironmentSettings.OutputPath, $"{config.EnvironmentSettings.TestRomFileName}.sfc");
        var msuPath = config.PatchOptions.MsuPaths.First();
        var msu = _msuLookupService.LoadMsu(msuPath, null, false, true, true);
        var response = _msuSelectorService.AssignMsu(new MsuSelectorRequest()
        {
            Msu = msu,
            OutputMsuType = _msuTypeService.GetMsuType("Super Metroid / A Link to the Past Combination Randomizer"),
            OutputPath = romPath,
            EmptyFolder = true,
        });

        if (!response.Successful)
        {
            _logger.LogWarning("Error assigning msu: {Error}", response.Message);
        }
    }

    private void Launch(PatchBuilderConfig config)
    {
        if (!config.PatchFlags.LaunchTestRom) return;

        var romPath = Path.Combine(config.EnvironmentSettings.OutputPath, $"{config.EnvironmentSettings.TestRomFileName}.sfc");

        if (!File.Exists(romPath))
        {
            _logger.LogError("No test rom found at {Path}", romPath);
            return;
        }

        _romLauncherService.LaunchRom(romPath, config.EnvironmentSettings.LaunchApplication,
            config.EnvironmentSettings.LaunchArguments);
    }
}
