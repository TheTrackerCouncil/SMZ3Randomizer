using System.Collections.Generic;
using System.Linq;
using AvaloniaControls.Models;
using ReactiveUI.SourceGenerators;
using TrackerCouncil.Smz3.Shared.Models;
using TrackerCouncil.Smz3.Shared.Multiplayer;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class MultiplayerStatusWindowViewModel : ViewModelBase
{
    [Reactive]
    [ReactiveLinkedProperties(nameof(ReconnectButtonVisibility), nameof(CanStartGame), nameof(ConnectionStatus))]
    public partial bool IsConnected { get; set; }

    [Reactive] public partial string GameUrl { get; set; } = "";

    [Reactive]
    [ReactiveLinkedProperties(nameof(StartButtonVisiblity), nameof(GeneratingLabelVisibility), nameof(PlayButtonsVisibility), nameof(PlayButtonsEnabled))]
    public partial MultiplayerGameStatus? GameStatus { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(CanStartGame))]
    public partial bool AllPlayersSubmittedConfigs { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(StartButtonVisiblity), nameof(GeneratingLabelVisibility), nameof(PlayButtonsVisibility), nameof(PlayButtonsEnabled))]
    public partial GeneratedRom? GeneratedRom { get; set; }

    [Reactive] public partial MultiplayerGameDetails? Details { get; set; }

    public string ConnectionStatus => IsConnected ? "Connected" : "Not Connected";

    public bool GeneratingLabelVisibility => GameStatus == MultiplayerGameStatus.Generating ||
                                                   (GameStatus == MultiplayerGameStatus.Started && GeneratedRom == null);

    public bool ReconnectButtonVisibility => !IsConnected;
    public bool StartButtonVisiblity => (LocalPlayer?.IsAdmin ?? false) && GameStatus == MultiplayerGameStatus.Created;
    public bool PlayButtonsVisibility => GameStatus == MultiplayerGameStatus.Started && GeneratedRom != null;
    public bool PlayButtonsEnabled => GameStatus == MultiplayerGameStatus.Started && GeneratedRom != null && LocalPlayer?.HasCompleted != true && LocalPlayer?.HasForfeited != true;
    public bool CanStartGame => IsConnected && AllPlayersSubmittedConfigs;

    [Reactive]
    [ReactiveLinkedProperties(nameof(StartButtonVisiblity))]
    public partial MultiplayerPlayerState? LocalPlayer { get; private set; }
    [Reactive] public partial List<MultiplayerPlayerStateViewModel> Players { get; private set; } = [];

    public void UpdateList(List<MultiplayerPlayerState> players, MultiplayerPlayerState? localPlayer)
    {
        LocalPlayer = localPlayer;
        Players = players.Select(x => new MultiplayerPlayerStateViewModel(x, x == localPlayer, localPlayer?.IsAdmin ?? false, IsConnected, GameStatus ?? MultiplayerGameStatus.Created)).ToList();
    }

    public void UpdatePlayer(MultiplayerPlayerState player, MultiplayerPlayerState? localPlayer)
    {
        LocalPlayer = localPlayer;
        var playerViewModel = Players.FirstOrDefault(x => x.PlayerGuid == player.Guid);
        if (playerViewModel != null)
        {
            playerViewModel.Update(player);
        }
        else
        {
            Players = Players.Concat(new List<MultiplayerPlayerStateViewModel>()
            {
                new (player, false, localPlayer?.IsAdmin ?? false, IsConnected,
                    GameStatus ?? MultiplayerGameStatus.Created)
            }).ToList();
        }
    }
}
