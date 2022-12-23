using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Randomizer.Data.Options;
using Randomizer.Shared;
using Randomizer.SMZ3.Generation;

namespace Randomizer.SMZ3.Contracts
{
    public interface ISeededRandomizer : IRandomizer
    {
        SeedData GenerateSeed(Config config, string seed, CancellationToken cancellationToken = default);

        SeedData GenerateSeed(List<Config> configs, string seed = "", CancellationToken cancellationToken = default);

        bool ValidateSeedSettings(SeedData seedData);

        public static int ParseSeed(ref string? input)
        {
            int seed;
            if (string.IsNullOrEmpty(input))
            {
                seed = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue);
            }
            else
            {
                input = input.Trim();
                if (!Parse.AsInteger(input, out seed) // Accept plain ints as seeds (i.e. mostly original behavior)
                    && !Parse.AsHex(input, out seed)) // Accept hex seeds (e.g. seed as stored in ROM info)
                {
                    // When all else fails, accept any other input by hashing it
                    seed = NonCryptographicHash.Fnv1a(input);
                }
            }

            input = seed.ToString("D", CultureInfo.InvariantCulture);
            return seed;
        }
    }
}
