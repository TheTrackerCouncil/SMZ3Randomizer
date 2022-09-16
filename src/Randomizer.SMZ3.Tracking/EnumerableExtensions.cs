using System;
using System.Collections.Generic;

using BunLabs;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides additional functionality for collections.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns a random element from the collection.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements in the collection.
        /// </typeparam>
        /// <param name="source">The collection to choose from.</param>
        /// <returns>
        /// A random element from <paramref name="source"/>, or a default value
        /// if <paramref name="source"/> is empty.
        /// </returns>
        public static T? Random<T>(this IEnumerable<T> source)
            => source.Random(Rng.Current);
    }
}
