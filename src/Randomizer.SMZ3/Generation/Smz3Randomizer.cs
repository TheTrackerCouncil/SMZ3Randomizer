using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Randomizer.Shared;
using Randomizer.SMZ3.FileData;

namespace Randomizer.SMZ3.Generation
{
    public class Smz3Randomizer : IWorldAccessor
    {
        public static readonly Version version = new Version(1, 0);

        private static readonly Regex legalCharacters = new Regex(@"[A-Z0-9]", RegexOptions.IgnoreCase);
        private static readonly Regex illegalCharacters = new Regex(@"[^A-Z0-9]", RegexOptions.IgnoreCase);
        private static readonly Regex continousSpace = new Regex(@" +");

        public static string Name => "Super Metroid & A Link to the Past Cas’ Randomizer";

        public World LastGeneratedWorld { get; private set; }

        public SeedData LastGeneratedSeed { get; private set; }

        public static int ParseSeed(ref string input)
        {
            int seed;
            if (string.IsNullOrEmpty(input))
            {
                seed = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue);
            }
            else
            {
                input = input.Trim();
                if (!Parse.AsInteger(input, out seed) // Accept plain ints as seeds (i.e. mostly original behavior)
                    && !Parse.AsHex(input, out seed)) // Accept hex seeds (e.g. seed as stored in ROM info)
                {
                    // When all else fails, accept any other input by hashing it
                    seed = NonCryptographicHash.Fnv1a(input);
                }
            }

            input = seed.ToString("D", CultureInfo.InvariantCulture);
            return seed;
        }

        public SeedData GenerateSeed(Config config, string seed, CancellationToken cancellationToken)
        {
            var seedNumber = ParseSeed(ref seed);
            var rng = new Random(seedNumber);
            if (config.Race)
                rng = new Random(rng.Next());

            var worlds = new List<World>();
            if (config.SingleWorld)
                worlds.Add(new World(config, "Player", 0, Guid.NewGuid().ToString("N")));
            else
            {
                throw new NotSupportedException("Multiworld seeds are currently not supported.");
                //var players = options.ContainsKey("players") ? int.Parse(options["players"]) : 1;
                //for (var p = 0; p < players; p++)
                //{
                //    var found = options.TryGetValue($"player-{p}", out var player);
                //    if (!found)
                //        throw new ArgumentException($"No name provided for player {p + 1}");
                //    if (!legalCharacters.IsMatch(player))
                //        throw new ArgumentException($"No alphanumeric characters found in name for player {p + 1}");
                //    player = CleanPlayerName(player);
                //    worlds.Add(new World(config, player, p, Guid.NewGuid().ToString("N")));
                //}
            }

            var filler = new Filler(worlds, config, rng, cancellationToken);
            filler.Fill();

            var playthrough = new Playthrough(worlds, config);
            var spheres = playthrough.Generate();

            var seedData = new SeedData
            {
                Guid = Guid.NewGuid().ToString("N"),
                Seed = seed,
                Game = Name,
                Mode = config.GameMode.ToLowerString(),
                Logic = $"{config.SMLogic.ToLowerString()}+{config.Z3Logic.ToLowerString()}",
                Playthrough = config.Race ? new List<Dictionary<string, string>>() : spheres,
                Worlds = new List<(World World, Dictionary<int, byte[]> Patches)>()
            };

            if (config.GenerateSeedOnly)
            {
                seedData.Worlds = worlds.Select(x => (x, (Dictionary<int, byte[]>)null)).ToList();
                return seedData;
            }

            /* Make sure RNG is the same when applying patches to the ROM to have consistent RNG for seed identifer etc */
            var patchSeed = rng.Next();
            foreach (var world in worlds)
            {
                var patchRnd = new Random(patchSeed);
                var patch = new Patcher(world, worlds, seedData.Guid, config.Race ? 0 : seedNumber, patchRnd);
                seedData.Worlds.Add((world, patch.CreatePatch(config)));
            }

            Debug.WriteLine("Generated seed on randomizer instance " + GetHashCode());
            LastGeneratedWorld = worlds[0];
            LastGeneratedSeed = seedData;
            return seedData;
        }

        public World GetWorld()
        {
            Debug.WriteLine("Retrieving world on randomizer instance " + GetHashCode());
            return LastGeneratedWorld ?? new World(new Config(), "", 0, "");
        }

        private static string CleanPlayerName(string name)
        {
            name = illegalCharacters.Replace(name, " ");
            name = continousSpace.Replace(name, " ");
            return name.Trim();
        }
    }
}
