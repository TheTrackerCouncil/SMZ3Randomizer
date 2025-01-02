using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.FileData;
using TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public partial class Smz3RomParser(ILogger<Smz3RomParser> logger, IWorldAccessor worldAccessor, IPatcherService patcherService, ParsedRomFiller parsedRomFiller, Configs configs, IGameHintService gameHintService)
{
    [GeneratedRegex(@"\b[0-9A-Fa-f]+\b")]
    private static partial Regex HexRegex();

    public ParsedRomDetails ParseRomFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException();
        }

        logger.LogInformation("Parsing rom file: {Path}", filePath);

        var exampleWorld = new World(new Config(), "", 1, "");
        var rom = File.ReadAllBytes(filePath);

        try
        {
            var isMultiworldEnabled = MetadataPatch.IsRomMultiworldEnabled(rom);
            var isKeysanityEnabled = MetadataPatch.IsRomKeysanityEnabled(rom);
            var romTitle = MetadataPatch.GetGameTitle(rom);
            var playerNames = isMultiworldEnabled ? MetadataPatch.GetPlayerNames(rom) : ["Player"];
            var playerIndex = MetadataPatch.GetPlayerIndex(rom);
            var players = playerNames.Select((name, index) => new ParsedRomPlayer()
            {
                PlayerName = name.Trim(), PlayerIndex = index, IsLocalPlayer = index == playerIndex,
            }).ToList();
            logger.LogInformation("Parsed players {PlayerList}", string.Join(", ", players.Select(p => p.PlayerName)));

            var itemTypes = configs.Items.Where(x => x.InternalItemType != ItemType.Nothing)
                .Select(x => (int)x.InternalItemType).ToList();

            var prerequisites = MedallionPatch.GetPrerequisitesFromRom(rom, exampleWorld.PrerequisiteRegions);
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
            RomGenerator? romGenerator = null;

            if (romTitle.StartsWith("ZSM"))
            {
                romGenerator = playerNames.First() == "Archipelago" ? RomGenerator.Archipelago : RomGenerator.Mainline;

                var flags = romTitle[3..];
                var flagIndex = flags.IndexOf(flags.FirstOrDefault(char.IsLetter));
                flags = flags.Substring(flagIndex, 2);
                hardLogic = flags[1] == 'H';

                var seedString = romTitle[(3 + flagIndex + 2)..];
                var seed = romGenerator == RomGenerator.Archipelago
                    ? seedString[playerIndex.ToString().Length..]
                    : seedString;
                var match = HexRegex().Match(seed);
                seedNumber = match.Success ? Convert.ToInt32(seed, 16) : new Random().Next();
            }
            else if (romTitle.StartsWith("SMZ3 Cas"))
            {
                romGenerator = RomGenerator.Cas;
            }

            if (romGenerator == null)
            {
                throw new InvalidOperationException("Could not determine rom generator from the rom details");
            }

            var locations =
                LocationsPatch.GetLocationsFromRom(rom, playerNames, exampleWorld, isMultiworldEnabled, itemTypes, romGenerator == RomGenerator.Cas);
            var rewards =
                ZeldaRewardsPatch.GetRewardsFromRom(rom, exampleWorld.RewardRegions, romGenerator == RomGenerator.Cas);
            var startingItems = StartingEquipmentPatch.GetStartingItemList(rom, romGenerator == RomGenerator.Cas);
            var gtCrystalCount = GoalsPatch.GetGanonsTowerCrystalCountFromRom(rom);
            var ganonCrystalCount = GoalsPatch.GetGanonCrystalCountFromRom(rom);
            var tourianBossCount = GoalsPatch.GetTourianBossCountFromRom(rom, romGenerator == RomGenerator.Cas);
            var text = ZeldaTextsPatch.ParseRomText(rom);

            logger.LogInformation("Imported {Title} (Seed {SeedNumber})", romTitle, seedNumber);

            return new ParsedRomDetails
            {
                OriginalPath = filePath,
                OriginalFilename = Path.GetFileNameWithoutExtension(filePath),
                RomTitle = romTitle,
                Seed = seedNumber,
                IsMultiworld = isMultiworldEnabled,
                IsHardLogic = hardLogic,
                KeysanityMode = keysanityMode,
                GanonsTowerCrystalCount = gtCrystalCount,
                GanonCrystalCount = ganonCrystalCount,
                TourianBossCount = tourianBossCount,
                RomGenerator = romGenerator.Value,
                Players = players,
                Locations = locations,
                Rewards = rewards,
                Bosses = bosses,
                Prerequisites = prerequisites,
                StartingItems = startingItems,
                ParsedText = text
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while parsing rom");
            throw;
        }
    }

    public SeedData GenerateSeedData(RandomizerOptions options, ParsedRomDetails parsedRomDetails, CancellationToken cancellationToken = default)
    {
        var config = options.ToConfig();
        config.PlayerName = parsedRomDetails.Players.First(x => x.IsLocalPlayer).PlayerName;
        config.ParsedRomDetails = parsedRomDetails;
        config.GameMode = parsedRomDetails.IsMultiworld ? GameMode.Multiworld : GameMode.Normal;
        config.RomGenerator = parsedRomDetails.RomGenerator;
        config.KeysanityMode = parsedRomDetails.KeysanityMode;
        config.Seed = parsedRomDetails.Seed.ToString();
        config.GanonsTowerCrystalCount = parsedRomDetails.GanonsTowerCrystalCount;
        config.GanonCrystalCount = parsedRomDetails.GanonCrystalCount;
        config.TourianBossCount = parsedRomDetails.TourianBossCount;
        config.LocationItems.Clear();
        config.ItemOptions = parsedRomDetails.StartingItems.ToDictionary(x => $"ItemType:{x.Key}", x => x.Value);
        config.LogicConfig = new LogicConfig()
        {
            InfiniteBombJump = true,
            ParlorSpeedBooster = true,
            MoatSpeedBooster = true,
        };

        var worlds = new List<World>
        {
            new(config, parsedRomDetails.Players.First(x => x.IsLocalPlayer).PlayerName, 0, Guid.NewGuid().ToString("N"))
        };

        parsedRomFiller.Fill(worlds, config, cancellationToken);

        var mockPlaythrough = new Playthrough(config, []);

        var seedData = new SeedData
        (
            guid: Guid.NewGuid().ToString("N"),
            seed: parsedRomDetails.OriginalFilename,
            game: parsedRomDetails.RomGenerator == RomGenerator.Archipelago ? $"AP SMZ3 Rom {config.Seed}" : $"SMZ3 Mainline Rom {config.Seed}",
            mode: config.GameMode.ToString(),
            worldGenerationData: [],
            playthrough: mockPlaythrough,
            configs: new List<Config> { config },
            primaryConfig: config
        );

        var rng = new Random(parsedRomDetails.Seed).Sanitize();
        gameHintService.GetInGameHints(worlds[0], worlds, mockPlaythrough, rng.Next());
        var patches = patcherService.GetPatches(new GetPatchesRequest()
        {
            World = worlds[0],
            Worlds = worlds,
            SeedGuid = seedData.Guid,
            Seed = parsedRomDetails.Seed,
            Random = rng,
            IsParsedRom = true,
            PreviousParsedText = parsedRomDetails.ParsedText
        });

        var worldGenerationData = new WorldGenerationData(worlds[0], patches);
        seedData.WorldGenerationData.Add(worldGenerationData);

        worldAccessor.World = worlds[0];
        worldAccessor.Worlds = worlds;

        return seedData;
    }
}
