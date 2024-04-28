using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using GitHubReleaseChecker;
using Microsoft.Extensions.Logging;
using NAudio.MediaFoundation;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.CrossPlatform.Views;
using Randomizer.Data.Configuration;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.SMZ3.ChatIntegration;
using ReactiveUI;

namespace Randomizer.CrossPlatform.Services;

public class MainWindowService(
    IGitHubReleaseCheckerService gitHubReleaseCheckerService,
    OptionsFactory optionsFactory,
    ILogger<MainWindowService> logger,
    IChatAuthenticationService chatAuthenticationService,
    IGitHubConfigDownloaderService gitHubConfigDownloaderService,
    IGitHubSpriteDownloaderService gitHubSpriteDownloaderService) : ControlService
{
    private MainWindowViewModel _model = new();
    private MainWindow _window = null!;
    private RandomizerOptions _options = null!;

    public MainWindowViewModel InitializeModel(MainWindow window)
    {
        _window = window;
        _options = optionsFactory.Create();
        _model.HasInvalidOptions = !_options.GeneralOptions.Validate();
        _ = CheckForUpdates();
        return _model;
    }

    public void DisableUpdates()
    {
        _options.GeneralOptions.CheckForUpdatesOnStartup = true;
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
        gitHubConfigDownloaderService.InstallDefaultConfigFolder();

        if (string.IsNullOrEmpty(_options.GeneralOptions.Z3RomPath) ||
            !_options.GeneralOptions.DownloadConfigsOnStartup)
        {
            return;
        }

        var configSource = _options.GeneralOptions.ConfigSources.FirstOrDefault();
        if (configSource == null)
        {
            configSource = new ConfigSource() { Owner = "TheTrackerCouncil", Repo = "SMZ3CasConfigs" };
            _options.GeneralOptions.ConfigSources.Add(configSource);
        }
        await gitHubConfigDownloaderService.DownloadFromSourceAsync(configSource);
        _options.Save();
    }

    public async Task DownloadSpritesAsync()
    {
        if (string.IsNullOrEmpty(_options.GeneralOptions.Z3RomPath) ||
            !_options.GeneralOptions.DownloadSpritesOnStartup)
        {
            return;
        }

        var toDownload = await gitHubSpriteDownloaderService.GetSpritesToDownloadAsync("TheTrackerCouncil", "SMZ3CasSprites");

        if (toDownload is not { Count: > 4 })
        {
            await gitHubSpriteDownloaderService.DownloadSpritesAsync("TheTrackerCouncil", "SMZ3CasSprites", toDownload);
            return;
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                SpriteDownloadStart?.Invoke(this, EventArgs.Empty);
                await gitHubSpriteDownloaderService.DownloadSpritesAsync("TheTrackerCouncil", "SMZ3CasSprites", toDownload);
                SpriteDownloadEnd?.Invoke(this, EventArgs.Empty);
            });
        }
    }

    private async Task CheckForUpdates()
    {
        if (!_options.GeneralOptions.CheckForUpdatesOnStartup) return;

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

