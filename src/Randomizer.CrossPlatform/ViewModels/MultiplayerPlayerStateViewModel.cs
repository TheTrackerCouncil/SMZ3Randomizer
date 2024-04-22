using AvaloniaControls.Models;
using Randomizer.Shared;
using Randomizer.Shared.Multiplayer;
using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class MultiplayerPlayerStateViewModel : ViewModelBase
{
    public MultiplayerPlayerStateViewModel(MultiplayerPlayerState state, bool isLocalPlayer, bool isLocalPlayerAdmin, bool isConnectedToServer, MultiplayerGameStatus gameStatus)
    {
        State = state;
        PlayerGuid = state.Guid;
        PlayerName = state.PlayerName;
        IsLocalPlayer = isLocalPlayer;
        IsLocalPlayerAdmin = isLocalPlayerAdmin;
        IsConnectedToServer = isConnectedToServer;
        GameStatus = gameStatus;
    }

    [Reactive]
    [ReactiveLinkedProperties(nameof(StatusLabel), nameof(ForfeitVisiblity))]
    public MultiplayerPlayerState State { get; private set; }
    public string PlayerGuid { get; }
    public string PlayerName { get; }
    public bool IsLocalPlayer { get; }
    public bool IsLocalPlayerAdmin { get; }
    public string StatusLabel => "(" + Status + ")";

    [Reactive]
    [ReactiveLinkedProperties(nameof(EditConfigVisibility), nameof(ForfeitVisiblity))]
    public MultiplayerGameStatus GameStatus { get; set; }

    [Reactive] public bool IsConnectedToServer { get; set; }

    public string Status
    {
        get
        {
            if (!IsConnectedToServer) return IsLocalPlayer ? "Disconnected" : "Unknown";
            return State.IsConnected ? State.Status.GetDescription() : "Disconnected";
        }
    }

    public bool EditConfigVisibility => GameStatus == MultiplayerGameStatus.Created && IsLocalPlayer;

    public bool ForfeitVisiblity =>
        (IsLocalPlayer || IsLocalPlayerAdmin) && GameStatus != MultiplayerGameStatus.Generating &&
        State is { HasForfeited: false, HasCompleted: false };

    public void Update(MultiplayerPlayerState state)
    {
        State = state;
    }

    public void Update(MultiplayerGameStatus status)
    {
        GameStatus = status;
    }
}
