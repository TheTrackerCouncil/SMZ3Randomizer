using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrackerCouncil.Smz3.Data.Services;

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
    /// <param name="ignoreNoPreviousHashes">If downloads should be triggered, even if no previous hashes exist</param>
    /// <returns>A dictionary of paths on GitHub and their git hashes</returns>
    public Task<IDictionary<string, string>?> GetSpritesToDownloadAsync(string owner, string repo, TimeSpan? timeout = null, bool ignoreNoPreviousHashes = false);

    /// <summary>
    /// Downloads sprites from GitHub to the folder
    /// </summary>
    /// <param name="owner">The GitHub repository owner</param>
    /// <param name="repo">The GitHub repository to download</param>
    /// <param name="spritesToDownload">A dictionary for sprites that need to be downloaded from calling GetSpritesToDownloadAsync</param>
    /// <param name="timeout">How long to timeout from the api call</param>
    /// <returns></returns>
    public Task DownloadSpritesAsync(string owner, string repo, IDictionary<string, string>? spritesToDownload = null, TimeSpan? timeout = null);

    /// <summary>
    /// Cancels the current sprite download
    /// </summary>
    public void CancelDownload();

    /// <summary>
    /// Event that fires off after each completed sprite download
    /// </summary>
    public event EventHandler<SpriteDownloadUpdateEventArgs> SpriteDownloadUpdate;
}
