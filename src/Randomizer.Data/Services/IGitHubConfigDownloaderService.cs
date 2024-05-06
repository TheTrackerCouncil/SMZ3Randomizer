using System.Threading.Tasks;
using Randomizer.Data.Options;

namespace Randomizer.Data.Services;

public interface IGitHubConfigDownloaderService
{
    public Task<bool> DownloadFromSourceAsync(ConfigSource configSource);

    public void InstallDefaultConfigFolder();
}
