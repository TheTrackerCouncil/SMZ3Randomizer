using System.Threading;
using System.Threading.Tasks;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.SeedGenerator.Contracts;

public interface IPlandoConfigLoader
{
    Task<PlandoConfig> LoadAsync(string path, CancellationToken cancellationToken = default);
}
