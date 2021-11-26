using System;

namespace Randomizer.Shared
{
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether any of the specified strings occur within this
        /// string.
        /// </summary>
        /// <param name="text">The string to search in.</param>
        /// <param name="values">A collection of strings to seek.</param>
        /// <returns>
        /// <c>true</c> if any element of the specified <paramref
        /// name="values"/> can be found within this <paramref name="text"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAny(this string text, params string[] values)
            => text.ContainsAny(StringComparison.Ordinal, values);

        /// <summary>
        /// Determines whether any of the specified strings occur within this
        /// string, using the specified comparison rules.
        /// </summary>
        /// <param name="text">The string to search in.</param>
        /// <param name="comparisonType">
        /// Specifies the comparison rules to use.
        /// </param>
        /// <param name="values">A collection of strings to seek.</param>
        /// <returns>
        /// <c>true</c> if any element of the specified <paramref
        /// name="values"/> can be found within this <paramref name="text"/>
        /// according to the <paramref name="comparisonType"/>; otherwise,
        /// <c>false</c>.
        /// </returns>
        public static bool ContainsAny(this string text,
            StringComparison comparisonType,
            params string[] values)
        {
            foreach (var value in values)
            {
                if (text.Contains(value, comparisonType))
                    return true;
            }

            return false;
        }
    }
}
