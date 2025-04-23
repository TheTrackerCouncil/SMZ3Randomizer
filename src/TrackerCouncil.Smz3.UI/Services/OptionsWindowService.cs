using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaControls.ControlServices;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Chat.Integration;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Tracking.Services;
using ConfigProvider = TrackerCouncil.Smz3.Data.Configuration.ConfigProvider;

namespace TrackerCouncil.Smz3.UI.Services;

public class TwitchErrorEventHandler(string error) : EventArgs
{
    public string Error => error;
}

public class OptionsWindowService(
    ConfigProvider configProvider,
    IMicrophoneService microphoneService,
    OptionsFactory optionsFactory,
    IChatAuthenticationService chatAuthenticationService,
    ILogger<OptionsWindowService> logger,
    IGitHubConfigDownloaderService gitHubConfigDownloaderService,
    IGitHubFileSynchronizerService gitHubFileSynchronizerService,
    TrackerSpriteService trackerSpriteService,
    ICommunicator communicator,
    ConfigProvider configs) : ControlService
{
    private readonly Dictionary<string, string> _availableInputDevices = new() { { "Default", "Default" } };
    private OptionsWindowViewModel _model = new();

    public event EventHandler? SpriteDownloadStarted;

    public event EventHandler? SpriteDownloadEnded;

    public event EventHandler<TwitchErrorEventHandler>? TwitchError;

    public OptionsWindowViewModel GetViewModel()
    {
        foreach (var device in microphoneService.GetDeviceDetails())
        {
            _availableInputDevices[device.Key] = device.Value;
        }

        _model = new OptionsWindowViewModel(optionsFactory.Create().GeneralOptions,
            trackerSpriteService.GetPackOptions(), _availableInputDevices,
            configProvider.GetAvailableProfiles().ToList());

        _model.RandomizerOptions.UpdateConfigButtonPressed += (_, _) =>
        {
            _ = UpdateConfigsAsync();
        };

        _model.RandomizerOptions.UpdateSpritesButtonPressed += (_, _) =>
        {
            _ = UpdateSpritesAsync();
        };

        _model.TrackerOptions.TestTextToSpeechPressed += (_, _) =>
        {
            communicator.UpdateVolume(_model.TrackerOptions.TextToSpeechVolume);
            communicator.Say(new SpeechRequest("This is a test message", null, true));
        };

        _model.TwitchIntegration.TwitchLoginPressed += (_, _) =>
        {
            TwitchLogin();
        };

        _model.TwitchIntegration.TwitchLogoutPressed += (_, _) =>
        {
            TwitchLogout();
        };

        _model.TrackerProfiles.OpenProfileFolderPressed += (_, _) =>
        {
            OpenProfileFolder();
        };

        _model.TrackerProfiles.RefreshProfilesPressed += (_, _) =>
        {
            _model.TrackerProfiles.AvailableProfiles = configProvider.GetAvailableProfiles()
                .Where(x => !string.IsNullOrEmpty(x) && !"Default".Equals(x)).ToList();
        };

        _ = ValidateTwitchOAuthToken();

        return _model;
    }

    public void Close()
    {
        communicator.Dispose();
    }

    public void SaveViewModel()
    {
        var options = optionsFactory.Create();
        _model.UpdateOptions(options.GeneralOptions);

        if (!Directory.Exists(options.GeneralOptions.RomOutputPath))
        {
            Directory.CreateDirectory(options.GeneralOptions.RomOutputPath);
        }

        options.Save();
    }

    private async Task ValidateTwitchOAuthToken()
    {
        if (string.IsNullOrEmpty(_model.TwitchIntegration.TwitchOAuthToken))
        {
            _model.TwitchIntegration.TwitchStatusText = "";
            _model.TwitchIntegration.IsLoggedIn = false;
            return;
        }

        var isValid = await chatAuthenticationService.ValidateTokenAsync(_model.TwitchIntegration.TwitchOAuthToken, default);
        if (!isValid)
        {
            _model.TwitchIntegration.TwitchStatusText = "Login expired.";
            _model.TwitchIntegration.TwitchOAuthToken = "";
            _model.TwitchIntegration.IsLoggedIn = false;
        }
        else
        {
            _model.TwitchIntegration.TwitchStatusText = "Logged in.";
            _model.TwitchIntegration.IsLoggedIn = true;
        }
    }

    private async void TwitchLogin()
    {
        try
        {
            await Task.Run(async () =>
            {
                try
                {
                    var token = await chatAuthenticationService.GetTokenInteractivelyAsync(default);

                    if(token == null)
                    {
                        logger.LogError("Token returned by chat authentication service null");
                        TwitchError?.Invoke(this, new TwitchErrorEventHandler("An unexpected error occurred while trying to log you in with Twitch. " +
                                                                              "Please try again or report this issue with the log file."));
                        return;
                    }

                    var userData = await chatAuthenticationService.GetAuthenticatedUserDataAsync(token, default);

                    if (userData == null)
                    {
                        logger.LogError("User Data returned by chat authentication service null");
                        TwitchError?.Invoke(this, new TwitchErrorEventHandler("An unexpected error occurred while trying to log you in with Twitch. " +
                                                                              "Please try again or report this issue with the log file."));
                        return;
                    }

                    _model.TwitchIntegration.TwitchUserName = userData.Name;
                    _model.TwitchIntegration.TwitchOAuthToken = token;
                    _model.TwitchIntegration.TwitchChannel = userData.Name;
                    _model.TwitchIntegration.TwitchId = userData.Id;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An unknown error occurred while logging in with Twitch");
                    TwitchError?.Invoke(this, new TwitchErrorEventHandler("An unexpected error occurred while trying to log you in with Twitch. " +
                                                                          "Please try again or report this issue with the log file."));
                }
            });
        }
        finally
        {
            _model.TwitchIntegration.IsLoggedIn = true;
        }

        await ValidateTwitchOAuthToken();
    }

    private async void TwitchLogout()
    {
        if (string.IsNullOrEmpty(_model.TwitchIntegration.TwitchOAuthToken))
            return;

        var revoked = await chatAuthenticationService.RevokeTokenAsync(_model.TwitchIntegration.TwitchOAuthToken, default);

        if (revoked)
        {
            _model.TwitchIntegration.TwitchUserName = "";
            _model.TwitchIntegration.TwitchOAuthToken = "";
            _model.TwitchIntegration.TwitchChannel = "";
            _model.TwitchIntegration.TwitchId = "";
        }

        _model.TwitchIntegration.TwitchStatusText = revoked ? "Logged out." : "Something went wrong.";
        _model.TwitchIntegration.IsLoggedIn = false;
    }

    private async Task UpdateConfigsAsync()
    {
        var options = optionsFactory.Create();
        var configSource = options.GeneralOptions.ConfigSources.FirstOrDefault();
        if (configSource == null)
        {
            configSource = new ConfigSource() { Owner = "TheTrackerCouncil", Repo = "SMZ3CasConfigs" };
            options.GeneralOptions.ConfigSources.Add(configSource);
        }

        if (await gitHubConfigDownloaderService.DownloadFromSourceAsync(configSource))
        {
            configs.LoadConfigProfiles();
            configs.CopySchemaFolder();
        }
    }

    private async Task UpdateSpritesAsync()
    {
        var spriteDownloadRequest = new GitHubFileDownloaderRequest
        {
            RepoOwner = "TheTrackerCouncil",
            RepoName = "SMZ3CasSprites",
            DestinationFolder = Directories.SpritePath,
            HashPath = Directories.SpriteHashYamlFilePath,
            InitialJsonPath = Directories.SpriteInitialJsonFilePath,
            ValidPathCheck = p => p.StartsWith("Sprites/") && p.Contains('.'),
            ConvertGitHubPathToLocalPath = p => p.Replace("Sprites/", ""),
            DeleteExtraFiles = Directories.DeleteSprites
        };

        var sprites = await gitHubFileSynchronizerService.GetGitHubFileDetailsAsync(spriteDownloadRequest);

        spriteDownloadRequest = new GitHubFileDownloaderRequest
        {
            RepoOwner = "TheTrackerCouncil",
            RepoName = "TrackerSprites",
            DestinationFolder = Directories.TrackerSpritePath,
            HashPath = Directories.TrackerSpritePath,
            InitialJsonPath = Directories.TrackerSpritePath,
            ValidPathCheck = p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".gif", StringComparison.OrdinalIgnoreCase),
            DeleteExtraFiles = Directories.DeleteTrackerSprites
        };

        sprites.AddRange(await gitHubFileSynchronizerService.GetGitHubFileDetailsAsync(spriteDownloadRequest));

        if (sprites.Any(x => x.Action != GitHubFileAction.Nothing))
        {
            SpriteDownloadStarted?.Invoke(this, EventArgs.Empty);
            await gitHubFileSynchronizerService.SyncGitHubFilesAsync(sprites);
            SpriteDownloadEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OpenProfileFolder()
    {
        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Directories.UserConfigPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Could not open profile folder {Directories.UserConfigPath}");
        }
    }
}
