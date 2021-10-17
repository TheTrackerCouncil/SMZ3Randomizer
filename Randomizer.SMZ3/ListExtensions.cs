using System;
using System.Collections.Generic;
using System.Linq;

namespace Randomizer.SMZ3
{
    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> source, Random rnd)
        {
            var list = source.ToList();
            return list.ElementAt(rnd.Next(list.Count));
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

        public static (IEnumerable<T>, IEnumerable<T>) SplitOff<T>(this IEnumerable<T> source, int count)
        {
            var head = source.Take(count);
            var tail = source.Skip(count);
            return (head, tail);
        }

        public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out IEnumerable<T> rest)
        {
            first = source.FirstOrDefault();
            rest = source.Skip(1);
        }

        public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out T second, out IEnumerable<T> rest)
            => (first, (second, rest)) = source;

        public static T MoveToTop<T>(this List<T> source, Predicate<T> match)
        {
            var item = source.Find(match)
                ?? throw new ArgumentException($"Could not find a matching item in the collection.", nameof(match));
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
        /// <param name="predicate">
        /// A function to test each element for a condition.
        /// </param>
        /// <returns>
        /// The only element in <paramref name="source"/> if it has exactly one
        /// element; otherwise, the default value for <typeparamref name="T"/>.
        /// </returns>
        public static T TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var items = source.Where(predicate).Take(2).ToList();
            return items.Count == 1 ? items[0] : default;
        }
    }
}
