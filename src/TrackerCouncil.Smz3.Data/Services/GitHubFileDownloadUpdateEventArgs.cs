using System;

namespace TrackerCouncil.Smz3.Data.Services;

/// <summary>
/// Event for the progress of a download of files from GitHub
/// </summary>
/// <param name="completed"></param>
/// <param name="total"></param>
public class GitHubFileDownloadUpdateEventArgs(int completed, int total) : EventArgs
{
    /// <summary>
    /// How many files have been finished (either successful or failed)
    /// </summary>
    public int Completed => completed;

    /// <summary>
    /// The total number of files to process
    /// </summary>
    public int Total => total;
}
