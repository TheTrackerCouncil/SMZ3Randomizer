using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.Data.Interfaces;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Multiplayer.Client;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Multiplayer;
using TrackerCouncil.Smz3.UI.ViewModels;
using TrackerCouncil.Smz3.UI.Views;

namespace TrackerCouncil.Smz3.UI.Services;

public class MultiplayerStatusWindowService(MultiplayerClientService multiplayerClientService,
    MultiplayerGameService multiplayerGameService,
    IRomGenerationService romGenerationService,
    OptionsFactory optionsFactory,
    SharedCrossplatformService sharedCrossplatformService) : ControlService, IDisposable
{
    private MultiplayerStatusWindow _window = null!;
    private MultiplayerStatusWindowViewModel _model = new();
    private RandomizerOptions _options = null!;
    private TrackerWindow? _trackerWindow;

    public MultiplayerStatusWindowViewModel GetViewModel(MultiplayerStatusWindow window, MultiplayerRomViewModel romModel)
    {
        sharedCrossplatformService.LookupMsus();

        sharedCrossplatformService.ParentControl = window;

        _options = optionsFactory.Create();

        multiplayerClientService.Error += MultiplayerClientServiceOnError;
        multiplayerClientService.GameRejoined += MultiplayerClientServiceOnGameRejoined;
        multiplayerClientService.PlayerForfeited += MultiplayerClientServiceOnPlayerForfeited;
        multiplayerClientService.PlayerListUpdated += MultiplayerClientServiceOnPlayerListUpdated;
        multiplayerClientService.PlayerUpdated += MultiplayerClientServiceOnPlayerUpdated;
        multiplayerClientService.Connected += MultiplayerClientServiceOnConnected;
        multiplayerClientService.ConnectionClosed += MultiplayerClientServiceOnConnectionClosed;
        multiplayerClientService.GameStateUpdated += MultiplayerClientServiceOnGameStateUpdated;
        multiplayerClientService.GameStarted += MultiplayerClientServiceOnGameStarted;

        _window = window;
        _model.GeneratedRom = romModel.Details.GeneratedRom;
        _model.Details = romModel.Details;
        return _model;
    }

    public async Task Connect()
    {
        if (!multiplayerClientService.IsConnected && _model.Details != null)
        {
            _model.GeneratedRom = _model.Details.GeneratedRom;
            await multiplayerClientService.Reconnect(_model.Details);
        }
        else if (multiplayerClientService.IsConnected)
        {
            MultiplayerClientServiceOnGameRejoined();
        }
    }

    public async Task SubmitConfig()
    {
        var options = await sharedCrossplatformService.OpenGenerationWindow(isMultiplayer: true);

        if (options == null)
        {
            return;
        }

        var config = _options.ToConfig();
        config.Seed = ""; // Not currently supported in multiplayer
        config.PlayerGuid = multiplayerClientService.CurrentPlayerGuid!;
        config.PlayerName = multiplayerClientService.LocalPlayer!.PlayerName;
        config.PhoneticName = multiplayerClientService.LocalPlayer!.PhoneticName;
        config.Race = false;  // Not currently supported in multiplayer
        config.ItemPlacementRule = ItemPlacementRule.Anywhere; // Not currently supported in multiplayer
        await multiplayerClientService.SubmitConfig(Config.ToConfigString(config));
    }

    public async Task Forfeit(MultiplayerPlayerStateViewModel player)
    {
        await multiplayerClientService.ForfeitPlayerGame(player.State.Guid);
    }

    public async Task Reconnect()
    {
        await multiplayerClientService.Reconnect();
    }

    public void OpenFolder()
    {
        sharedCrossplatformService.OpenFolder(_model.GeneratedRom);
    }

    public void PlayRom()
    {
        sharedCrossplatformService.PlayRom(_model.GeneratedRom);
    }

    public async Task StartGame()
    {
        if (multiplayerClientService.Players == null)
        {
            DisplayError("No players found to start the game.");
        }

        await multiplayerClientService.UpdateGameStatus(MultiplayerGameStatus.Generating);

        var error = await multiplayerGameService.GenerateSeed();

        // If an error happened, set it back to being to the initial state
        if (error != null)
        {
            DisplayError(error);
            await multiplayerClientService.UpdateGameStatus(MultiplayerGameStatus.Created);
        }
    }

    public void OpenSpoilerLog()
    {
        sharedCrossplatformService.OpenSpoilerLog(_model.GeneratedRom);
    }

    public async Task LaunchTracker()
    {
        _trackerWindow = await sharedCrossplatformService.LaunchTrackerAsync(_model.GeneratedRom);
        FinalizeLaunch();
    }

    public async Task LaunchRom()
    {
        _trackerWindow = await sharedCrossplatformService.LaunchRom(_model.GeneratedRom);
        FinalizeLaunch();
    }

    private void FinalizeLaunch()
    {
        if (_trackerWindow != null)
        {
            _trackerWindow.Closed += (_, _) =>
            {
                _trackerWindow = null;
                if (!_window.IsVisible)
                {
                    _window.Show(MessageWindow.GlobalParentWindow!);
                }
            };
        }
    }

    private async void MultiplayerClientServiceOnGameStarted(List<MultiplayerPlayerGenerationData> playerGenerationData)
    {
        var seedData = multiplayerGameService.RegenerateSeed(playerGenerationData, out var error);
        if (!string.IsNullOrEmpty(error))
        {
            DisplayError(error);
            return;
        }

        var rom = await romGenerationService.GeneratePreSeededRomAsync(_options, seedData!, multiplayerClientService.DatabaseGameDetails!);
        if (rom.Rom != null)
        {
            _model.GeneratedRom = rom.Rom;
            DisplayMessage("Rom successfully generated.\nTo begin, launch tracker and the rom, then start auto tracking.");
        }
    }

    private void MultiplayerClientServiceOnGameStateUpdated()
    {
        _model.GameStatus = multiplayerClientService.GameStatus;
    }

    private void MultiplayerClientServiceOnConnectionClosed(string error, Exception? exception)
    {
        _model.IsConnected = false;
    }

    private async void MultiplayerClientServiceOnConnected()
    {
        await multiplayerClientService.RejoinGame();
    }

    private void MultiplayerClientServiceOnPlayerUpdated(MultiplayerPlayerState state, MultiplayerPlayerState? previousstate, bool islocalplayer)
    {
        _model.UpdatePlayer(state, multiplayerClientService.LocalPlayer);
        CheckPlayerConfigs();
    }

    private void MultiplayerClientServiceOnPlayerListUpdated()
    {
        UpdatePlayerList();
    }

    private async void MultiplayerClientServiceOnPlayerForfeited(MultiplayerPlayerState state, bool isLocalPlayer)
    {
        if (!isLocalPlayer || multiplayerClientService.GameStatus != MultiplayerGameStatus.Created) return;
        await multiplayerClientService.Disconnect();
        Dispatcher.UIThread.Invoke(() => _window.Close());
    }

    private void MultiplayerClientServiceOnGameRejoined()
    {
        _model.IsConnected = true;
        _model.GameUrl = multiplayerClientService.GameUrl ?? "";
        _model.GameStatus = multiplayerClientService.GameStatus ?? MultiplayerGameStatus.Created;
        UpdatePlayerList();
    }

    private void MultiplayerClientServiceOnError(string error, Exception? exception)
    {
        DisplayError(error);
    }

    private void DisplayError(string message)
    {
        DisplayMessage(message, MessageWindowIcon.Error);
    }

    private void UpdatePlayerList()
    {
        _model.UpdateList(multiplayerClientService.Players ?? new List<MultiplayerPlayerState>(), multiplayerClientService.LocalPlayer);
        CheckPlayerConfigs();
    }

    private void CheckPlayerConfigs()
    {
        if (multiplayerClientService is { GameStatus: MultiplayerGameStatus.Created, LocalPlayer.IsAdmin: true })
        {
            _model.AllPlayersSubmittedConfigs =
                multiplayerClientService.Players?.All(x => x.Config != null) ?? false;
        }
    }

    private void DisplayMessage(string message, MessageWindowIcon icon = MessageWindowIcon.None, MessageWindowButtons buttons = MessageWindowButtons.OK)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Invoke(() => DisplayMessage(message));
            return;
        }

        var window = new MessageWindow(new MessageWindowRequest()
        {
            Message = message, Title = "SMZ3 Cas' Randomizer", Icon = icon, Buttons = buttons
        });
        window.ShowDialog(_window.Owner ?? _window);
    }

    public void Dispose()
    {
        _trackerWindow?.Close();

        multiplayerGameService.Dispose();
        ITaskService.Run(async () =>
        {
            await multiplayerClientService.Disconnect();
        });
        multiplayerClientService.Error -= MultiplayerClientServiceOnError;
        multiplayerClientService.GameRejoined -= MultiplayerClientServiceOnGameRejoined;
        multiplayerClientService.PlayerForfeited -= MultiplayerClientServiceOnPlayerForfeited;
        multiplayerClientService.PlayerListUpdated -= MultiplayerClientServiceOnPlayerListUpdated;
        multiplayerClientService.PlayerUpdated -= MultiplayerClientServiceOnPlayerUpdated;
        multiplayerClientService.Connected -= MultiplayerClientServiceOnConnected;
        multiplayerClientService.ConnectionClosed -= MultiplayerClientServiceOnConnectionClosed;
        multiplayerClientService.GameStateUpdated -= MultiplayerClientServiceOnGameStateUpdated;
        multiplayerClientService.GameStarted -= MultiplayerClientServiceOnGameStarted;
        GC.SuppressFinalize(this);

    }
}
