using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using Microsoft.Extensions.Logging;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.CrossPlatform.Views;
using Randomizer.Data.Options;
using Randomizer.Multiplayer.Client;
using Tmds.DBus.Protocol;

namespace Randomizer.CrossPlatform.Services;

public class MultiplayerConnectWindowService(MultiplayerClientService multiplayerClientService, OptionsFactory optionsFactory, ILogger<MultiplayerConnectWindowService> logger) : ControlService, IDisposable
{
    private static readonly Regex s_illegalCharacters = new(@"[^A-Z0-9\-]", RegexOptions.IgnoreCase);
    private MultiplayerConnectWindowViewModel _model = new();
    private MultiplayerConnectWindow _window = null!;
    private RandomizerOptions _options = null!;

    public MultiplayerConnectWindowViewModel GetViewModel(MultiplayerConnectWindow window, bool isCreatingGame)
    {
        _options = optionsFactory.Create();

        multiplayerClientService.Connected += MultiplayerClientServiceOnConnected;
        multiplayerClientService.Error += MultiplayerClientServiceOnError;
        multiplayerClientService.GameCreated += MultiplayerClientServiceOnGameCreated;
        multiplayerClientService.GameJoined += MultiplayerClientServiceOnGameJoined;

        _window = window;
        _model.IsCreatingGame = isCreatingGame;
        _model.Url = _options.MultiplayerUrl;
        return _model;
    }

    public void Connect()
    {
        if (s_illegalCharacters.IsMatch(_model.DisplayName))
        {
            DisplayError("Player display names can only contains letters, numbers, hyphens, and underscores.");
            return;
        }

        if (_model.IsConnecting)
        {
            _model.IsConnecting = false;
            _ = multiplayerClientService.Disconnect();
        }
        else
        {
            _model.IsConnecting = true;
            _ = multiplayerClientService.Connect(_model.ServerUrl);
        }
    }

    private void MultiplayerClientServiceOnGameJoined()
    {
        _window.Close(true, multiplayerClientService.DatabaseGameDetails);
    }

    private void MultiplayerClientServiceOnGameCreated()
    {
        _window.Close(true, multiplayerClientService.DatabaseGameDetails);
    }

    private void MultiplayerClientServiceOnError(string error, Exception? exception)
    {
        DisplayError(error);
    }

    private void MultiplayerClientServiceOnConnected()
    {
        if (_model.IsCreatingGame)
        {
            logger.LogInformation("Connected to server successfully. Creating new game.");
            _options.MultiplayerUrl = _model.Url;
            _options.Save();
            _ = multiplayerClientService.CreateGame(_model.DisplayName, _model.PhoneticName,
                _model.MultiplayerGameType, GetVersion(), _model.AsyncGame, _model.SendItemsOnComplete,
                _model.DeathLink);
        }
        else
        {
            logger.LogInformation("Connected to Server successfully. Joining game.");
            _ = multiplayerClientService.JoinGame(_model.GameGuid, _model.DisplayName, _model.PhoneticName, GetVersion());
        }
    }

    private void DisplayError(string error)
    {
        var window = new MessageWindow(new MessageWindowRequest()
        {
            Message = error,
            Title = "SMZ3 Cas' Randomizer",
            Icon = MessageWindowIcon.Error,
            Buttons = MessageWindowButtons.OK
        });
        window.ShowDialog(_window);
    }

    private string GetVersion()
    {
        var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? "";
        if (version.Contains('+'))
        {
            version = version[..version.IndexOf('+')];
        }

        return version;
    }

    public void Dispose()
    {
        multiplayerClientService.Connected -= MultiplayerClientServiceOnConnected;
        multiplayerClientService.Error -= MultiplayerClientServiceOnError;
        multiplayerClientService.GameCreated -= MultiplayerClientServiceOnGameCreated;
        multiplayerClientService.GameJoined -= MultiplayerClientServiceOnGameJoined;
        GC.SuppressFinalize(this);
    }
}
