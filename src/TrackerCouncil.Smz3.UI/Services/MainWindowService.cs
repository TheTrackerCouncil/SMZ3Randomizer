using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using GitHubReleaseChecker;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Chat.Integration;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
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
    Configs configs) : ControlService
{
    private MainWindowViewModel _model = new();
    private MainWindow _window = null!;
    private RandomizerOptions _options = null!;

    public MainWindowViewModel InitializeModel(MainWindow window)
    {
        _window = window;
        _options = optionsFactory.Create();
        _model.HasInvalidOptions = !_options.GeneralOptions.Validate();
        ITaskService.Run(CheckForUpdates);
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

    public async Task<bool> ValidateTwitchToken()
    {
        if (string.IsNullOrEmpty(_options.GeneralOptions.TwitchOAuthToken))
        {
            return true;
        }

        var isTokenValid = await chatAuthenticationService.ValidateTokenAsync(_options.GeneralOptions.TwitchOAuthToken, default);

        if (!isTokenValid)
        {
            _options.GeneralOptions.TwitchOAuthToken = string.Empty;
            var messageWindow = new MessageWindow(new MessageWindowRequest()
            {
                Message = "Your Twitch login has expired. Please go to Options and log in with Twitch again to re-enable chat integration features.",
                Title = "SMZ3 Cas’ Randomizer",
                Buttons = MessageWindowButtons.OK,
                Icon = MessageWindowIcon.Warning
            });
            messageWindow.ShowDialog();
            return false;
        }

        return true;
    }

    public event EventHandler? SpriteDownloadStart;

    public event EventHandler? SpriteDownloadEnd;

    public async Task DownloadConfigsAsync()
    {
        var configsUpdated = gitHubConfigDownloaderService.InstallDefaultConfigFolder();

        if (string.IsNullOrEmpty(_options.GeneralOptions.Z3RomPath) ||
            !_options.GeneralOptions.DownloadConfigsOnStartup)
        {
            if (configsUpdated)
            {
                configs.LoadConfigs();
            }
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
            configs.LoadConfigs();
        }
    }

    public async Task DownloadSpritesAsync()
    {
        if (string.IsNullOrEmpty(_options.GeneralOptions.Z3RomPath) ||
            !_options.GeneralOptions.DownloadSpritesOnStartup)
        {
            await spriteService.LoadSpritesAsync();
            trackerSpriteService.LoadSprites();
            return;
        }

        var spriteDownloadRequest = new GitHubFileDownloaderRequest
        {
            RepoOwner = "TheTrackerCouncil",
            RepoName = "SMZ3CasSprites",
            DestinationFolder = RandomizerDirectories.SpritePath,
            HashPath = RandomizerDirectories.SpriteHashYamlFilePath,
            InitialJsonPath = RandomizerDirectories.SpriteInitialJsonFilePath,
            ValidPathCheck = p => Sprite.ValidDownloadExtensions.Contains(Path.GetExtension(p).ToLowerInvariant()),
            ConvertGitHubPathToLocalPath = p => p.Replace("Sprites/", ""),
        };

        var toDownload = await gitHubFileSynchronizerService.GetGitHubFileDetailsAsync(spriteDownloadRequest);

        spriteDownloadRequest = new GitHubFileDownloaderRequest
        {
            RepoOwner = "TheTrackerCouncil",
            RepoName = "TrackerSprites",
            DestinationFolder = RandomizerDirectories.TrackerSpritePath,
            HashPath = RandomizerDirectories.TrackerSpriteHashYamlFilePath,
            InitialJsonPath = RandomizerDirectories.TrackerSpriteInitialJsonFilePath,
            ValidPathCheck = p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".gif", StringComparison.OrdinalIgnoreCase),
        };

        toDownload.AddRange(await gitHubFileSynchronizerService.GetGitHubFileDetailsAsync(spriteDownloadRequest));

        if (toDownload is not { Count: > 4 })
        {
            await gitHubFileSynchronizerService.SyncGitHubFilesAsync(toDownload);
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                SpriteDownloadStart?.Invoke(this, EventArgs.Empty);
                await gitHubFileSynchronizerService.SyncGitHubFilesAsync(toDownload);
                SpriteDownloadEnd?.Invoke(this, EventArgs.Empty);
            });
        }

        await spriteService.LoadSpritesAsync();
        trackerSpriteService.LoadSprites();
    }

    private async Task CheckForUpdates()
    {
        if (!_options.GeneralOptions.CheckForUpdatesOnStartup)
        {
            return;
        }

        var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

        try
        {
            var gitHubRelease = await gitHubReleaseCheckerService
                .GetGitHubReleaseToUpdateToAsync("TheTrackerCouncil", "SMZ3Randomizer", version ?? "", false);

            if (!string.IsNullOrWhiteSpace(gitHubRelease?.Url) && gitHubRelease.Url != _options.GeneralOptions.IgnoredUpdateUrl)
            {
                _model.DisplayNewVersionBanner = true;
                _model.NewVersionGitHubUrl = gitHubRelease.Url;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting GitHub release");
        }
    }
}

