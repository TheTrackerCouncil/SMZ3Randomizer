using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AppImageManager;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using GitHubReleaseChecker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PySpeechService.Client;
using PySpeechService.TextToSpeech;
using TrackerCouncil.Smz3.Chat.Integration;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.UI.ViewModels;
using TrackerCouncil.Smz3.UI.Views;

namespace TrackerCouncil.Smz3.UI.Services;

public class MainWindowService(
    IGitHubReleaseCheckerService gitHubReleaseCheckerService,
    OptionsFactory optionsFactory,
    ILogger<MainWindowService> logger,
    IChatAuthenticationService chatAuthenticationService,
    IGitHubConfigDownloaderService gitHubConfigDownloaderService,
    IGitHubFileSynchronizerService gitHubFileSynchronizerService,
    SpriteService spriteService,
    TrackerSpriteService trackerSpriteService,
    ConfigProvider configs,
    IServiceProvider serviceProvider) : ControlService
{
    private MainWindowViewModel _model = new();
    private RandomizerOptions _options = null!;

    public MainWindowViewModel InitializeModel(MainWindow window)
    {
        _options = optionsFactory.Create();
        _model.OpenSetupWindow = !_options.GeneralOptions.HasOpenedSetupWindow;

        if (!_model.OpenSetupWindow && OperatingSystem.IsLinux() && !_options.GeneralOptions.SkipDesktopFile)
        {
            _model.OpenDesktopFileWindow = !AppImage.DoesDesktopFileExist(App.AppId);
        }

        ITaskService.Run(CheckForUpdates);
        ITaskService.Run(StartPySpeechService);
        return _model;
    }

    public void DisableUpdates()
    {
        _options.GeneralOptions.CheckForUpdatesOnStartup = false;
        _options.Save();
        _model.DisplayNewVersionBanner = false;
    }

    public void IgnoreUpdate()
    {
        _options.GeneralOptions.IgnoredUpdateUrl = _model.NewVersionGitHubUrl;
        _options.Save();
        _model.DisplayNewVersionBanner = false;
    }

    public void HandleUserDesktopResponse(bool addDesktopFile)
    {
        if (addDesktopFile && OperatingSystem.IsLinux())
        {
            App.BuildLinuxDesktopFile();
        }
        else
        {
            _options.GeneralOptions.SkipDesktopFile = true;
            _options.Save();
        }
    }

    public async Task ValidateTwitchToken()
    {
        if (string.IsNullOrEmpty(_options.GeneralOptions.TwitchUserName))
        {
            return;
        }

        var isTokenValid = !string.IsNullOrEmpty(_options.GeneralOptions.TwitchOAuthToken) && await chatAuthenticationService.ValidateTokenAsync(_options.GeneralOptions.TwitchOAuthToken, default);

        if (!isTokenValid)
        {
            _options.GeneralOptions.TwitchOAuthToken = string.Empty;
            Dispatcher.UIThread.Invoke(() =>
            {
                var messageWindow = new MessageWindow(new MessageWindowRequest()
                {
                    Message =
                        "Your Twitch login has expired. Please go to Options and log in with Twitch again to re-enable chat integration features.",
                    Title = "SMZ3 Cas’ Randomizer",
                    Buttons = MessageWindowButtons.OK,
                    Icon = MessageWindowIcon.Warning
                });
                messageWindow.ShowDialog();
            });
        }
    }

    public event EventHandler? SpriteDownloadStart;

    public event EventHandler? SpriteDownloadEnd;

    public async Task StartPySpeechService()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var pySpeechService = serviceProvider.GetService<IPySpeechService>();
        if (pySpeechService != null)
        {
            pySpeechService.AutoReconnect = true;
            await pySpeechService.StartAsync();
            await pySpeechService.SetSpeechSettingsAsync(new SpeechSettings());
        }
    }
    public async Task DownloadConfigsAsync()
    {
        var configsUpdated = gitHubConfigDownloaderService.InstallDefaultConfigFolder();

        if (string.IsNullOrEmpty(_options.GeneralOptions.Z3RomPath) ||
            !_options.GeneralOptions.DownloadConfigsOnStartup)
        {
            configs.LoadConfigProfiles();
            return;
        }

        var configSource = _options.GeneralOptions.ConfigSources.FirstOrDefault();
        if (configSource == null)
        {
            configSource = new ConfigSource() { Owner = "TheTrackerCouncil", Repo = "SMZ3CasConfigs" };
            _options.GeneralOptions.ConfigSources.Add(configSource);
        }

        if (await gitHubConfigDownloaderService.DownloadFromSourceAsync(configSource))
        {
            configsUpdated = true;
        }

        if (configsUpdated)
        {
            _options.Save();
            configs.LoadConfigProfiles();
        }
    }

    public async Task DownloadSpritesAsync()
    {
        if (string.IsNullOrEmpty(_options.GeneralOptions.Z3RomPath) ||
            !_options.GeneralOptions.DownloadSpritesOnStartup)
        {
            await spriteService.LoadSpritesAsync();
            trackerSpriteService.LoadSprites();
            _model.IsLoadingSprites = false;
            return;
        }

        var spriteDownloadRequest = new GitHubFileDownloaderRequest
        {
            RepoOwner = "TheTrackerCouncil",
            RepoName = "SMZ3CasSprites",
            DestinationFolder = Directories.SpritePath,
            HashPath = Directories.SpriteHashYamlFilePath,
            InitialJsonPath = Directories.SpriteInitialJsonFilePath,
            ValidPathCheck = p => Sprite.ValidDownloadExtensions.Contains(Path.GetExtension(p).ToLowerInvariant()),
            ConvertGitHubPathToLocalPath = p => p.Replace("Sprites/", ""),
            DeleteExtraFiles = Directories.DeleteSprites
        };

        var toDownload = await gitHubFileSynchronizerService.GetGitHubFileDetailsAsync(spriteDownloadRequest);

        spriteDownloadRequest = new GitHubFileDownloaderRequest
        {
            RepoOwner = "TheTrackerCouncil",
            RepoName = "TrackerSprites",
            DestinationFolder = Directories.TrackerSpritePath,
            HashPath = Directories.TrackerSpriteHashYamlFilePath,
            InitialJsonPath = Directories.TrackerSpriteInitialJsonFilePath,
            ValidPathCheck = p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".yml", StringComparison.OrdinalIgnoreCase),
            DeleteExtraFiles = Directories.DeleteSprites
        };

        toDownload.AddRange(await gitHubFileSynchronizerService.GetGitHubFileDetailsAsync(spriteDownloadRequest));

        var numToDownload = toDownload.Count(x => x.Action != GitHubFileAction.Nothing);

        if (numToDownload > 4)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                SpriteDownloadStart?.Invoke(this, EventArgs.Empty);
                await gitHubFileSynchronizerService.SyncGitHubFilesAsync(toDownload);
                SpriteDownloadEnd?.Invoke(this, EventArgs.Empty);
            });
        }
        else if (numToDownload > 0)
        {
            await gitHubFileSynchronizerService.SyncGitHubFilesAsync(toDownload);
        }

        await spriteService.LoadSpritesAsync();
        trackerSpriteService.LoadSprites();
        _model.IsLoadingSprites = false;
    }

    public async Task<string?> InstallWindowsUpdate(string url)
    {
        var filename = Path.GetFileName(new Uri(url).AbsolutePath);
        var localPath = Path.Combine(Path.GetTempPath(), filename);

        logger.LogInformation("Downloading {Url} to {LocalPath}", url, localPath);

        var response = await DownloadFileAsyncAttempt(url, localPath);

        if (!response.Item1)
        {
            logger.LogInformation("Download failed: {Error}", response.Item2);
            return response.Item2;
        }

        try
        {
            logger.LogInformation("Launching setup file");

            var psi = new ProcessStartInfo
            {
                FileName = localPath,
                UseShellExecute = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                CreateNoWindow = true
            };

            Process.Start(psi);
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to start setup file");
            return "Failed to start setup file";
        }
    }

    private static async Task<(bool, string?)> DownloadFileAsyncAttempt(string url, string target, int attemptNumber = 0, int totalAttempts = 3)
    {

        using var httpClient = new HttpClient();

        try
        {
            await using var downloadStream = await httpClient.GetStreamAsync(url);
            await using var fileStream = new FileStream(target, FileMode.Create);
            await downloadStream.CopyToAsync(fileStream);
            return (true, null);
        }
        catch (Exception ex)
        {
            if (attemptNumber < totalAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(attemptNumber));
                return await DownloadFileAsyncAttempt(url, target, attemptNumber + 1, totalAttempts);
            }
            else
            {
                return (false, $"Download failed: {ex.Message}");
            }
        }
    }

    private async Task CheckForUpdates()
    {
        if (!_options.GeneralOptions.CheckForUpdatesOnStartup)
        {
            return;
        }

        try
        {
            var gitHubRelease = await gitHubReleaseCheckerService
                .GetGitHubReleaseToUpdateToAsync("TheTrackerCouncil", "SMZ3Randomizer", App.Version, false);

            if (!string.IsNullOrWhiteSpace(gitHubRelease?.Url) && gitHubRelease.Url != _options.GeneralOptions.IgnoredUpdateUrl)
            {
                _model.DisplayNewVersionBanner = true;
                _model.NewVersionGitHubUrl = gitHubRelease.Url;

                if (OperatingSystem.IsLinux())
                {
                    _model.NewVersionDownloadUrl = gitHubRelease.Asset
                        .FirstOrDefault(x => x.Url.ToLower().EndsWith(".appimage"))?.Url;
                    _model.DisplayDownloadLink = !string.IsNullOrEmpty(_model.NewVersionDownloadUrl);
                }
                else if (OperatingSystem.IsWindows())
                {
                    _model.NewVersionDownloadUrl = gitHubRelease.Asset
                        .FirstOrDefault(x => x.Url.ToLower().EndsWith(".exe"))?.Url;
                    _model.DisplayDownloadLink = !string.IsNullOrEmpty(_model.NewVersionDownloadUrl);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting GitHub release");
        }
    }
}

