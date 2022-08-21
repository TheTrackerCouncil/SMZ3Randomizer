using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.FileData;

namespace Randomizer.SMZ3.Generation
{
    public class Smz3Plandomizer : IRandomizer
    {
        private readonly PlandoFillerFactory _fillerFactory;
        private readonly IWorldAccessor _worldAccessor;
        private readonly ILogger<Smz3Plandomizer> _logger;

        public Smz3Plandomizer(PlandoFillerFactory fillerFactory, IWorldAccessor worldAccessor, ILogger<Smz3Plandomizer> logger)
        {
            _fillerFactory = fillerFactory;
            _worldAccessor = worldAccessor;
            _logger = logger;
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

            var seedData = new SeedData
            {
                Guid = Guid.NewGuid().ToString("N"),
                Seed = $"Plando: {config.PlandoConfig?.FileName ?? "unknown"}",
                Game = "SMZ3 Cas’ Plando",
                Mode = config.GameMode.ToLowerString(),
                Playthrough = config.Race ? new Playthrough(config, Enumerable.Empty<Playthrough.Sphere>()) : playthrough,
                Worlds = new List<(World World, Dictionary<int, byte[]> Patches)>()
            };

            foreach (var world in worlds)
            {
                var patchRnd = new Random();
                var patch = new Patcher(world, worlds, seedData.Guid, 0, patchRnd);
                seedData.Worlds.Add((world, patch.CreatePatch(config)));
            }

            Debug.WriteLine("Generated seed on randomizer instance " + GetHashCode());
            _worldAccessor.World = worlds[0];
            return seedData;
        }
    }
}
