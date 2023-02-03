using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.FileData;

namespace Randomizer.SMZ3.Generation
{
    public class Smz3Plandomizer : IRandomizer
    {
        private readonly PlandoFillerFactory _fillerFactory;
        private readonly IWorldAccessor _worldAccessor;
        private readonly ILogger<Smz3Plandomizer> _logger;
        private readonly IMetadataService _metadataService;
        private readonly GameLinesConfig _gameLines;
        private readonly IGameHintService _hintService;

        public Smz3Plandomizer(PlandoFillerFactory fillerFactory, IWorldAccessor worldAccessor, Configs configs, IMetadataService metadataService, IGameHintService gameHintGenerator, ILogger<Smz3Plandomizer> logger)
        {
            _fillerFactory = fillerFactory;
            _worldAccessor = worldAccessor;
            _logger = logger;
            _gameLines = configs.GameLines;
            _metadataService = metadataService;
            _hintService = gameHintGenerator;
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
                playthrough = Playthrough.Generate(worlds, config);
            }
            catch (RandomizerGenerationException ex)
            {
                _logger.LogWarning(ex, "Encountered playthrough simulation exception");
                playthrough = new Playthrough(config, Enumerable.Empty<Playthrough.Sphere>());
            }

            var plandoName = config.PlandoConfig?.FileName ?? "unknown";

            // If matching base plando file name, just use the date for the seed name
            if (Regex.IsMatch(plandoName, "^Spoiler_Plando_(.*)_[0-9]+$"))
            {
                plandoName = Regex.Replace(plandoName, "(^Spoiler_Plando_|_[0-9]+$)", "");
            }

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
                var patchRnd = new Random().Sanitize();
                var patch = new Patcher(world, worlds, seedData.Guid, 0, patchRnd, _metadataService, _gameLines, _logger);
                var hints = _hintService.GetInGameHints(world, worlds, playthrough, 0);
                var worldGenerationData = new WorldGenerationData(world, patch.CreatePatch(config, hints), hints);
                seedData.WorldGenerationData.Add(worldGenerationData);
            }

            Debug.WriteLine("Generated seed on randomizer instance " + GetHashCode());
            _worldAccessor.World = worlds[0];
            _worldAccessor.Worlds = worlds;
            return seedData;
        }
    }
}
