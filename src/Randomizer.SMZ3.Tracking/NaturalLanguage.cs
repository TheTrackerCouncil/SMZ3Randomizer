using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides natural language functionality.
    /// </summary>
    internal static class NaturalLanguage
    {
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
            var groupedItems = items.GroupBy(x => x.Type, (type, items) =>
            {
                var item = items.First(); // Just pick the first. It's possible (though unlikely) there's multiple items for a single item type
                var count = items.Count();
                return (item, count);
            });

            var interestingItems = groupedItems.Where(x => !x.item.Metadata.IsJunk(config)).ToList();
            var junkItems = groupedItems.Where(x => x.item.Metadata.IsJunk(config)).ToList();

            if (junkItems.Count == 0)
                return Join(interestingItems.Select(GetPhrase));

            if (interestingItems.Count == 0)
                return Join(junkItems.Select(GetPhrase));

            if (junkItems.Count > 1)
                return Join(interestingItems.Select(GetPhrase).Concat(new[] { $"{junkItems.Count} other items" }));

            return Join(groupedItems.Select(GetPhrase));

            static string GetPhrase((Item item, int count) x)
                => x.count > 1 ? $"{x.count} {x.item.Metadata.Plural ?? $"{x.item.Name}s"}": $"{x.item.Name}"; ;
        }
    }
}
