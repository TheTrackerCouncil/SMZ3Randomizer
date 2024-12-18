using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrackerCouncil.Smz3.Data.Services;

/// <summary>
/// Service for synchronizing a GitHub repository and a local folder
/// </summary>
public interface IGitHubFileSynchronizerService
{
    /// <summary>
    /// Retrieves the list of files to synchronize between GitHub and the local folder
    /// </summary>
    /// <param name="request">Request with details about the GitHub repository to sync</param>
    /// <returns>A list of all files located either on GitHub or in the local folder</returns>
    public Task<List<GitHubFileDetails>> GetGitHubFileDetailsAsync(GitHubFileDownloaderRequest request);

    /// <summary>
    /// Synchronizes the provided list of GitHubFileDetails
    /// </summary>
    /// <param name="request">Request with details about the GitHub repository to sync</param>
    public Task SyncGitHubFilesAsync(GitHubFileDownloaderRequest request);

    /// <summary>
    /// Synchronizes the provided list of GitHubFileDetails
    /// </summary>
    /// <param name="fileDetails">List of files to download</param>
    public Task SyncGitHubFilesAsync(List<GitHubFileDetails> fileDetails);

    /// <summary>
    /// Cancels the current sprite download
    /// </summary>
    public void CancelDownload();

    /// <summary>
    /// Event that fires off after each completed sprite download
    /// </summary>
    public event EventHandler<GitHubFileDownloadUpdateEventArgs> SynchronizeUpdate;
}
