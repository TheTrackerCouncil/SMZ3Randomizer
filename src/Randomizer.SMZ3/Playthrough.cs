using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Randomizer.Shared;
using Randomizer.SMZ3.Regions;

namespace Randomizer.SMZ3
{
    public class Playthrough
    {
        public Playthrough(Config config, IEnumerable<Sphere> spheres)
        {
            Config = config;
            Spheres = spheres.ToImmutableList();
        }

        public Config Config { get; }

        public IReadOnlyList<Sphere> Spheres { get; }

        public static bool IsImportant(Location location, KeysanityMode keysanity)
            => location.Item.Progression
            || (location.Item.IsDungeonItem && keysanity is KeysanityMode.Both or KeysanityMode.Zelda)
            || (location.Item.IsKeycard && keysanity is KeysanityMode.Both or KeysanityMode.SuperMetroid);

        /// <summary>
        /// Simulates a rough playthrough and returns a value indicating whether
        /// the playthrough is likely possible.
        /// </summary>
        /// <param name="worlds">A list of the worlds to play through.</param>
        /// <param name="config">The config used to generate the worlds.</param>
        /// <param name="playthrough">
        /// If this method returns <see langword="true"/>, the generated
        /// playthrough; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the playthrough is possible; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public static bool TryGenerate(IReadOnlyCollection<World> worlds, Config config, [MaybeNullWhen(true)] out Playthrough playthrough)
        {
            try
            {
                playthrough = Generate(worlds, config);
                return true;
            }
            catch (RandomizerGenerationException)
            {
                playthrough = null;
                return false;
            }
        }

        /// <summary>
        /// Simulates a rough playthrough of the specified worlds.
        /// </summary>
        /// <param name="worlds">A list of the worlds to play through.</param>
        /// <param name="config">The config used to generate the worlds.</param>
        /// <returns>A new <see cref="Playthrough"/>.</returns>
        /// <exception cref="RandomizerGenerationException">
        /// The seed is likely impossible.
        /// </exception>
        public static Playthrough Generate(IReadOnlyCollection<World> worlds, Config config)
        {
            var spheres = new List<Sphere>();
            var locations = new List<Location>();
            var items = new List<Item>();

            var allRewards = worlds.SelectMany(w => Reward.CreatePool(w));
            var rewardRegions = worlds.SelectMany(w => w.Regions).OfType<IHasReward>();
            var regions = new List<Region>();
            var rewards = new List<Reward>();

            foreach (var world in worlds)
            {
                if (!world.Config.MetroidKeysanity)
                {
                    items.AddRange(Item.CreateKeycards(world));
                }
            }

            var totalItemCount = worlds.SelectMany(w => w.Items).Count();
            while (items.Count < totalItemCount)
            {
                var sphere = new Sphere();

                var allLocations = worlds.SelectMany(w => w.Locations.Available(items.Where(i => i.World == w), rewards.Where(r => r.World == w)));
                var newLocations = allLocations.Except(locations).ToList();
                var newItems = newLocations.Select(l => l.Item).ToList();
                rewards = allRewards.Where(x => x.Region.CanComplete(new Progression(items, new List<Reward>()))).ToList();
                locations.AddRange(newLocations);
                items.AddRange(newItems);

                if (!newItems.Any())
                {
                    /* With no new items added we might have a problem, so list inaccessable items */
                    var inaccessibleLocations = worlds.SelectMany(w => w.Locations).Where(l => !locations.Contains(l)).ToList();
                    if (inaccessibleLocations.Select(l => l.Item).Count() >= (15 * worlds.Count))
                        throw new RandomizerGenerationException("Too many inaccessible items, seed likely impossible.");

                    sphere.InaccessibleLocations.AddRange(inaccessibleLocations);
                    break;
                }

                sphere.Locations.AddRange(newLocations);
                sphere.Items.AddRange(newItems);
                spheres.Add(sphere);

                if (spheres.Count > 100)
                    throw new RandomizerGenerationException("Too many spheres, seed likely impossible.");
            }

            return new Playthrough(config, spheres);
        }

        public List<Dictionary<string, string>> GetPlaythroughText()
        {
            return Spheres.Select(x =>
            {
                var text = new Dictionary<string, string>();

                foreach (var location in x.InaccessibleLocations)
                {
                    if (Config.MultiWorld)
                        text.Add($"Inaccessible Item: {location} ({location.Region.World.Player})", $"{location.Item.Name} ({location.Item.World.Player})");
                    else
                        text.Add($"Inaccessible Item: {location}", $"{location.Item.Name}");
                }

                foreach (var location in x.Locations.Where(x => IsImportant(x, Config.KeysanityMode)))
                {
                    if (Config.MultiWorld)
                        text.Add($"{location} ({location.Region.World.Player})", $"{location.Item.Name} ({location.Item.World.Player})");
                    else
                        text.Add($"{location}", $"{location.Item.Name}");
                }

                return text;
            }).ToList();
        }

        public class Sphere
        {
            public Sphere()
            {
            }

            public List<Location> Locations { get; } = new();

            public List<Item> Items { get; } = new();

            public List<Location> InaccessibleLocations { get; } = new();
        }
    }
}
