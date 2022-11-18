using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    public class Smz3Randomizer : ISeededRandomizer
    {
        private static readonly Regex s_illegalCharacters = new(@"[^A-Z0-9]", RegexOptions.IgnoreCase);
        private static readonly Regex s_continousSpace = new(@" +");
        private readonly IWorldAccessor _worldAccessor;
        private readonly ILogger<Smz3Randomizer> _logger;
        private readonly IMetadataService _metadataService;
        private readonly GameLinesConfig _gameLines;
        private readonly IGameHintService _hintService;

        public Smz3Randomizer(IFiller filler, IWorldAccessor worldAccessor, Configs configs, IMetadataService metadataService, IGameHintService gameHintGenerator, ILogger<Smz3Randomizer> logger)
        {
            Filler = filler;
            _worldAccessor = worldAccessor;
            _logger = logger;
            _gameLines = configs.GameLines;
            _metadataService = metadataService;
            _hintService = gameHintGenerator;
        }

        public static string Name => "Super Metroid & A Link to the Past Cas’ Randomizer";

        public static Version Version => new(2, 0);

        public SeedData LastGeneratedSeed { get; private set; }

        protected IFiller Filler { get; }

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

        public SeedData GenerateSeed(Config config, CancellationToken cancellationToken = default)
            => GenerateSeed(config, "", cancellationToken);

        public SeedData GenerateSeed(Config config, string seed, CancellationToken cancellationToken = default)
        {
            var seedNumber = ParseSeed(ref seed);
            var rng = new Random(seedNumber);
            config.Seed = seedNumber.ToString();
            if (config.Race)
                rng = new Random(rng.Next());
            config.SettingsString = Config.ToConfigString(config, true);

            _logger.LogDebug($"Seed: {seedNumber} | Race: {config.Race} | Keysanity: {config.KeysanityMode} | Item placement: {config.ItemPlacementRule}");

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

            Filler.SetRandom(rng);
            Filler.Fill(worlds, config, cancellationToken);

            var playthrough = Playthrough.Generate(worlds, config);
            var seedData = new SeedData
            {
                Guid = Guid.NewGuid().ToString("N"),
                Seed = seed,
                Game = Name,
                Mode = config.GameMode.ToLowerString(),
                Playthrough = config.Race ? new Playthrough(config, Enumerable.Empty<Playthrough.Sphere>()) : playthrough,
                Worlds = new List<(World World, Dictionary<int, byte[]> Patches)>()
            };

            if (config.GenerateSeedOnly)
            {
                seedData.Worlds = worlds.Select(x => (x, (Dictionary<int, byte[]>)null)).ToList();
                return seedData;
            }

            var hints = _hintService.GetInGameHints(worlds[0], worlds, playthrough, config.UniqueHintCount, rng.Next());

            /* Make sure RNG is the same when applying patches to the ROM to have consistent RNG for seed identifer etc */
            var patchSeed = rng.Next();
            foreach (var world in worlds)
            {
                var patchRnd = new Random(patchSeed);
                var patch = new Patcher(world, worlds, seedData.Guid, config.Race ? 0 : seedNumber, patchRnd, _metadataService, _gameLines);
                seedData.Worlds.Add((world, patch.CreatePatch(config, hints)));
            }

            Debug.WriteLine("Generated seed on randomizer instance " + GetHashCode());
            _worldAccessor.World = worlds[0];
            _worldAccessor.Worlds = worlds;
            LastGeneratedSeed = seedData;
            return seedData;
        }

        /// <summary>
        /// Ensures that a generated seed matches the requested preferences
        /// </summary>
        /// <param name="seedData">The seed data that contains the generated spheres to guarantee early items</param>
        /// <param name="config">The confirm with the seed generation settings</param>
        /// <returns>True if the seed matches all config settings, false otherwise</returns>
        public bool ValidateSeedSettings(SeedData seedData, Config config)
        {
            // Go through and make sure specified locations are populated correctly
            foreach (var world in seedData.Worlds.Select(x => x.World))
            {
                var configLocations = config.LocationItems;

                foreach ((var locationId, var value) in configLocations)
                {
                    var location = world.Locations.First(x => x.Id == locationId);
                    if (value < Enum.GetValues(typeof(ItemPool)).Length)
                    {
                        var itemPool = (ItemPool)value;
                        if ((itemPool == ItemPool.Progression && !location.Item.Progression) || (itemPool == ItemPool.Junk && !location.Item.Type.IsInCategory(ItemCategory.Junk)))
                        {
                            _logger.LogInformation($"Location {location.Name} did not have the correct ItemType of {itemPool}. Actual item: {location.Item.Name}");
                            return false;
                        }
                    }
                    else
                    {
                        var itemType = (ItemType)value;
                        if (location.Item.Type != itemType)
                        {
                            _logger.LogInformation($"Location {location.Name} did not have the correct Item of {itemType}. Actual item: {location.Item.Name}");
                            return false;
                        }
                    }
                }
            }

            // Through and make sure the early items are populated in early spheres
            foreach (var itemType in config.EarlyItems)
            {
                var sphereIndex = seedData.Playthrough.Spheres.IndexOf(x => x.Items.Any(y => y.Progression && y.Type == itemType));
                if (sphereIndex > 2)
                {
                    _logger.LogInformation($"Item {itemType} did not show up early as expected. Sphere: {sphereIndex}");
                    return false;
                }
            }

            return true;
        }

        private static string CleanPlayerName(string name)
        {
            name = s_illegalCharacters.Replace(name, " ");
            name = s_continousSpace.Replace(name, " ");
            return name.Trim();
        }
    }
}
