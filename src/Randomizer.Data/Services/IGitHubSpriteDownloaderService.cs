using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Randomizer.Data.Services;

/// <summary>
/// Service for downloading sprites from GitHub
/// </summary>
public interface IGitHubSpriteDownloaderService
{
    /// <summary>
    /// Compares the previously downloaded sprites with what was found on GitHub and returns the sprites to download
    /// </summary>
    /// <param name="owner">The GitHub repository owner</param>
    /// <param name="repo">The GitHub repository to download</param>
    /// <param name="timeout">How long to timeout from the api call</param>
    /// <returns>A dictionary of paths on GitHub and their git hashes</returns>
    public Task<IDictionary<string, string>?> GetSpritesToDownloadAsync(string owner, string repo, TimeSpan? timeout = null);

    /// <summary>
    /// Downloads sprites from GitHub to the folder
    /// </summary>
    /// <param name="owner">The GitHub repository owner</param>
    /// <param name="repo">The GitHub repository to download</param>
    /// <param name="spritesToDownload">A dictionary for sprites that need to be downloaded from calling GetSpritesToDownloadAsync</param>
    /// <param name="timeout">How long to timeout from the api call</param>
    /// <returns></returns>
    public Task DownloadSpritesAsync(string owner, string repo, IDictionary<string, string>? spritesToDownload = null, TimeSpan? timeout = null);
}
