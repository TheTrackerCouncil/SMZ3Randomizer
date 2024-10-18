using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.FileData;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class Smz3Plandomizer : IRandomizer
{
    private readonly PlandoFillerFactory _fillerFactory;
    private readonly IWorldAccessor _worldAccessor;
    private readonly ILogger<Smz3Plandomizer> _logger;
    private readonly IPatcherService _patcherService;
    private readonly PlaythroughService _playthroughService;

    public Smz3Plandomizer(PlandoFillerFactory fillerFactory, IWorldAccessor worldAccessor,
        ILogger<Smz3Plandomizer> logger, IPatcherService patcherService, PlaythroughService playthroughService)
    {
        _fillerFactory = fillerFactory;
        _worldAccessor = worldAccessor;
        _logger = logger;
        _patcherService = patcherService;
        _playthroughService = playthroughService;
    }

    public SeedData GenerateSeed(Config config, CancellationToken cancellationToken = default)
    {
        if (config.PlandoConfig == null)
            throw new InvalidOperationException("No plando config provided for plandomizer");

        var worlds = new List<World>
        {
            new(config, "Player", 0, Guid.NewGuid().ToString("N"))
        };

        var filler = _fillerFactory.Create(config.PlandoConfig);
        filler.Fill(worlds, config, cancellationToken);

        Playthrough playthrough;
        try
        {
            playthrough = _playthroughService.Generate(worlds, config);
        }
        catch (RandomizerGenerationException ex)
        {
            _logger.LogWarning(ex, "Encountered playthrough simulation exception");
            playthrough = new Playthrough(config, Enumerable.Empty<Playthrough.Sphere>());
        }

        var plandoName = string.IsNullOrEmpty(config.PlandoConfig?.FileName)
            ? config.Seed
            : config.PlandoConfig.FileName;

        // If matching base plando file name, just use the date for the seed name
        if (Regex.IsMatch(plandoName, "^Spoiler_Plando_(.*)_[0-9]+$"))
        {
            plandoName = Regex.Replace(plandoName, "(^Spoiler_Plando_|_[0-9]+$)", "");
        }

        var seed = string.IsNullOrEmpty(config.Seed) ? plandoName : config.Seed;
        var seedNumber = ISeededRandomizer.ParseSeed(ref seed);
        var rng = new Random(seedNumber).Sanitize();

        var seedData = new SeedData
        (
            guid: Guid.NewGuid().ToString("N"),
            seed: $"Plando: {plandoName}",
            game: "SMZ3 Cas’ Plando",
            mode: config.GameMode.ToLowerString(),
            worldGenerationData: new WorldGenerationDataCollection(),
            playthrough: config.Race ? new Playthrough(config, Enumerable.Empty<Playthrough.Sphere>()) : playthrough,
            configs: new List<Config>() { config },
            primaryConfig: config
        );

        foreach (var world in worlds)
        {
            var patches = _patcherService.GetPatches(new GetPatchesRequest()
            {
                World = world,
                Worlds = worlds,
                SeedGuid = seedData.Guid,
                Seed = seedNumber,
                Random = rng,
                PlandoConfig = config.PlandoConfig ?? new PlandoConfig()
            });

            var worldGenerationData = new WorldGenerationData(world, patches);
            seedData.WorldGenerationData.Add(worldGenerationData);
        }

        Debug.WriteLine("Generated seed on randomizer instance " + GetHashCode());
        _worldAccessor.World = worlds[0];
        _worldAccessor.Worlds = worlds;
        return seedData;
    }
}
