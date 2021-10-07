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
    }
}
