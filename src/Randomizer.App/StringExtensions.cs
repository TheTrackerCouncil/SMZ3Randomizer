using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.App
{
    public static class StringExtensions
    {
        public static string Or(this string value, string fallbackValue)
            => string.IsNullOrEmpty(value) ? fallbackValue : value;

        public static string? SubstringBeforeCharacter(this string value, char character)
            => value.Contains(character, StringComparison.Ordinal)
                ? value[..value.IndexOf(character, StringComparison.Ordinal)]
                : null;

        public static string? SubstringAfterCharacter(this string value, char character)
            => value.Contains(character, StringComparison.Ordinal)
                ? value[(value.IndexOf(character, StringComparison.Ordinal) + 1)..]
                : null;
    }
}
