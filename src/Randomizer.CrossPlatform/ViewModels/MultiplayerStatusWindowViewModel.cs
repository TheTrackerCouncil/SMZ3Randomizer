using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;
using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class MultiplayerStatusWindowViewModel : ViewModelBase
{
    [Reactive] public bool IsConnected { get; set; }

    [Reactive] public string GameUrl { get; set; } = "";

    [Reactive] public MultiplayerGameStatus? GameStatus { get; set; }

    [Reactive] public bool AllPlayersSubmittedConfigs { get; set; }

    [Reactive] public GeneratedRom? GeneratedRom { get; set; }

    public string ConnectionStatus => IsConnected ? "Connected" : "Not Connected";

    public bool GeneratingLabelVisibility => GameStatus == MultiplayerGameStatus.Generating ||
                                                   (GameStatus == MultiplayerGameStatus.Started && GeneratedRom == null);


    public bool ReconnectButtonVisibility => IsConnected;
    public bool StartButtonVisiblity => (LocalPlayer?.IsAdmin ?? false) && GameStatus == MultiplayerGameStatus.Created;
    public bool PlayButtonsVisibility => GameStatus == MultiplayerGameStatus.Started && GeneratedRom != null;
    public bool PlayButtonsEnabled => GameStatus == MultiplayerGameStatus.Started && GeneratedRom != null && LocalPlayer?.HasCompleted != true && LocalPlayer?.HasForfeited != true;
    public bool CanStartGame => IsConnected && AllPlayersSubmittedConfigs;
    public MultiplayerPlayerState? LocalPlayer { get; private set; }
    public List<MultiplayerPlayerStateViewModel> Players { get; private set; }

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
