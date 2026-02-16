using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;

namespace TrackerCouncil.Smz3.Data.GeneratedData;

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
        public List<Location> Locations { get; } = [];

        public List<Item> Items { get; } = [];

        public List<Location> InaccessibleLocations { get; } = [];

        public List<Item> StartingItems { get; } = [];
    }
}
