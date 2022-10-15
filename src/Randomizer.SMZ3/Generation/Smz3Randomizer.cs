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
            => GenerateSeed(new List<Config> { config }, "", cancellationToken);

        public SeedData GenerateSeed(Config config, string seed, CancellationToken cancellationToken = default)
            => GenerateSeed(new List<Config>() { config }, seed, cancellationToken);

        public SeedData GenerateSeed(List<Config> configs, string seed, CancellationToken cancellationToken = default)
        {
            var primaryConfig = configs.First();

            var seedNumber = ParseSeed(ref seed);
            var rng = new Random(seedNumber);
            primaryConfig.Seed = seedNumber.ToString();
            if (primaryConfig.Race)
                rng = new Random(rng.Next());
            primaryConfig.SettingsString = Config.ToConfigString(primaryConfig, true);

            _logger.LogDebug($"Seed: {seedNumber} | Race: {primaryConfig.Race} | Keysanity: {primaryConfig.KeysanityMode} | Item placement: {primaryConfig.ItemPlacementRule} | World count : {configs.Count()}");

            /*primaryConfig.GameMode = GameMode.Multiworld;

            // Testing code. TODO: Remove
            if (primaryConfig.MultiWorld)
            {
                for (var i = 0; i < 4; i++)
                {
                    configs.Add(primaryConfig);
                }
            }*/

            var worlds = new List<World>();
            if (primaryConfig.SingleWorld)
                worlds.Add(new World(primaryConfig, "Player", 0, Guid.NewGuid().ToString("N")));
            else
            {
                for (var playerId = 0; playerId < configs.Count; playerId++)
                {
                    var player = "test" + playerId;
                    worlds.Add(new World(configs[playerId], player, playerId, Guid.NewGuid().ToString("N")));
                }
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
            Filler.Fill(worlds, primaryConfig, cancellationToken);

            var playthrough = Playthrough.Generate(worlds, primaryConfig);
            var seedData = new SeedData
            {
                Guid = Guid.NewGuid().ToString("N"),
                Seed = seed,
                Game = Name,
                Mode = primaryConfig.GameMode.ToLowerString(),
                Playthrough = primaryConfig.Race ? new Playthrough(primaryConfig, Enumerable.Empty<Playthrough.Sphere>()) : playthrough,
                Worlds = new List<(World World, Dictionary<int, byte[]> Patches)>(),
                Hints = new()
            };

            if (primaryConfig.GenerateSeedOnly)
            {
                seedData.Worlds = worlds.Select(x => (x, (Dictionary<int, byte[]>)null)).ToList();
                return seedData;
            }

            /* Make sure RNG is the same when applying patches to the ROM to have consistent RNG for seed identifer etc */
            var patchSeed = rng.Next();
            foreach (var world in worlds)
            {
                var patchRnd = new Random(patchSeed);
                var hints = _hintService.GetInGameHints(world, worlds, playthrough, rng.Next());
                seedData.Hints.Add((world, hints.ToList()));
                var patch = new Patcher(world, worlds, seedData.Guid, primaryConfig.Race ? 0 : seedNumber, patchRnd, _metadataService, _gameLines);
                seedData.Worlds.Add((world, patch.CreatePatch(world.Config, hints)));
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
        /// <returns>True if the seed matches all config settings, false otherwise</returns>
        public bool ValidateSeedSettings(SeedData seedData)
        {
            // Go through and make sure specified locations are populated correctly
            foreach (var world in seedData.Worlds.Select(x => x.World))
            {
                var config = world.Config;
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
