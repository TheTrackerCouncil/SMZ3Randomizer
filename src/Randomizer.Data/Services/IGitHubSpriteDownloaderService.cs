using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Randomizer.Data.Services;

/// <summary>
/// Service for downloading sprites from GitHub
/// </summary>
public interface IGitHubSpriteDownloaderService
{
    public Task<IDictionary<string, string>?> GetSpritesToDownloadAsync(string owner, string repo, TimeSpan? timeout = null);
    public Task DownloadSpritesAsync(string owner, string repo, IDictionary<string, string>? spritesToDownload = null, TimeSpan? timeout = null);
}
