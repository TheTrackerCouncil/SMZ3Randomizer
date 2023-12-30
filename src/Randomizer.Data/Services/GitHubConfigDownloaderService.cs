using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubReleaseChecker;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Options;

namespace Randomizer.Data.Services;

/// <summary>
/// Service for downloading config files from GitHub
/// </summary>
public class GitHubConfigDownloaderService : IGitHubConfigDownloaderService
{
    private readonly ILogger<GitHubConfigDownloaderService> _logger;
    private readonly IGitHubReleaseService _gitHubReleaseService;
    private readonly string _tempDirectory;
    private readonly string _targetDirectory;

    public GitHubConfigDownloaderService(ILogger<GitHubConfigDownloaderService> logger, IGitHubReleaseService gitHubReleaseService)
    {
        _logger = logger;
        _gitHubReleaseService = gitHubReleaseService;
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SMZ3CasConfigs");
        _targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SMZ3CasRandomizer", "Configs");
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
        Directory.CreateDirectory(_tempDirectory);
        if (!Directory.Exists(_targetDirectory))
        {
            Directory.CreateDirectory(_targetDirectory);
        }
    }

    public async Task<bool> DownloadFromSourceAsync (ConfigSource configSource)
    {
        var releases = await _gitHubReleaseService.GetReleasesAsync(configSource.Owner, configSource.Repo);
        var latestRelease = releases?.FirstOrDefault();

        if (latestRelease == null)
        {
            return false;
        }

        if (latestRelease.Asset.Any() == false)
        {
            _logger.LogError("Latest version {Version} does not have any downloads", latestRelease.Tag);
            return false;
        }
        if (latestRelease.Tag == configSource.PriorVersion)
        {
            _logger.LogInformation("Config version {Version} is already up-to-date", configSource.PriorVersion);
            return false;
        }

        var asset = latestRelease.Asset.First();
        _logger.LogInformation("Downloading config version {Version} from {Url} ", latestRelease.Tag, asset.Url);

        var tempFile = await DownloadFileAsync(latestRelease.Asset.First());

        if (string.IsNullOrEmpty(tempFile) || !File.Exists(tempFile))
        {
            _logger.LogError("Could not download config release from {Url}", latestRelease.Asset.First().Url);
            return false;
        }

        var tempFolder = Path.Combine(_tempDirectory, Path.GetFileNameWithoutExtension(tempFile));

        try
        {
            _logger.LogInformation("Installing config {Version} to {Path}", latestRelease.Tag, _targetDirectory);
            System.IO.Compression.ZipFile.ExtractToDirectory(tempFile, tempFolder);
            CopyFolder(Path.Combine(_tempDirectory, Path.GetFileNameWithoutExtension(tempFile)), _targetDirectory);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error downloading installing config version");
            return false;
        }

        // Clean up temp files
        try
        {
            File.Delete(tempFile);
            Directory.Delete(tempFolder, true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to delete temp file/folder");
        }

        configSource.PriorVersion = latestRelease.Tag;

        return true;
    }

    private async Task<string?> DownloadFileAsync(GitHubReleaseAsset asset)
    {
        var destinationFile = Path.Combine(_tempDirectory, asset.Name);
        if (File.Exists(destinationFile))
        {
            File.Delete(destinationFile);
        }
        using var client = new HttpClient();
        var response = await client.GetAsync(new Uri(asset.Url));
        await using var fs = new FileStream(destinationFile, FileMode.Create);
        await response.Content.CopyToAsync(fs);
        _logger.LogInformation("Downloaded {Url} to {Path}", asset.Name, destinationFile);

        return destinationFile;
    }

    private void CopyFolder(string source, string destination)
    {
        foreach (var file in Directory.EnumerateFiles(source))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
        }

        foreach (var folder in Directory.EnumerateDirectories(source))
        {
            var directory = new DirectoryInfo(folder).Name;
            var newDestination = Path.Combine(destination, directory);
            if (!Directory.Exists(newDestination))
            {
                Directory.CreateDirectory(newDestination);
            }
            CopyFolder(folder, newDestination);
        }
    }
}
