﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.FileData;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class Smz3Randomizer : ISeededRandomizer
{
    private readonly IWorldAccessor _worldAccessor;
    private readonly ILogger<Smz3Randomizer> _logger;
    private readonly IGameHintService _hintService;
    private readonly IPatcherService _patcherService;
    private readonly PlaythroughService _playthroughService;

    public Smz3Randomizer(IFiller filler, IWorldAccessor worldAccessor, IGameHintService gameHintGenerator,
        ILogger<Smz3Randomizer> logger, IPatcherService patcherService, PlaythroughService playthroughService)
    {
        Filler = filler;
        _worldAccessor = worldAccessor;
        _logger = logger;
        _patcherService = patcherService;
        _playthroughService = playthroughService;
        _hintService = gameHintGenerator;
    }

    public static string Name => "Super Metroid & A Link to the Past Cas’ Randomizer";

    protected IFiller Filler { get; }

    public SeedData GenerateSeed(Config config, CancellationToken cancellationToken = default)
        => GenerateSeed(new List<Config> { config }, "", cancellationToken);

    public SeedData GenerateSeed(Config config, string? seed, CancellationToken cancellationToken = default)
        => GenerateSeed(new List<Config>() { config }, seed, cancellationToken);

    public SeedData GenerateSeed(List<Config> configs, string? seed, CancellationToken cancellationToken = default)
    {
        var primaryConfig = configs.OrderBy(x => x.Id).First();

        var seedNumber = ISeededRandomizer.ParseSeed(ref seed);
        var rng = new Random(seedNumber);
        primaryConfig.Seed = seedNumber.ToString();

        _logger.LogInformation("Attempting to generate seed {SeedNumber}", seedNumber);
        _logger.LogInformation("Configs: {ConfigString}", Config.ToConfigString(configs));

        if (primaryConfig.Race)
            rng = new Random(rng.Next());

        var worlds = new List<World>();
        if (primaryConfig.SingleWorld)
        {
            worlds.Add(new World(primaryConfig, "Player", 0, Guid.NewGuid().ToString("N")));
            _logger.LogDebug(
                "Seed: {SeedNumber} | Race: {PrimaryConfigRace} | Keysanity: {PrimaryConfigKeysanityMode} | Item placement: {PrimaryConfigItemPlacementRule}",
                seedNumber, primaryConfig.Race, primaryConfig.KeysanityMode, primaryConfig.ItemPlacementRule);
        }
        else
        {
            worlds.AddRange(configs.OrderBy(x => x.Id).Select(config =>
                new World(config, config.PlayerName, config.Id, config.PlayerGuid, config.IsLocalConfig)));
            _logger.LogDebug(
                "Seed: {SeedNumber} | Race: {PrimaryConfigRace} | World Count: {Count}",
                seedNumber, primaryConfig.Race, configs.Count);
        }

        Filler.SetRandom(rng);
        Filler.Fill(worlds, primaryConfig, cancellationToken);

        var playthrough = _playthroughService.Generate(worlds, primaryConfig);
        var seedData = new SeedData
        (
            guid: Guid.NewGuid().ToString("N"),
            seed: seed ?? primaryConfig.Seed,
            game: Name,
            mode: primaryConfig.GameMode.ToLowerString(),
            worldGenerationData: new WorldGenerationDataCollection(),
            playthrough: primaryConfig.Race ? new Playthrough(primaryConfig, Enumerable.Empty<Playthrough.Sphere>()) : playthrough,
            configs: configs,
            primaryConfig: primaryConfig
        );

        if (primaryConfig.GenerateSeedOnly)
        {
            seedData.WorldGenerationData.AddRange(worlds.Select(x => new WorldGenerationData(x)));
            return seedData;
        }

        /* Make sure RNG is the same when applying patches to the ROM to have consistent RNG for seed identifer etc */
        var patchSeed = rng.Next();
        foreach (var world in worlds)
        {
            _hintService.GetInGameHints(world, worlds, playthrough, rng.Next());
            var patches = _patcherService.GetPatches(new GetPatchesRequest()
            {
                World = world,
                Worlds = worlds,
                SeedGuid = seedData.Guid,
                Seed = primaryConfig.Race ? 0 : seedNumber,
                Random = new Random(patchSeed).Sanitize()
            });
            var worldGenerationData = new WorldGenerationData(world, patches);
            _logger.LogInformation("Patch created for world {WorldId}", world.Id);
            seedData.WorldGenerationData.Add(worldGenerationData);
        }

        // Mark Agahnim and the Golden Four Metroid bosses as known
        var bossRewardRegions = worlds.SelectMany(x => x.BossRegions)
            .Where(x => x.BossType is BossType.Agahnim || x.BossType.IsInCategory(BossCategory.GoldenFour))
            .OfType<IHasReward>();
        foreach (var region in bossRewardRegions)
        {
            region.MarkedReward = region.RewardType;
        }

        Debug.WriteLine("Generated seed on randomizer instance " + GetHashCode());
        _logger.LogInformation("Generated seed successfully");
        _worldAccessor.World = worlds.First(x => x.IsLocalWorld);
        _worldAccessor.Worlds = worlds;
        return seedData;
    }

    /// <summary>
    /// Ensures that a generated seed matches the requested preferences
    /// </summary>
    /// <param name="seedData">The seed data that contains the generated spheres to guarantee early items</param>
    /// <returns>True if the seed matches all config settings, false otherwise</returns>
    public bool ValidateSeedSettings(SeedData seedData)
    {
        // Go through and make sure specified locations are populated correctly
        foreach (var world in seedData.WorldGenerationData.Worlds)
        {
            var config = world.Config;
            var configLocations = config.LocationItems;

            foreach (var (locationId, value) in configLocations)
            {
                var location = world.Locations.First(x => x.Id == locationId);
                if (value < Enum.GetValues(typeof(ItemPool)).Length)
                {
                    var itemPool = (ItemPool)value;
                    if ((itemPool == ItemPool.Progression && !location.Item.Progression) || (itemPool == ItemPool.Junk && !location.Item.Type.IsInCategory(ItemCategory.Junk)))
                    {
                        _logger.LogInformation(
                            "Location {LocationName} did not have the correct ItemType of {ItemPool}. Actual item: {ItemName}",
                            location.Name, itemPool, location.Item.Name);
                        return false;
                    }
                }
                else
                {
                    var itemType = (ItemType)value;
                    if (location.Item.Type != itemType)
                    {
                        _logger.LogInformation(
                            "Location {LocationName} did not have the correct Item of {ItemType}. Actual item: {ItemName}",
                            location.Name, itemType, location.Item.Name);
                        return false;
                    }
                }
            }

            // Through and make sure the early items are populated in early spheres
            foreach (var itemType in ItemSettingOptions.GetEarlyItemTypes(config))
            {
                var sphereIndex = seedData.Playthrough.Spheres.IndexOf(x => x.Items.Any(y => y.Progression && y.Type == itemType));
                if (sphereIndex > 2)
                {
                    _logger.LogInformation("Item {ItemType} did not show up early as expected. Sphere: {SphereIndex}", itemType, sphereIndex);
                    return false;
                }
            }
        }

        return true;
    }
}
