using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Shared.Enums;

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

                foreach (var location in x.Locations.Where(l => l.IsPotentiallyImportant(Config.KeysanityMode)))
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
            public List<Location> Locations { get; } = new();

            public List<Item> Items { get; } = new();

            public List<Location> InaccessibleLocations { get; } = new();
        }
    }
}
