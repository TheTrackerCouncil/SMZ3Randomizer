using System;
using System.Globalization;

namespace Randomizer.SMZ3.Generation
{
    /// <summary>
    /// Converts string representations to their appropriate types.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Converts a hexadecimal number to an equivalent 32-bit signed
        /// integer.
        /// </summary>
        /// <param name="input">
        /// A string that contains the number of parse.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains a 32-bit signed integer
        /// equivalent to <paramref name="input"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="input"/> contains a valid
        /// 32-bit hexadecimal number; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool AsHex(string input, out int value)
        {
            try
            {
                value = Convert.ToInt32(input, 16);
                return true;
            }
            catch (Exception ex)
            when (ex is FormatException or OverflowException)
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Parses the string as a plain 32-bit signed integer.
        /// </summary>
        /// <param name="input">
        /// A string that contains the number to parse.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains a 32-bit signed integer
        /// equivalent to <paramref name="input"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="input"/> contains a valid
        /// 32-bit integer; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool AsInteger(string input, out int value)
        {
            return int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
