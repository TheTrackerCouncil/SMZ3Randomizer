using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Interfaces;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.SeedGenerator.FileData;
using TrackerCouncil.Smz3.SeedGenerator.FileData.IpsPatches;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class RomGenerationService : IRomGenerationService
{
    private readonly RandomizerContext _dbContext;
    private readonly Smz3Randomizer _randomizer;
    private readonly Smz3Plandomizer _plandomizer;
    private readonly ILogger<RomGenerationService> _logger;
    private readonly ITrackerStateService _stateService;
    private readonly SpritePatcherService _spritePatcherService;
    private readonly IMsuLookupService? _msuLookupService;
    private readonly IMsuSelectorService? _msuSelectorService;
    private readonly IMsuTypeService? _msuTypeService;
    private readonly RomTextService _romTextService;

    public RomGenerationService(Smz3Randomizer randomizer,
        Smz3Plandomizer plandomizer,
        RandomizerContext dbContext,
        ILogger<RomGenerationService> logger,
        ITrackerStateService stateService,
        SpritePatcherService spritePatcherService,
        IMsuLookupService? msuLookupService,
        IMsuSelectorService? msuSelectorService,
        IMsuTypeService? msuTypeService,
        RomTextService romTextService)
    {
        _randomizer = randomizer;
        _plandomizer = plandomizer;
        _dbContext = dbContext;
        _logger = logger;
        _stateService = stateService;
        _spritePatcherService = spritePatcherService;
        _msuLookupService = msuLookupService;
        _msuSelectorService = msuSelectorService;
        _msuTypeService = msuTypeService;
        _romTextService = romTextService;
    }

    public SeedData GeneratePlandoSeed(RandomizerOptions options, PlandoConfig plandoConfig)
    {
        var config = options.ToConfig();
        config.Seed = plandoConfig.Seed;
        config.SettingsString = "";
        config.ItemOptions = new Dictionary<string, int>();
        config.LocationItems = new Dictionary<LocationId, int>();
        config.PlandoConfig = plandoConfig;
        config.KeysanityMode = plandoConfig.KeysanityMode;
        config.GanonsTowerCrystalCount = plandoConfig.GanonsTowerCrystalCount;
        config.GanonCrystalCount = plandoConfig.GanonCrystalCount;
        config.OpenPyramid = plandoConfig.OpenPyramid;
        config.TourianBossCount = plandoConfig.TourianBossCount;
        config.LogicConfig = plandoConfig.Logic.Clone();
        return _plandomizer.GenerateSeed(config, CancellationToken.None);
    }

    /// <summary>
    /// Generates a randomizer ROM and returns details about the rom
    /// </summary>
    /// <param name="options">The randomizer generation options</param>
    /// <param name="attempts">The number of times the rom should be attempted to be generated</param>
    /// <returns>True if the rom was generated successfully, false otherwise</returns>
    public async Task<GeneratedRomResult> GenerateRandomRomAsync(RandomizerOptions options, int attempts = 5)
    {
        string? error = null;
        var seed = (SeedData?)null;

        for (var i = 0; i < attempts; i++)
        {
            try
            {
                seed = GenerateSeed(options);
                if (!_randomizer.ValidateSeedSettings(seed))
                {
                    error = "The seed generated is playable and a rom has been generated, but it does not contain all requested settings.\n" +
                            "Retrying to generate the seed may work, but the selected settings may be impossible to generate successfully and will need to be updated.";
                }
                else
                {
                    error = "";
                    break;
                }
            }
            catch (RandomizerGenerationException e)
            {
                return new GeneratedRomResult()
                {
                    GenerationError =
                        $"Error generating rom\n{e.Message}\nPlease try again. If it persists, try modifying your seed settings."
                };
            }
        }

        if (seed != null)
        {
            var rom = await GenerateRomInternalAsync(seed, options, null);

            return new GeneratedRomResult()
            {
                Rom = rom,
                GenerationError = error,
            };
        }
        else
        {
            return new GeneratedRomResult()
            {
                GenerationError = "There was an unknown error creating the rom. Please check your settings and try again."
            };
        }

    }

    /// <summary>
    /// Uses the options to generate the rom
    /// </summary>
    /// <param name="options">The randomizer generation options</param>
    /// <param name="seed">The seed data to write to the ROM.</param>
    /// <returns>The bytes of the rom file</returns>
    public byte[] GenerateRomBytes(RandomizerOptions options, SeedData? seed)
    {
        if (string.IsNullOrEmpty(options.GeneralOptions.SMRomPath) ||
            string.IsNullOrEmpty(options.GeneralOptions.Z3RomPath))
            throw new InvalidOperationException("Super Metroid or Zelda rom path is not specified");

        byte[] rom;
        using (var smRom = File.OpenRead(options.GeneralOptions.SMRomPath))
        using (var z3Rom = File.OpenRead(options.GeneralOptions.Z3RomPath))
        {
            rom = Rom.CombineSMZ3Rom(smRom, z3Rom);
        }

        using (var ips = IpsPatch.Smz3())
        {
            Rom.ApplyIps(rom, ips);
        }

        _spritePatcherService.ApplySpriteOptions(rom, out var linkSpriteName, out var samusSpriteName);

        if (seed != null)
        {
            Rom.ApplySeed(rom, seed.WorldGenerationData.LocalWorld.Patches);
            var localConfig = seed.Configs.First(x => x.IsLocalConfig);
            localConfig.SamusName = samusSpriteName;
            localConfig.LinkName = linkSpriteName;
        }

        if (options.PatchOptions.CasPatches.Respin)
        {
            using var patch = IpsPatch.Respin();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.NerfedCharge)
        {
            using var patch = IpsPatch.NerfedCharge();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.RefillAtSaveStation)
        {
            using var patch = IpsPatch.RefillAtSaveStation();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.FastDoors)
        {
            using var patch = IpsPatch.FastDoors();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.FastElevators)
        {
            using var patch = IpsPatch.FastElevators();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.AimAnyButton)
        {
            using var patch = IpsPatch.AimAnyButton();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.Speedkeep)
        {
            using var patch = IpsPatch.SpeedKeep();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.DisableFlashing)
        {
            using var patch = IpsPatch.DisableMetroidFlashing();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.DisableScreenShake)
        {
            using var patch = IpsPatch.DisableMetroidScreenShake();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.EasierWallJumps)
        {
            using var patch = IpsPatch.EasierWallJumps();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.SnapMorph)
        {
            using var patch = IpsPatch.SnapMorph();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.CasPatches.SandPitPlatforms)
        {
            using var patch = IpsPatch.SandPitPlatforms();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.MetroidControls.RunButtonBehavior == RunButtonBehavior.AutoRun)
        {
            using var patch = IpsPatch.AutoRun();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.MetroidControls.ItemCancelBehavior != ItemCancelBehavior.Vanilla)
        {
            using var patch = options.PatchOptions.MetroidControls.ItemCancelBehavior == ItemCancelBehavior.Toggle ? IpsPatch.ItemCancelToggle() : IpsPatch.ItemCancelHoldFire();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        if (options.PatchOptions.MetroidControls.AimButtonBehavior == AimButtonBehavior.UnifiedAim)
        {
            using var patch = IpsPatch.UnifiedAim();
            Rom.ApplySuperMetroidIps(rom, patch);
        }

        Rom.UpdateChecksum(rom);

        return rom;
    }


    /// <summary>
    /// Generates a plando ROM and returns details about the rom
    /// </summary>
    /// <param name="options">The randomizer generation options</param>
    /// <param name="plandoConfig">Config with the details of how to generate the rom</param>
    /// <returns>True if the rom was generated successfully, false otherwise</returns>
    public async Task<GeneratedRomResult> GeneratePlandoRomAsync(RandomizerOptions options, PlandoConfig plandoConfig)
    {
        try
        {
            var seed = GeneratePlandoSeed(options, plandoConfig);
            var rom = await GenerateRomInternalAsync(seed, options, null);
            return new GeneratedRomResult()
            {
                Rom = rom
            };
        }
        catch (PlandoConfigurationException e)
        {
            return new GeneratedRomResult()
            {
                GenerationError = $"The plando configuration is invalid or incomplete.\n{e.Message}\nPlease check the plando configuration file you used and try again."
            };
        }
    }

    public async Task<GeneratedRomResult> GeneratePreSeededRomAsync(RandomizerOptions options, SeedData seed, MultiplayerGameDetails multiplayerGameDetails)
    {
        var results = await GenerateRomInternalAsync(seed, options, multiplayerGameDetails);
        return new GeneratedRomResult()
        {
            Rom = results
        };
    }

    private async Task<GeneratedRom?> GenerateRomInternalAsync(SeedData seed, RandomizerOptions options, MultiplayerGameDetails? multiplayerGameDetails)
    {
        var bytes = GenerateRomBytes(options, seed);
        var config = seed.Playthrough.Config;
        var safeSeed = seed.Seed.ReplaceAny(Path.GetInvalidFileNameChars(), '_');

        var folderPath = Path.Combine(options.RomOutputPath, $"{DateTimeOffset.Now:yyyyMMdd-HHmmss}_{safeSeed}");
        Directory.CreateDirectory(folderPath);

        // For BizHawk shuffler support, the file name is checked when running the BizHawk Auto Tracking Lua script
        // If the rom file name is changed, make sure to update the BizHawk emulator.lua script and the LuaConnector
        var fileSuffix = $"{DateTimeOffset.Now:yyyyMMdd-HHmmss}_{safeSeed}";
        var romFileName = $"SMZ3_Cas_{fileSuffix}.sfc";
        var romPath = Path.Combine(folderPath, romFileName);
        ApplyMsuOptions(options, romPath);
        await File.WriteAllBytesAsync(romPath, bytes);

        var spoilerPath = await _romTextService.WriteSpoilerLog(options, seed, config, folderPath, fileSuffix);
        await _romTextService.WritePlandoConfig(seed, folderPath, fileSuffix);
        _romTextService.PrepareAutoTrackerFiles(options);

        var rom = await SaveSeedToDatabaseAsync(options, seed, romPath, spoilerPath, multiplayerGameDetails);

        try
        {
            options.Save();
        }
        catch (Exception)
        {
            // Do nothing
        }

        return rom;
    }

    /// <summary>
    /// Takes the given seed information and saves it to the database
    /// </summary>
    /// <param name="options">The randomizer generation options</param>
    /// <param name="seed">The generated seed data</param>
    /// <param name="romPath">The path to the rom file</param>
    /// <param name="spoilerPath">The path to the spoiler file</param>
    /// <param name="multiplayerGameDetails">Details of the connected multiplayer game</param>
    /// <returns>The db entry for the generated rom</returns>
    private async Task<GeneratedRom> SaveSeedToDatabaseAsync(RandomizerOptions options, SeedData seed, string romPath, string spoilerPath, MultiplayerGameDetails? multiplayerGameDetails)
    {
        var settingsString = seed.Configs.Count() > 1
            ? Config.ToConfigString(seed.Configs)
            : Config.ToConfigString(seed.PrimaryConfig, true);

        var rom = new GeneratedRom()
        {
            Seed = seed.Seed,
            RomPath = Path.GetRelativePath(options.RomOutputPath, romPath),
            SpoilerPath = Path.GetRelativePath(options.RomOutputPath, spoilerPath),
            Date = DateTimeOffset.Now,
            Settings = settingsString,
            GeneratorVersion = RandomizerVersion.MajorVersion,
            MultiplayerGameDetails = multiplayerGameDetails,
            MsuRandomizationStyle = options.PatchOptions.MsuRandomizationStyle,
            MsuShuffleStyle = options.PatchOptions.MsuShuffleStyle,
            MsuPaths = string.Join("|", options.PatchOptions.MsuPaths)
        };
        _dbContext.GeneratedRoms.Add(rom);

        if (multiplayerGameDetails != null)
        {
            multiplayerGameDetails.GeneratedRom = rom;
        }

        await _stateService.CreateStateAsync(seed.WorldGenerationData.Worlds, rom);
        return rom;
    }

    private bool ApplyMsuOptions(RandomizerOptions options, string romPath)
    {
        if (!options.PatchOptions.MsuPaths.Any())
        {
            return false;
        }

        if (_msuLookupService == null || _msuSelectorService == null || _msuTypeService == null)
        {
            return false;
        }

        if (!_msuLookupService.Msus.Any())
        {
            _msuLookupService.LookupMsus(options.GeneralOptions.MsuPath);
        }

        var romFileInfo = new FileInfo(romPath);
        var outputPath = romFileInfo.FullName.Replace(romFileInfo.Extension, ".msu");

        if (options.PatchOptions.MsuRandomizationStyle == null)
        {
            _msuSelectorService.AssignMsu(new MsuSelectorRequest()
            {
                MsuPath = options.PatchOptions.MsuPaths.First(),
                OutputMsuType = _msuTypeService.GetSMZ3MsuType(),
                OutputPath = outputPath,
            });
        }
        else if (options.PatchOptions.MsuRandomizationStyle == MsuRandomizationStyle.Single)
        {
            _msuSelectorService.PickRandomMsu(new MsuSelectorRequest()
            {
                MsuPaths = options.PatchOptions.MsuPaths,
                OutputMsuType = _msuTypeService.GetSMZ3MsuType(),
                OutputPath = outputPath,
            });
        }
        else
        {
            _msuSelectorService.CreateShuffledMsu(new MsuSelectorRequest()
            {
                MsuPaths = options.PatchOptions.MsuPaths,
                OutputMsuType = _msuTypeService.GetSMZ3MsuType(),
                OutputPath = outputPath,
                ShuffleStyle = options.PatchOptions.MsuShuffleStyle
            });
        }

        return true;
    }


    /// <summary>
    /// Generates a seed for a rom based on the given randomizer options
    /// </summary>
    /// <param name="options">The randomizer generation options</param>
    /// <param name="seed">The string seed to use for generating the rom</param>
    /// <returns>The seed data</returns>
    private SeedData GenerateSeed(RandomizerOptions options, string? seed = null)
    {
        var config = options.ToConfig();
        return _randomizer.GenerateSeed(config, seed ?? config.Seed, CancellationToken.None);
    }
}
