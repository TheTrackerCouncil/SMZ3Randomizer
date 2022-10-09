using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IGameHintGenerator _gameHintGenerator;

        public Smz3Plandomizer(PlandoFillerFactory fillerFactory, IWorldAccessor worldAccessor, ILogger<Smz3Plandomizer> logger, IServiceProvider serviceProvider)
        {
            _fillerFactory = fillerFactory;
            _worldAccessor = worldAccessor;
            _logger = logger;

            var scope = serviceProvider.CreateScope();
            _metadataService = scope.ServiceProvider.GetService<IMetadataService>();
            var configs = scope.ServiceProvider.GetService<Configs>();
            _gameLines = configs.GameLines;
            _gameHintGenerator = scope.ServiceProvider.GetService<IGameHintGenerator>();
        }

        public SeedData GenerateSeed(Config config, CancellationToken cancellationToken = default)
        {
            var worlds = new List<World>
            {
                new World(config, "Player", 0, Guid.NewGuid().ToString("N"))
            };

            var filler = _fillerFactory.Create(config.PlandoConfig);
            filler.Fill(worlds, config, cancellationToken);

            Playthrough playthrough = null;
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
            {
                Guid = Guid.NewGuid().ToString("N"),
                Seed = $"Plando: {plandoName}",
                Game = "SMZ3 Cas’ Plando",
                Mode = config.GameMode.ToLowerString(),
                Playthrough = config.Race ? new Playthrough(config, Enumerable.Empty<Playthrough.Sphere>()) : playthrough,
                Worlds = new List<(World World, Dictionary<int, byte[]> Patches)>()
            };

            var hints = _gameHintGenerator.GetHints(worlds[0], worlds, playthrough, 8, 0);

            foreach (var world in worlds)
            {
                var patchRnd = new Random();
                var patch = new Patcher(world, worlds, seedData.Guid, 0, patchRnd, _metadataService, _gameLines);
                seedData.Worlds.Add((world, patch.CreatePatch(config, hints)));
            }

            Debug.WriteLine("Generated seed on randomizer instance " + GetHashCode());
            _worldAccessor.World = worlds[0];
            _worldAccessor.Worlds = worlds;
            return seedData;
        }
    }
}
