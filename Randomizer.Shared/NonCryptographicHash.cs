using System;
using System.Text;

namespace Randomizer.Shared
{
    /// <summary>
    /// Provides non-cryptographic hash functions.
    /// </summary>
    public static class NonCryptographicHash
    {
        /// <summary>
        /// Returns the Fowler-Noll-Vo hash for the specified data.
        /// </summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>
        /// A new 32-bit signed integer representing the FNV-1a hash of
        /// <paramref name="data"/>.
        /// </returns>
        public static int Fnv1a(string input)
        {
            var data = Encoding.UTF8.GetBytes(input);
            return Fnv1a(data);
        }

        /// <summary>
        /// Returns the Fowler-Noll-Vo hash for the specified data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>
        /// A new 32-bit signed integer representing the FNV-1a hash of
        /// <paramref name="data"/>.
        /// </returns>
        public static int Fnv1a(Span<byte> data)
        {
            var hash = FnvParameters.OffsetBasis32;
            foreach (var x in data)
            {
                hash ^= x;
                hash *= FnvParameters.Prime32;
            }

            return unchecked((int)hash);
        }

        internal static class FnvParameters
        {
            public const uint OffsetBasis32 = 0x811c9dc5;
            public const uint Prime32 = 0x01000193;
        }
    }
}
