using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Options;
using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Services;

public delegate bool PathCheck(string path);

public delegate string PathConversion(string path);

/// <summary>
/// Request for synchronizing a folder with GitHub
/// </summary>
public class GitHubFileDownloaderRequest
{
    /// <summary>
    /// The user/team name that owns the repository
    /// </summary>
    public required string RepoOwner { get; set; }

    /// <summary>
    /// The name of the repository
    /// </summary>
    public required string RepoName { get; set; }

    /// <summary>
    /// The destination folder to synchronize GitHub files to
    /// </summary>
    public required string DestinationFolder { get; set; }

    /// <summary>
    /// File to save the GitHub hashes to to prevent redownloading a file if it hasn't changed
    /// </summary>
    public required string HashPath { get; set; }

    /// <summary>
    /// Path to a JSON file which contains the GitHub path and hash information. Used to prevent downloading files
    /// when first installing the application.
    /// </summary>
    public string? InitialJsonPath { get; set; }

    /// <summary>
    /// Delegate to check if a given file should be synchronized
    /// </summary>
    public PathCheck? ValidPathCheck { get; set; }

    /// <summary>
    /// Delegate to convert the relative path on GitHub to the relative path to save the file locally
    /// </summary>
    public PathConversion? ConvertGitHubPathToLocalPath { get; set; }

    /// <summary>
    /// Timeout value for each individual request
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// Class that houses data of a file to synchronize
/// </summary>
public class GitHubFileDetails
{
    /// <summary>
    /// The relative path of the file
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// The expected path of the file on the local computer
    /// </summary>
    public required string LocalPath { get; set; }

    /// <summary>
    /// The url to use for downloading the file
    /// </summary>
    public required string DownloadUrl { get; set; }

    /// <summary>
    /// The hash of the file on GitHub to track if the file was previously downloaded
    /// </summary>
    public required string RemoteHash { get; set; }

    /// <summary>
    /// If the file is found on the local computer
    /// </summary>
    public required bool FileExistsLocally { get; set; }

    /// <summary>
    /// If there is a difference in the file between the local computer and Github
    /// </summary>
    public required bool FileMatchesLocally { get; set; }

    /// <summary>
    /// The path to the file to save the GitHub hash to
    /// </summary>
    public required string HashPath { get; set; }
}

/// <summary>
/// Service to synchronize files between a local folder and a GitHub repo
/// </summary>
public class GitHubFileSynchronizerService : IGitHubFileSynchronizerService
{
    private ILogger<GitHubFileSynchronizerService> _logger;
    private CancellationTokenSource _cts = new();

    public GitHubFileSynchronizerService(ILogger<GitHubFileSynchronizerService> logger)
    {
        _logger = logger;
    }

    public void CancelDownload() => _cts.Cancel();

    public event EventHandler<GitHubFileDownloadUpdateEventArgs>? SynchronizeUpdate;

    public async Task<List<GitHubFileDetails>> GetGitHubFileDetailsAsync(GitHubFileDownloaderRequest request)
    {
        var files = await GetGitHubFilesAsync(request);
        if (files == null)
        {
            return [];
        }

        var previousHashes = GetPreviousHashes(request);

        var fileList = new ConcurrentBag<GitHubFileDetails>();

        Parallel.ForEach(files, parallelOptions: new ParallelOptions { MaxDegreeOfParallelism = 4 },
            fileData =>
            {
                var localPath = ConvertToLocalPath(request, fileData.Key);
                var currentHash = fileData.Value;
                previousHashes.TryGetValue(localPath, out var prevHash);

                fileList.Add(new GitHubFileDetails()
                {
                    Path = fileData.Key,
                    LocalPath = localPath,
                    DownloadUrl = $"https://raw.githubusercontent.com/{request.RepoOwner}/{request.RepoName}/main/{fileData.Key}",
                    RemoteHash = fileData.Value,
                    FileExistsLocally = File.Exists(localPath),
                    FileMatchesLocally = File.Exists(localPath) && prevHash == currentHash,
                    HashPath = request.HashPath
                });
            });

        if (Directory.Exists(request.DestinationFolder))
        {
            var foundLocalFiles = fileList.Select(x => x.LocalPath).ToHashSet();

            foreach (var file in Directory.EnumerateFiles(request.DestinationFolder, "*", SearchOption.AllDirectories).Where(x => !foundLocalFiles.Contains(x) && IsValidPath(request, x)))
            {
                fileList.Add(new GitHubFileDetails()
                {
                    Path = file,
                    LocalPath = file,
                    DownloadUrl = "",
                    RemoteHash = "",
                    FileExistsLocally = true,
                    FileMatchesLocally = false,
                    HashPath = request.HashPath
                });
            }
        }


        return fileList.Where(x => !x.FileMatchesLocally).ToList();
    }

    public async Task SyncGitHubFilesAsync(GitHubFileDownloaderRequest request)
    {
        var fileDetails = await GetGitHubFileDetailsAsync(request);
        await SyncGitHubFilesAsync(fileDetails);
    }


    public async Task SyncGitHubFilesAsync(List<GitHubFileDetails> fileDetails)
    {
        if (fileDetails.Count == 0)
        {
            return;
        }

        var filesToProcess = fileDetails.Where(x => !x.FileMatchesLocally).ToList();
        var total = filesToProcess.Count;
        var completed = 0;

        if (total > 0)
        {
            await Parallel.ForEachAsync(filesToProcess, parallelOptions: new ParallelOptions() { MaxDegreeOfParallelism = 4, CancellationToken = _cts.Token},
                async (fileData, _) =>
                {
                    if (!string.IsNullOrEmpty(fileData.DownloadUrl))
                    {
                        await DownloadFileAsync(fileData.LocalPath, fileData.DownloadUrl);
                    }
                    else
                    {
                        File.Delete(fileData.LocalPath);
                        _logger.LogInformation("Deleted {Path}", fileData.LocalPath);
                    }

                    completed++;
                    SynchronizeUpdate?.Invoke(this, new GitHubFileDownloadUpdateEventArgs(completed, total));
                });
        }

        foreach (var hashPath in fileDetails.Select(x => x.HashPath).Distinct())
        {
            SaveFileHashYaml(hashPath,
                fileDetails.Where(x => x.HashPath == hashPath && !string.IsNullOrEmpty(x.DownloadUrl))
                    .ToDictionary(x => x.LocalPath, x => x.RemoteHash));
        }
    }

    private Dictionary<string, string> GetPreviousHashes(GitHubFileDownloaderRequest request)
    {
        var initialJsonPath = request.InitialJsonPath;
        var hashYamlPath = request.HashPath;
        ;
        if (!string.IsNullOrEmpty(initialJsonPath) && File.Exists(initialJsonPath))
        {
            var initialJson = File.ReadAllText(initialJsonPath);
            var tree = JsonSerializer.Deserialize<GitHubTree>(initialJson);

            if (tree?.tree == null || tree.tree.Count == 0)
            {
                File.Delete(initialJsonPath);
                return [];
            }

            _logger.LogInformation("Loading previous file hashes from {Path}", initialJsonPath);

            var toReturn = tree.tree
                .Where(p => IsValidPath(request, p.path))
                .ToDictionary(x => ConvertToLocalPath(request, x.path), x => x.sha);

            SaveFileHashYaml(request.HashPath, toReturn);
            File.Delete(initialJsonPath);

            return toReturn;
        }
        else if (File.Exists(hashYamlPath))
        {
            var yamlText = File.ReadAllText(hashYamlPath);
            var serializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            try
            {
                return serializer.Deserialize<Dictionary<string, string>>(yamlText);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deserializing file hash yaml file {Path}", hashYamlPath);
            }
        }

        return [];
    }

    private void SaveFileHashYaml(string hashPath, Dictionary<string, string> hashes)
    {
        var serializer = new Serializer();
        var yamlText = serializer.Serialize(hashes);
        File.WriteAllText(hashPath, yamlText);
    }

    private async Task<bool> DownloadFileAsync(string destination, string url, int attempts = 2)
    {
        try
        {
            var destinationFile = new FileInfo(destination);
            Directory.CreateDirectory(destinationFile.DirectoryName ?? "");
            using var client = new HttpClient();
            var response = await client.GetAsync(new Uri(url));
            await using var fs = new FileStream(destination, FileMode.Create);
            await response.Content.CopyToAsync(fs);
            _logger.LogInformation("Downloaded {Url} to {Path}", url, destination);
            return true;
        }
        catch (Exception e)
        {
            if (attempts == 0)
            {
                _logger.LogError(e, "Unable to download {Url} to {Path}", url, destination);
                return false;
            }
            else
            {
                return await DownloadFileAsync(destination, url, attempts-1);
            }
        }
    }

    private async Task<Dictionary<string, string>?> GetGitHubFilesAsync(GitHubFileDownloaderRequest request)
    {
        var apiUrl = $"https://api.github.com/repos/{request.RepoOwner}/{request.RepoName}/git/trees/main?recursive=1";

        string response;

        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "GitHubReleaseChecker");
            client.Timeout = request.Timeout;
            response = await client.GetStringAsync(apiUrl);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to call GitHub Release API");
            return null;
        }

        var tree = JsonSerializer.Deserialize<GitHubTree>(response);

        if (tree?.tree?.Any() != true)
        {
            _logger.LogWarning("Unable to parse GitHub JSON");
            return null;
        }

        _logger.LogInformation("Retrieved data for {Count} files from GitHub", tree.tree.Count);

        return tree.tree
            .Where(x => IsValidPath(request, x.path))
            .ToDictionary(x => x.path, x => x.sha);
    }

    private bool IsValidPath(GitHubFileDownloaderRequest request, string path)
    {
        return request.ValidPathCheck == null || request.ValidPathCheck(path);
    }

    private string ConvertToLocalPath(GitHubFileDownloaderRequest request, string path)
    {
        path = Path.Combine(request.DestinationFolder,
            request.ConvertGitHubPathToLocalPath == null ? path : request.ConvertGitHubPathToLocalPath(path));
        return path.Replace("/", Path.DirectorySeparatorChar.ToString());
    }

    private class GitHubTree
    {
        public string sha { get; set; } = "";
        public string url { get; set; } = "";
        public List<GitHubFile>? tree { get; set; }
    }

    private class GitHubFile
    {
        public required string path { get; set; }
        public required string mode { get; set; }
        public required string type { get; set; }
        public required string sha { get; set; }
        public required string url { get; set; }
    }
}
