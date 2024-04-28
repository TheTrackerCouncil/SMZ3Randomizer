using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Options;

namespace Randomizer.Data.Services;

public class GitHubSpriteDownloaderService : IGitHubSpriteDownloaderService
{
    private ILogger<GitHubSpriteDownloaderService> _logger;
    private string _spriteFolder;
    private OptionsFactory _optionsFactory;

    public GitHubSpriteDownloaderService(ILogger<GitHubSpriteDownloaderService> logger, OptionsFactory optionsFactory)
    {
        _logger = logger;
        _optionsFactory = optionsFactory;
        _spriteFolder = Sprite.SpritePath;
        _logger.LogInformation("Sprite path: {Path}", Sprite.SpritePath);
    }

    public async Task<IDictionary<string, string>?> GetSpritesToDownloadAsync(string owner, string repo, TimeSpan? timeout = null)
    {
        var sprites = await GetGitHubSpritesAsync(owner, repo, timeout);
        if (sprites == null)
        {
            return new Dictionary<string, string>();
        }

        var previousHashes = GetPreviousSpriteHashes();
        var toDownload = new ConcurrentDictionary<string, string>();

        Parallel.ForEach(sprites, parallelOptions: new ParallelOptions() { MaxDegreeOfParallelism = 4 },
            spriteData =>
            {
                var localPath = ConvertGitHubPath(spriteData.Key);
                var currentHash = spriteData.Value;
                previousHashes.TryGetValue(localPath, out var prevHash);

                if (currentHash == prevHash && File.Exists(localPath))
                {
                    return;
                }

                toDownload[spriteData.Key] = spriteData.Value;
            });

        return toDownload;
    }

    public async Task DownloadSpritesAsync(string owner, string repo, IDictionary<string, string>? spritesToDownload = null, TimeSpan? timeout = null)
    {
        if (spritesToDownload == null)
        {
            spritesToDownload = await GetSpritesToDownloadAsync(owner, repo, timeout);
        }

        if (!Directory.Exists(_spriteFolder))
        {
            Directory.CreateDirectory(_spriteFolder);
        }

        var previousHashes = GetPreviousSpriteHashes();
        var added = new ConcurrentDictionary<string, string>();

        if (spritesToDownload?.Any() == true)
        {
            await Parallel.ForEachAsync(spritesToDownload, parallelOptions: new ParallelOptions() { MaxDegreeOfParallelism = 4 },
                async (spriteData, _) =>
                {
                    var localPath = ConvertGitHubPath(spriteData.Key);
                    var currentHash = spriteData.Value;
                    var downloadUrl = GetGitHubRawUrl(spriteData.Key, owner, repo);
                    var successful = await DownloadFileAsync(localPath, downloadUrl);

                    if (successful)
                    {
                        added[localPath] = currentHash;
                    }
                });
        }

        if (File.Exists(Path.Combine(_spriteFolder, "sprites.json")))
        {
            File.Delete(Path.Combine(_spriteFolder, "sprites.json"));
        }

        foreach (var addedSprite in added)
        {
            previousHashes[addedSprite.Key] = addedSprite.Value;
        }

        _optionsFactory.Create().GeneralOptions.SpriteHashes = previousHashes;
        _optionsFactory.Create().Save();
    }

    private Dictionary<string, string> GetPreviousSpriteHashes()
    {
        if (File.Exists(Path.Combine(_spriteFolder, "sprites.json")))
        {
            var spriteJson = File.ReadAllText(Path.Combine(_spriteFolder, "sprites.json"));
            var tree = JsonSerializer.Deserialize<GitHubTree>(spriteJson);

            if (tree?.tree?.Any() == true)
            {
                _logger.LogInformation("Loading previous sprite hashes from {Path}", Path.Combine(_spriteFolder, "sprites.json"));
                return tree.tree
                    .Where(IsValidSpriteFile)
                    .ToDictionary(x => ConvertGitHubPath(x.path), x => x.sha);
            }
        }

        return _optionsFactory.Create().GeneralOptions.SpriteHashes;
    }

    private async Task<bool> DownloadFileAsync(string destination, string url, int attempts = 2)
    {
        try
        {
            var destinationFile = new FileInfo(destination);
            if (destinationFile.Directory?.Exists == false)
            {
                destinationFile.Directory.Create();
            }
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

    private async Task<Dictionary<string, string>?> GetGitHubSpritesAsync(string owner, string repo, TimeSpan? timeout = null)
    {
        var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/git/trees/main?recursive=1";

        string response;

        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "GitHubReleaseChecker");
            client.Timeout = timeout ?? TimeSpan.FromSeconds(5);
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

        _logger.LogInformation("Retrieved {Count} file data from GitHub", tree.tree.Count);


        return tree.tree
            .Where(IsValidSpriteFile)
            .ToDictionary(x => x.path, x => x.sha);
    }

    private string ConvertGitHubPath(string path)
    {
        var pathParts = new List<string>() { _spriteFolder };
        pathParts.AddRange(path.Replace("Sprites/", "").Split("/"));
        return Path.Combine(pathParts.ToArray());
    }

    private string GetGitHubRawUrl(string path, string owner, string repo)
    {
        return $"https://raw.githubusercontent.com/{owner}/{repo}/main/{path}";
    }

    private bool IsValidSpriteFile(GitHubFile file)
    {
        return file.path.StartsWith("Sprites/") && file.path.Contains(".");
    }

    private class GitHubTree
    {
        public string sha { get; set; } = "";
        public string url { get; set; } = "";
        public List<GitHubFile>? tree { get; set; }
    }

    public class GitHubFile
    {
        public required string path { get; set; }
        public required string mode { get; set; }
        public required string type { get; set; }
        public required string sha { get; set; }
        public required string url { get; set; }
    }
}
