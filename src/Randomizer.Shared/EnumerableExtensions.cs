using System;
using System.Collections.Generic;

namespace Randomizer.Shared
{
    /// <summary>
    /// Provides additional functionality on collections.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines the position of the first occurence of a matching element
        /// in the collection.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements in <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// The collection to search for a matching element.
        /// </param>
        /// <param name="predicate">
        /// A predicate used to determine which element to find.
        /// </param>
        /// <returns>
        /// The index of the first element for which <paramref
        /// name="predicate"/> returns <c>true</c>; or <c>-1</c> if none of the
        /// elements in <paramref name="source"/> match.
        /// </returns>
        public static int IndexOf<T>(this IReadOnlyList<T> source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                    return i;
            }

            return -1;
        }
    }
}
