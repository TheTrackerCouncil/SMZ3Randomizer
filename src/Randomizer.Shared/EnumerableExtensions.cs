using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Filters a sequence of nullable values and returns only elements that
        /// have a value.
        /// </summary>
        /// <typeparam name="T">
        /// The type of element in <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">The collection to filter on.</param>
        /// <returns>
        /// A new collection that has no <see langword="null"/> values.
        /// </returns>
        public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> source)
        {
            return source.Where(x => x != null)
                .Select(x => x!);
        }

        public static List<T> Shuffle<T>(this IEnumerable<T> source, Random rnd)
        {
            var copy = new List<T>(source);
            var n = copy.Count;
            while ((n -= 1) > 0)
            {
                var k = rnd.Next(n + 1);
                (copy[n], copy[k]) = (copy[k], copy[n]);
            }
            return copy;
        }

        public static T? Random<T>(this IEnumerable<T> source, Random rnd)
        {
            var list = source.ToList();
            return list.Count == 0 ? default : list.ElementAt(rnd.Next(list.Count));
        }

        /// <summary>
        /// Returns a random element matching the specified predicate in a
        /// sequence.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements in <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">The collection to search in.</param>
        /// <param name="predicate">
        /// A function to test each elements for a condition.
        /// </param>
        /// <param name="rng">
        /// The random number generator used to pick a random matching element.
        /// </param>
        /// <returns>
        /// A random element from <paramref name="source"/> that matches
        /// <paramref name="predicate"/>, or <see langword="null"/> if no
        /// elements in <paramref name="source"/> match <paramref
        /// name="predicate"/>.
        /// </returns>
        public static T? RandomOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, Random rng)
        {
            var matchingElements = source.Where(predicate).ToList();
            if (matchingElements.Count == 0)
                return default;

            var randomIndex = rng.Next(matchingElements.Count);
            return matchingElements.ElementAt(randomIndex);
        }

        public static (IEnumerable<T>, IEnumerable<T>) SplitOff<T>(this IEnumerable<T> source, int count)
        {
            var head = source.Take(count);
            var tail = source.Skip(count);
            return (head, tail);
        }

        public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out IEnumerable<T> rest)
        {
            first = source.First();
            rest = source.Skip(1);
        }

        public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out T second, out IEnumerable<T> rest)
            => (first, (second, rest)) = source;

        public static T MoveToTop<T>(this List<T> source, Predicate<T> match)
        {
            var item = source.Find(match)
                ?? throw new ArgumentException($"Could not find a matching item in the collection.", nameof(match));
            source.Remove(item);
            source.Insert(0, item);
            return item;
        }

        /// <summary>
        /// Returns the only element in a sequence, or a default value if the
        /// sequence does not have exactly one element.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">The sequence.</param>
        /// <returns>
        /// The only element in <paramref name="source"/> if it has exactly one
        /// element; otherwise, the default value for <typeparamref name="T"/>.
        /// </returns>
        public static T? TrySingle<T>(this IEnumerable<T> source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var items = source.Take(2).ToList();
            return items.Count == 1 ? items[0] : default;
        }

        /// <summary>
        /// Returns the only element in a sequence, or a default value if the
        /// sequence does not have exactly one element.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">The sequence.</param>
        /// <param name="predicate">
        /// A function to test each element for a condition.
        /// </param>
        /// <returns>
        /// The only element in <paramref name="source"/> if it has exactly one
        /// element; otherwise, the default value for <typeparamref name="T"/>.
        /// </returns>
        public static T? TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return source.Where(predicate).TrySingle();
        }

        /// <summary>
        /// Filters a sequence based on a predicate if the specified condition
        /// is <c>true</c>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements in <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">The sequence.</param>
        /// <param name="condition">
        /// The condition to test before applying the filter.
        /// </param>
        /// <param name="predicate">The filter to apply.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> that contains elements from
        /// <paramref name="source"/>. If <paramref name="condition"/> is
        /// <c>true</c>, only elements matching <paramref name="predicate"/> are
        /// returned. Otherwise, all elements are returned.
        /// </returns>
        public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// Filters a sequence based on a predicate if the specified condition
        /// is <c>false</c>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements in <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">The sequence.</param>
        /// <param name="condition">
        /// The condition to test before applying the filter.
        /// </param>
        /// <param name="predicate">The filter to apply.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> that contains elements from
        /// <paramref name="source"/>. If <paramref name="condition"/> is
        /// <c>false</c>, only elements matching <paramref name="predicate"/>
        /// are returned. Otherwise, all elements are returned.
        /// </returns>
        public static IEnumerable<T> WhereUnless<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
        {
            return source.WhereIf(!condition, predicate);
        }

    }
}
