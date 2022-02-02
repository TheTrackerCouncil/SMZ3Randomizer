using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
