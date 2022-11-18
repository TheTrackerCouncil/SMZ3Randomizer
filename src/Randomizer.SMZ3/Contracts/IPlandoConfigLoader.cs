using System.Threading;
using System.Threading.Tasks;
using Randomizer.Data.Options;
using Randomizer.SMZ3.Generation;

namespace Randomizer.SMZ3.Contracts
{
    public interface IPlandoConfigLoader
    {
        Task<PlandoConfig> LoadAsync(string path, CancellationToken cancellationToken = default);
    }
}
