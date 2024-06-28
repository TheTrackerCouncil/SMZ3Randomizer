using System.Threading;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.SeedGenerator.Contracts;

public interface IRandomizer
{
    SeedData GenerateSeed(Config config, CancellationToken cancellationToken = default);
}
