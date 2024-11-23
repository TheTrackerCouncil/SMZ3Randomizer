using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.FileData;
using TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public partial class Smz3RomParser(ILogger<Smz3RomParser> logger, IWorldAccessor worldAccessor, IPatcherService patcherService, ParsedRomFiller parsedRomFiller, Configs configs)
{
    [GeneratedRegex(@"\b[0-9A-Fa-f]+\b")]
    private static partial Regex HexRegex();

    public ParsedRomDetails ParseRomFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException();
        }

        var exampleWorld = new World(new Config(), "", 1, "");
        var rom = File.ReadAllBytes(filePath);

        var isMultiworldEnabled = MetadataPatch.IsRomMultiworldEnabled(rom);
        var isKeysanityEnabled = MetadataPatch.IsRomKeysanityEnabled(rom);
        var romTitle = MetadataPatch.GetGameTitle(rom);
        var playerNames = MetadataPatch.GetPlayerNames(rom);
        var playerIndex = MetadataPatch.GetPlayerIndex(rom);
        var players = playerNames.Select((name, index) => new ParsedRomPlayer()
        {
            PlayerName = name.Trim(), PlayerIndex = index, IsLocalPlayer = index == playerIndex,
        }).ToList();
        logger.LogInformation("Parsed players {PlayerList}", string.Join(", ", players.Select(p => p.PlayerName)));

        var itemTypes = configs.Items.Where(x => x.InternalItemType != ItemType.Nothing)
            .Select(x => (int)x.InternalItemType).ToList();
        var locations = LocationsPatch.GetLocationsFromRom(rom, playerNames, exampleWorld, isMultiworldEnabled, itemTypes);
        var prerequisites = MedallionPatch.GetPrerequisitesFromRom(rom, exampleWorld.PrerequisiteRegions);
        var rewards = ZeldaRewardsPatch.GetRewardsFromRom(rom, exampleWorld.RewardRegions);
        var bosses = exampleWorld.BossRegions.Select(x => new ParsedRomBossDetails()
        {
            RegionType = x.GetType(), RegionName = x.Name, BossType = x.BossType
        }).ToList();

        var keysanityMode = KeysanityMode.None;
        if (isKeysanityEnabled)
        {
            var isMetroidKeysanity = MetroidKeysanityPatch.GetIsMetroidKeysanity(rom);
            var isZeldaKeysanity = ZeldaKeysanityPatch.GetIsZeldaKeysanity(rom);

            if (isMetroidKeysanity && isZeldaKeysanity)
            {
                keysanityMode = KeysanityMode.Both;
            }
            else if (isMetroidKeysanity)
            {
                keysanityMode = KeysanityMode.SuperMetroid;
            }
            else if (isZeldaKeysanity)
            {
                keysanityMode = KeysanityMode.Zelda;
            }
        }

        logger.LogInformation("Keysanity: {Mode}", keysanityMode);

        var hardLogic = false;
        var seedNumber = 0;
        var isArchipelagoSeed = false;
        var isCasRom = false;

        if (romTitle.StartsWith("ZSM"))
        {
            isArchipelagoSeed = playerNames.First() == "Archipelago";

            var flags = romTitle[3..];
            var flagIndex = flags.IndexOf(flags.FirstOrDefault(char.IsLetter));
            flags = flags.Substring(flagIndex, 2);
            hardLogic = flags[1] == 'H';

            var seedString = romTitle[(3 + flagIndex + 2)..];
            var seed = isArchipelagoSeed ? seedString[playerIndex.ToString().Length..] : seedString;
            var match = HexRegex().Match(seed);
            seedNumber = match.Success ? Convert.ToInt32(seed, 16) : new Random().Next();
        }
        else if (romTitle.StartsWith("SMZ3 Cas"))
        {
            isCasRom = true;
        }

        var gtCrystalCount = GoalsPatch.GetGanonsTowerCrystalCountFromRom(rom);
        var ganonCrystalCount = GoalsPatch.GetGanonCrystalCountFromRom(rom);
        var tourianBossCount = GoalsPatch.GetTourianBossCountFromRom(rom, isCasRom);

        logger.LogInformation("Imported {Title} (Seed {SeedNumber})", romTitle, seedNumber);

        return new ParsedRomDetails()
        {
            RomTitle = romTitle,
            Seed = seedNumber,
            IsMultiworld = isMultiworldEnabled,
            IsHardLogic = hardLogic,
            KeysanityMode = keysanityMode,
            IsArchipelago = isArchipelagoSeed,
            GanonsTowerCrystalCount = gtCrystalCount,
            GanonCrystalCount = ganonCrystalCount,
            TourianBossCount = tourianBossCount,
            IsCasRom = isCasRom,
            Players = players,
            Locations = locations,
            Rewards = rewards,
            Bosses = bosses,
            Prerequisites = prerequisites,
        };
    }

    public SeedData GenerateSeedData(RandomizerOptions options, ParsedRomDetails parsedRomDetails, CancellationToken cancellationToken = default)
    {
        var config = options.ToConfig();
        config.PlayerName = parsedRomDetails.Players.First(x => x.IsLocalPlayer).PlayerName;
        config.ParsedRomDetails = parsedRomDetails;
        config.KeysanityMode = parsedRomDetails.KeysanityMode;
        config.Seed = parsedRomDetails.Seed.ToString();
        config.GanonsTowerCrystalCount = parsedRomDetails.GanonsTowerCrystalCount;
        config.GanonCrystalCount = parsedRomDetails.GanonCrystalCount;
        config.TourianBossCount = parsedRomDetails.TourianBossCount;

        var worlds = new List<World>
        {
            new(config, parsedRomDetails.Players.First(x => x.IsLocalPlayer).PlayerName, 0, Guid.NewGuid().ToString("N"))
        };

        parsedRomFiller.Fill(worlds, config, cancellationToken);

        var seedData = new SeedData
        (
            guid: Guid.NewGuid().ToString("N"),
            seed: config.Seed,
            game: parsedRomDetails.IsArchipelago ? $"AP SMZ3 Rom {config.Seed}" : $"SMZ3 Mainline Rom {config.Seed}",
            mode: GameMode.Normal.ToString(),
            worldGenerationData: [],
            playthrough: new Playthrough(config, []),
            configs: new List<Config> { config },
            primaryConfig: config
        );

        var patches = patcherService.GetPatches(new GetPatchesRequest()
        {
            World = worlds[0],
            Worlds = worlds,
            SeedGuid = seedData.Guid,
            Seed = parsedRomDetails.Seed,
            Random = new Random(parsedRomDetails.Seed).Sanitize(),
            IsParsedRom = true
        });

        var worldGenerationData = new WorldGenerationData(worlds[0], patches);
        seedData.WorldGenerationData.Add(worldGenerationData);

        worldAccessor.World = worlds[0];
        worldAccessor.Worlds = worlds;

        return seedData;
    }
}
