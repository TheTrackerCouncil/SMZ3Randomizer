using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Multiplayer.Client;
using TrackerCouncil.Smz3.UI.ViewModels;
using TrackerCouncil.Smz3.UI.Views;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace TrackerCouncil.Smz3.UI.Services;

public class MultiplayerConnectWindowService(
    MultiplayerClientService multiplayerClientService,
    OptionsFactory optionsFactory,
    ILogger<MultiplayerConnectWindowService> logger) : ControlService, IDisposable
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
                _model.MultiplayerGameType, App.Version, _model.AsyncGame, _model.SendItemsOnComplete,
                _model.DeathLink);
        }
        else
        {
            logger.LogInformation("Connected to Server successfully. Joining game.");
            _ = multiplayerClientService.JoinGame(_model.GameGuid, _model.DisplayName, _model.PhoneticName, App.Version);
        }
    }

    private void DisplayError(string error)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Invoke(() => DisplayError(error));
            return;
        }

        var window = new MessageWindow(new MessageWindowRequest()
        {
            Message = error,
            Title = "SMZ3 Cas' Randomizer",
            Icon = MessageWindowIcon.Error,
            Buttons = MessageWindowButtons.OK
        });
        window.ShowDialog(_window);
        if (_model.IsConnecting)
        {
            _model.IsConnecting = false;
        }
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
