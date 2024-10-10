using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class RomTextService(ILogger<RomTextService> logger, IGameHintService gameHintService, ISnesConnectorService snesConnectorService)
{
    public async Task<string> WriteSpoilerLog(RandomizerOptions options, SeedData seed, Config config, string folderPath, string fileSuffix)
    {
        var spoilerLog = GetSpoilerLog(options, seed, config.Race || config.DisableSpoilerLog);
        var spoilerFileName = $"Spoiler_Log_{fileSuffix}.txt";
        var spoilerPath = Path.Combine(folderPath, spoilerFileName);
        await File.WriteAllTextAsync(spoilerPath, spoilerLog);
        return spoilerPath;
    }

    public async Task WritePlandoConfig(SeedData seed, string folderPath, string fileSuffix)
    {
        var plandoConfigString = ExportPlandoConfig(seed);
        if (!string.IsNullOrEmpty(plandoConfigString))
        {
            var plandoFileName = $"Spoiler_Plando_{fileSuffix}.yml";
            var plandoPath = Path.Combine(folderPath, plandoFileName);
            await File.WriteAllTextAsync(plandoPath, plandoConfigString);
        }
    }

    public void PrepareAutoTrackerFiles(RandomizerOptions options)
    {
        // Cleanup old auto tracker scripts
        if (Directory.Exists(Path.Combine(options.AutoTrackerScriptsOutputPath, "bizhawk")))
        {
            Directory.Delete(Path.Combine(options.AutoTrackerScriptsOutputPath, "bizhawk"), true);
        }

        if (Directory.Exists(Path.Combine(options.AutoTrackerScriptsOutputPath, "snes9xrr_32bit")))
        {
            Directory.Delete(Path.Combine(options.AutoTrackerScriptsOutputPath, "snes9xrr_32bit"), true);
        }

        if (Directory.Exists(Path.Combine(options.AutoTrackerScriptsOutputPath, "snex9xrr_64bit")))
        {
            Directory.Delete(Path.Combine(options.AutoTrackerScriptsOutputPath, "snex9xrr_64bit"), true);
        }

        try
        {
            snesConnectorService.CreateLuaScriptsFolder(options.AutoTrackerScriptsOutputPath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to copy Auto Tracker Scripts");
        }
    }

    private void CopyDirectory(string source, string dest, bool recursive, bool overwrite)
    {
        var sourceDir = new DirectoryInfo(source);

        var sourceSubDirs = sourceDir.GetDirectories();

        if (!Directory.Exists(dest))
        {
            Directory.CreateDirectory(dest);
        }

        foreach (var file in sourceDir.GetFiles())
        {
            if (overwrite && File.Exists(Path.Combine(dest, file.Name)))
            {
                File.Delete(Path.Combine(dest, file.Name));
            }

            if (!File.Exists(Path.Combine(dest, file.Name)))
            {
                file.CopyTo(Path.Combine(dest, file.Name));
            }
        }

        if (recursive)
        {
            foreach (var subDir in sourceSubDirs)
            {
                var destSubDir = Path.Combine(dest, subDir.Name);
                CopyDirectory(subDir.FullName, destSubDir, recursive, overwrite);
            }
        }
    }

    private string? ExportPlandoConfig(SeedData seed)
    {
        try
        {
            if (seed.WorldGenerationData.Count > 1)
            {
                logger.LogWarning("Attempting to export plando config for multi-world seed. Skipping.");
                return null;
            }

            var world = seed.WorldGenerationData.LocalWorld.World;
            var plandoConfig = new PlandoConfig(world);

            var serializer = new YamlDotNet.Serialization.Serializer();
            return serializer.Serialize(plandoConfig);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while exporting the plando configuration for seed {Seed}. No plando config will be generated.", seed.Seed);
            return null;
        }
    }

    /// <summary>
    /// Underlines text in the spoiler log
    /// </summary>
    /// <param name="text">The text to be underlined</param>
    /// <param name="line">The character to use for underlining</param>
    /// <returns>The text to be underlined followed by the underlining text</returns>
    private static string Underline(string text, char line = '-')
        => text + "\n" + new string(line, text.Length);

    /// <summary>
    /// Gets the spoiler log of a given seed
    /// </summary>
    /// <param name="options">The randomizer generation options</param>
    /// <param name="seed">The previously generated seed data</param>
    /// <param name="configOnly">If the spoiler log should only have config settings printed</param>
    /// <returns>The string output of the spoiler log file</returns>
    private string GetSpoilerLog(RandomizerOptions options, SeedData seed, bool configOnly)
    {
        var log = new StringBuilder();
        log.AppendLine(Underline($"SMZ3 Cas’ spoiler log", '='));
        log.AppendLine($"Generated on {DateTime.Now:F}");
        log.AppendLine($"Seed: {options.SeedOptions.Seed} (actual: {seed.Seed})");

        if (options.SeedOptions.Race)
        {
            log.AppendLine("[Race]");
        }

        log.AppendLine();
        log.AppendLine(Underline("Settings", '='));
        log.AppendLine();

        foreach (var world in seed.WorldGenerationData.Worlds)
        {
            if (world.Config.MultiWorld)
            {
                log.AppendLine(Underline("Player: " + world.Player));
                log.AppendLine();
            }

            log.AppendLine($"Settings String: {Config.ToConfigString(world.Config, true)}");
            log.AppendLine($"Early Items: {string.Join(',', ItemSettingOptions.GetEarlyItemTypes(world.Config).Select(x => x.ToString()).ToArray())}");
            log.AppendLine($"Starting Inventory: {string.Join(',', ItemSettingOptions.GetStartingItemTypes(world.Config).Select(x => x.ToString()).ToArray())}");

            var locationPrefs = new List<string>();
            foreach (var (locationId, value) in world.Config.LocationItems)
            {
                var itemPref = value < Enum.GetValues(typeof(ItemPool)).Length ? ((ItemPool)value).ToString() : ((ItemType)value).ToString();
                locationPrefs.Add($"{world.Locations.First(x => x.Id == locationId).Name} - {itemPref}");
            }
            log.AppendLine($"Location Preferences: {string.Join(',', locationPrefs.ToArray())}");

            var type = options.LogicConfig.GetType();
            var logicOptions = string.Join(',', type.GetProperties().Select(x => $"{x.Name}: {x.GetValue(world.Config.LogicConfig)}"));
            log.AppendLine($"Logic Options: {logicOptions}");

            if (world.Config.Keysanity)
            {
                log.AppendLine("Keysanity: " + world.Config.KeysanityMode.ToString());
            }

            var gtCrystals = world.Config.GanonsTowerCrystalCount;
            var ganonCrystals = world.Config.GanonCrystalCount;
            var pyramid = world.Config.OpenPyramid ? "Open" : "Closed";
            var tourianBosses = world.Config.TourianBossCount;
            log.AppendLine($"Win Conditions: GT = {gtCrystals} Crystals, Ganon = {ganonCrystals} Crystals, Pyramid = {pyramid}, Tourian = {tourianBosses} Bosses");

            log.AppendLine();
        }

        if (File.Exists(options.PatchOptions.Msu1Path))
            log.AppendLine($"MSU-1 pack: {Path.GetFileNameWithoutExtension(options.PatchOptions.Msu1Path)}");
        log.AppendLine();

        if (configOnly)
        {
            return log.ToString();
        }

        log.AppendLine();
        log.AppendLine(Underline("Hints", '='));
        log.AppendLine();

        foreach (var worldGenerationData in seed.WorldGenerationData)
        {
            if (worldGenerationData.Config.MultiWorld)
            {
                log.AppendLine(Underline("Player: " + worldGenerationData.World.Player));
                log.AppendLine();
            }

            foreach (var hint in worldGenerationData.World.HintTiles)
            {
                var hintText = gameHintService.GetHintTileText(hint, worldGenerationData.World,
                    seed.WorldGenerationData.Worlds.ToList());
                log.AppendLine($"{hint.HintTileCode} - {hintText}");
            }
            log.AppendLine();
        }

        log.AppendLine();
        log.AppendLine(Underline("Spheres", '='));
        log.AppendLine();

        var spheres = seed.Playthrough.GetPlaythroughText();
        for (var i = 0; i < spheres.Count; i++)
        {
            if (spheres[i].Count == 0)
                continue;

            log.AppendLine(Underline($"Sphere {i + 1}"));
            log.AppendLine();
            foreach (var (location, item) in spheres[i])
                log.AppendLine($"{location}: {item}");
            log.AppendLine();
        }

        log.AppendLine();
        log.AppendLine(Underline("Dungeons", '='));
        log.AppendLine();

        foreach (var world in seed.WorldGenerationData.Worlds)
        {
            log.AppendLine(world.Config.MultiWorld
                ? Underline("Player " + world.Player + " Rewards")
                : Underline("Rewards"));

            log.AppendLine();

            foreach (var region in world.Regions)
            {
                if (region is IHasReward rewardRegion)
                    log.AppendLine($"{region.Name}: {rewardRegion.RewardType}");
            }
            log.AppendLine();

            log.AppendLine(world.Config.MultiWorld
                ? Underline("Player " + world.Player + " Medallions")
                : Underline("Medallions"));
            log.AppendLine();

            foreach (var region in world.Regions)
            {
                if (region is IHasPrerequisite medallionRegion)
                    log.AppendLine($"{region.Name}: {medallionRegion.RequiredItem}");
            }
            log.AppendLine();
        }

        log.AppendLine();
        log.AppendLine(Underline("All Items", '='));
        log.AppendLine();

        foreach (var world in seed.WorldGenerationData.Worlds)
        {
            if (world.Config.MultiWorld)
            {
                log.AppendLine(Underline("Player: " + world.Player));
                log.AppendLine();
            }

            foreach (var location in world.Locations)
            {
                log.AppendLine(world.Config.MultiWorld
                    ? $"{location}: {location.Item} ({location.Item.World.Player})"
                    : $"{location}: {location.Item}");
            }
            log.AppendLine();
        }

        return log.ToString();
    }

    private string GetSourceDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var directory = Directory.GetParent(currentDirectory);
        while (directory != null && directory.Name != "src")
        {
            directory = Directory.GetParent(directory.FullName);
        }
        return directory?.FullName ?? currentDirectory;
    }
}
