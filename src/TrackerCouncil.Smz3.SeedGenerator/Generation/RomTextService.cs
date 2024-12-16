using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class RomTextService(ILogger<RomTextService> logger, IGameHintService gameHintService, ISnesConnectorService snesConnectorService)
{
    private static readonly string s_plandoSchemaPath = @"https://raw.githubusercontent.com/TheTrackerCouncil/SMZ3CasConfigs/refs/heads/main/Schemas/plando.json";

    public async Task<string> WriteSpoilerLog(RandomizerOptions options, SeedData seed, Config config, string folderPath, string fileSuffix, bool isParsedRom = false)
    {
        var spoilerLog = GetSpoilerLog(options, seed, config.Race || config.DisableSpoilerLog, isParsedRom);
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

            StringBuilder output = new();
            output.AppendLine($"# yaml-language-server: $schema={GetPlandoSchemaPath()}");
            output.AppendLine();
            output.AppendLine("# Visual Studio Code with the redhat YAML extension is recommended for schema validation.");
            output.AppendLine();
            output.AppendLine(serializer.Serialize(plandoConfig));
            return output.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while exporting the plando configuration for seed {Seed}. No plando config will be generated.", seed.Seed);
            return null;
        }
    }

    private static string GetPlandoSchemaPath()
    {
#if DEBUG
        var parentDir = new DirectoryInfo(RandomizerDirectories.SolutionPath).Parent;
        var localPlandoSchemaPath = Path.Combine(parentDir?.FullName ?? RandomizerDirectories.SolutionPath, "SMZ3CasConfigs", "Schemas", "plando.json");
        if (File.Exists(localPlandoSchemaPath))
        {
            return localPlandoSchemaPath;
        }
        else
        {
            return s_plandoSchemaPath;
        }
#else
        return s_plandoSchemaPath;
#endif
    }

    /// <summary>
    /// Underlines text in the spoiler log
    /// </summary>
    /// <param name="text">The text to be underlined</param>
    /// <param name="line">The character to use for underlining</param>
    /// <returns>The text to be underlined followed by the underlining text</returns>
    private static string Underline(string text, char line = '-')
        => text + "\n" + new string(line, text.Length);

    private string SectionTitle(string text)
    {
        return "\n" + Underline(text, '=') + "\n";
    }

    /// <summary>
    /// Gets the spoiler log of a given seed
    /// </summary>
    /// <param name="options">The randomizer generation options</param>
    /// <param name="seed">The previously generated seed data</param>
    /// <param name="configOnly">If the spoiler log should only have config settings printed</param>
    /// <param name="isParsedRom">If this is a spoiler log for a parsed rom from a file</param>
    /// <returns>The string output of the spoiler log file</returns>
    private string GetSpoilerLog(RandomizerOptions options, SeedData seed, bool configOnly, bool isParsedRom = false)
    {
        var log = new StringBuilder();
        log.AppendLine(Underline($"SMZ3 Cas’ spoiler log", '='));

        if (isParsedRom)
        {
            log.AppendLine($"Parsed rom details on {DateTime.Now:F}");
        }
        else
        {
            log.AppendLine($"Generated on {DateTime.Now:F}");
        }

        log.AppendLine($"Seed: {options.SeedOptions.Seed} (actual: {seed.Seed})");

        if (options.SeedOptions.Race)
        {
            log.AppendLine("[Race]");
        }

        log.AppendLine(SectionTitle("Settings"));

        foreach (var world in seed.WorldGenerationData.Worlds)
        {
            if (!isParsedRom)
            {
                if (world.Config.MultiWorld)
                {
                    log.AppendLine(Underline("Player: " + world.Player));
                    log.AppendLine();
                }

                log.AppendLine($"Settings String: {Config.ToConfigString(world.Config, true)}");
                log.AppendLine($"Early Items: {string.Join(',', ItemSettingOptions.GetEarlyItemTypes(world.Config).Select(x => x.ToString()).ToArray())}");
            }

            log.AppendLine($"Starting Inventory: {string.Join(',', ItemSettingOptions.GetStartingItemTypes(world.Config).Select(x => x.ToString()).ToArray())}");

            if (!isParsedRom)
            {
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
            }

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

        if (configOnly)
        {
            return log.ToString();
        }

        log.AppendLine(SectionTitle("Hints"));

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

        if (!isParsedRom)
        {
            log.AppendLine(SectionTitle("Spheres"));

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
        }

        log.AppendLine(SectionTitle("Dungeons"));

        foreach (var world in seed.WorldGenerationData.Worlds)
        {
            log.AppendLine(!isParsedRom && world.Config.MultiWorld
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

        log.AppendLine(SectionTitle("All Items"));

        foreach (var world in seed.WorldGenerationData.Worlds)
        {
            if (world.Config.MultiWorld)
            {
                log.AppendLine(Underline("Player: " + world.Player));
                log.AppendLine();
            }

            foreach (var location in world.Locations)
            {
                log.AppendLine(world.Config.GameMode == GameMode.Multiworld
                    ? $"{location}: {location.Item} ({location.Item.PlayerName})"
                    : $"{location}: {location.Item}");
            }
            log.AppendLine();
        }

        return log.ToString();
    }
}
