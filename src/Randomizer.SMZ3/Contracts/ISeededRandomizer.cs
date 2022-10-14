using System.Collections.Generic;
using System.Threading;
using Randomizer.Data.Options;
using Randomizer.SMZ3.Generation;

namespace Randomizer.SMZ3.Contracts
{
    public interface ISeededRandomizer : IRandomizer
    {
        SeedData GenerateSeed(Config config, string seed, CancellationToken cancellationToken = default);

        SeedData GenerateSeed(List<Config> configs, string seed = "", CancellationToken cancellationToken = default);

        bool ValidateSeedSettings(SeedData seedData);
    }
}
