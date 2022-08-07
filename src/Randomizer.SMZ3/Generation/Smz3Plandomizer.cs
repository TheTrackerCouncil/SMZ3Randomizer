using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.FileData;
using Randomizer.SMZ3.Infrastructure;

namespace Randomizer.SMZ3.Generation
{
    public class Smz3Plandomizer : IRandomizer
    {
        private readonly PlandoFiller _plandoFiller;

        public Smz3Plandomizer(PlandoFiller plandoFiller)
        {
            _plandoFiller = plandoFiller;
        }

        public SeedData GenerateSeed(Config config, CancellationToken cancellationToken = default)
        {
            var worlds = new List<World>
            {
                // TODO: How to get keysanity in here? Maybe pass plando config
                // around in Config and construct PlandoFiller here using
                // injected factory?
                new World(config, "Player", 0, Guid.NewGuid().ToString("N"))
            };

            _plandoFiller.Fill(worlds, config, cancellationToken);

            var seedData = new SeedData
            {
                Guid = Guid.NewGuid().ToString("N"),
                Seed = seed,
                Game = Name,
                Mode = config.GameMode.ToLowerString(),
                Logic = $"{config.SMLogic.ToLowerString()}+{config.Z3Logic.ToLowerString()}",
                Playthrough = config.Race ? new Playthrough(config, Enumerable.Empty<Playthrough.Sphere>()) : playthrough,
                Worlds = new List<(World World, Dictionary<int, byte[]> Patches)>()
            };


            /* Make sure RNG is the same when applying patches to the ROM to have consistent RNG for seed identifer etc */
            var patchSeed = rng.Next();
            foreach (var world in worlds)
            {
                var patchRnd = new Random(patchSeed);
                var patch = new Patcher(world, worlds, seedData.Guid, config.Race ? 0 : seedNumber, patchRnd);
                seedData.Worlds.Add((world, patch.CreatePatch(config)));
}

            Debug.WriteLine("Generated seed on randomizer instance " + GetHashCode());
            _worldAccessor.World = worlds[0];
            LastGeneratedSeed = seedData;
            return seedData;
        }
    }
}
