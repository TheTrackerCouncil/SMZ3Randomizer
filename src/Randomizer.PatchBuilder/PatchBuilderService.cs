using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Options;
using Randomizer.SMZ3.Generation;

namespace Randomizer.PatchBuilder;

public class PatchBuilderService
{
    private readonly ILogger<PatchBuilderService> _logger;
    private readonly RomGenerationService _romGenerationService;

    public PatchBuilderService(ILogger<PatchBuilderService> logger, RomGenerationService romGenerationService)
    {
        _logger = logger;
        _romGenerationService = romGenerationService;
    }

    public void CreatePatches(PatchBuilderConfig config)
    {
        _logger.LogInformation("CreatePatches Start");
        try
        {
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
        var scriptFile = string.IsNullOrEmpty(config.EnvironmentSettings.PatchBuildScriptPath)
            ? @"..\..\..\..\..\alttp_sm_combo_randomizer_rom\build.bat"
            : config.EnvironmentSettings.PatchBuildScriptPath;
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
        info = new FileInfo(@"..\..\..\..\..\alttp_sm_combo_randomizer_rom\build\zsm.ips");

        var length = info.Exists ? info.Length : 0L;
        if (length < 1000)
        {
            throw new InvalidOperationException("Invalid zsm.ips file created");
        }
    }

    private void CopyPatches(PatchBuilderConfig config)
    {
        if (!config.PatchFlags.CopyPatchesToProject) return;
        var patchBuildFolder = new DirectoryInfo(@"..\..\..\..\..\alttp_sm_combo_randomizer_rom\build");
        foreach (var file in patchBuildFolder.EnumerateFiles().Where(x => x.Extension.Contains("ips")))
        {
            var destinationFile = new FileInfo(@"..\..\..\..\Randomizer.SMZ3\FileData\IpsPatches\" + file.Name);
            file.CopyTo(destinationFile.FullName, true);
            _logger.LogInformation("Copying {Source} to {Destination}", file, destinationFile.FullName);
        }
    }

    private string? GenerateTestRom(PatchBuilderConfig config)
    {
        if (!config.PatchFlags.GenerateTestRom) return null;
        var smRom = new FileInfo(string.IsNullOrEmpty(config.EnvironmentSettings.MetroidRomPath)
            ? @"..\..\..\..\..\alttp_sm_combo_randomizer_rom\resources\sm.sfc"
            : config.EnvironmentSettings.MetroidRomPath);
        var z3Rom = new FileInfo(string.IsNullOrEmpty(config.EnvironmentSettings.Z3RomPath)
            ? @"..\..\..\..\..\alttp_sm_combo_randomizer_rom\resources\z3.sfc"
            : config.EnvironmentSettings.Z3RomPath);

        if (!smRom.Exists || !z3Rom.Exists)
        {
            throw new FileNotFoundException("Super Metroid or Zelda rom not found");
        }

        var outputFolder = config.EnvironmentSettings.OutputPath;
        if (string.IsNullOrEmpty(outputFolder))
        {
            outputFolder = new DirectoryInfo(@"..\..\..\..\..\alttp_sm_combo_randomizer_rom\build").FullName;
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
        if (string.IsNullOrEmpty(romPath)) return;
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
            else if (launchArguments.Contains("%rom%"))
            {
                launchArguments = config.EnvironmentSettings.LaunchArguments.Replace("%rom%", $"\"{romPath}\"");
            }
            else
            {
                launchArguments = $"{config.EnvironmentSettings.LaunchArguments} \"{romPath}\"";
            }
        }

        try
        {
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


}
