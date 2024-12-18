using System.Threading.Tasks;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.Data.Services;

public interface IGitHubConfigDownloaderService
{
    public Task<bool> DownloadFromSourceAsync(ConfigSource configSource);

    public bool InstallDefaultConfigFolder();
}
