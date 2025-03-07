﻿using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;

namespace TrackerCouncil.Smz3.Tracking;

/// <summary>
/// Provides natural language functionality.
/// </summary>
internal static class NaturalLanguage
{
    /// <summary>
    /// Returns a string representing a collection of locations as a
    /// comma-separated list, but where the last location is separated with
    /// "and" instead.
    /// </summary>
    /// <param name="locations">The locations.</param>
    /// <param name="cap">The cap of locations to fully name.</param>
    /// <returns>
    /// A string containing each item in <paramref name="locations"/> separated
    /// with a comma, except for the two items, which are separated with
    /// "and".
    /// </returns>
    /// <example>
    /// <c>NaturalLanguage.Join(new[]{"one", "two", "three"})</c> returns
    /// <c>"one, two and three"</c>.
    /// </example>
    public static string Join(ICollection<Location> locations, int cap = 4)
    {
        if (locations.Count == 0)
        {
            return string.Empty;
        }
        else if (locations.Count == 1)
        {
            return locations.First().RandomName;
        }
        else if (locations.Count <= cap)
        {
            var last = locations.Last().RandomName;
            var remainder = locations.SkipLast(1).Select(x => x.RandomName);
            return $"{string.Join(", ", remainder)} and {last}";
        }
        else
        {
            var namedLocations = locations.Take(cap).Select(x => x.RandomName);
            return $"{string.Join(", ", namedLocations)} and {locations.Count - cap} other locations";
        }
    }

    /// <summary>
    /// Returns a string representing a collection of items as a
    /// comma-separated list, but where the last item is separated with
    /// "and" instead.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>
    /// A string containing each item in <paramref name="items"/> separated
    /// with a comma, except for the two items, which are separated with
    /// "and".
    /// </returns>
    /// <example>
    /// <c>NaturalLanguage.Join(new[]{"one", "two", "three"})</c> returns
    /// <c>"one, two and three"</c>.
    /// </example>
    public static string Join(IEnumerable<string> items)
    {
        var last = items.LastOrDefault();
        if (last == null)
            return string.Empty;

        var remainder = items.SkipLast(1);
        if (!remainder.Any())
            return last;

        return $"{string.Join(", ", remainder)} and {last}";
    }

    /// <summary>
    /// Returns a string representing a collection of items as a
    /// comma-separated list, but where the last item is separated with
    /// "and" instead.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <param name="config">The configuration, used to determine which items to mention.</param>
    /// <returns>
    /// A string containing each item in <paramref name="items"/> separated
    /// with a comma, except for the two items, which are separated with
    /// "and".
    /// </returns>
    public static string Join(IEnumerable<Item> items, Config config)
    {
        var groupedItems = items.GroupBy(x => x.Type, (type, innerItems) =>
        {
            var item = innerItems.First(); // Just pick the first. It's possible (though unlikely) there's multiple items for a single item type
            var count = innerItems.Count();
            return (item, count);
        }).ToList();

        var interestingItems = groupedItems.Where(x => x.item.Metadata.IsPossibleProgression(config, x.item.IsLocalPlayerItem)).ToList();
        var junkItems = groupedItems.Where(x => !x.item.Metadata.IsPossibleProgression(config, x.item.IsLocalPlayerItem)).OrderBy(x => x.item.IsDungeonItem).ToList();

        if (junkItems.Count == 0)
        {
            return Join(interestingItems.Select(GetPhrase));
        }
        else if (interestingItems.Count + junkItems.Count < 5)
        {
            return Join(interestingItems.Concat(junkItems).Select(GetPhrase));
        }

        if (interestingItems.Count <= 3)
        {
            var numToTake = 3 - interestingItems.Count;
            interestingItems.AddRange(junkItems.Take(numToTake));
            junkItems = junkItems.Skip(numToTake).ToList();
        }

        return Join(interestingItems.Select(GetPhrase).Concat([$"{junkItems.Count} other items"]));

        static string GetPhrase((Item item, int count) x)
            => x.count > 1 ? $"{x.count} {x.item.Metadata.Plural ?? $"{x.item.Name}s"}": $"{x.item.Name}";
    }
}
