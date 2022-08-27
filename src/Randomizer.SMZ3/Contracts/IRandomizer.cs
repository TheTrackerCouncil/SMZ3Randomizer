using System;
using System.Threading;

using Randomizer.SMZ3.Generation;

namespace Randomizer.SMZ3.Contracts
{
    public interface IRandomizer
    {
        SeedData GenerateSeed(Config config, CancellationToken cancellationToken = default);
    }
}
