using AvaloniaControls;
using AvaloniaControls.Models;
using ReactiveUI.SourceGenerators;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Multiplayer;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class MultiplayerPlayerStateViewModel(
    MultiplayerPlayerState state,
    bool isLocalPlayer,
    bool isLocalPlayerAdmin,
    bool isConnectedToServer,
    MultiplayerGameStatus gameStatus)
    : ViewModelBase
{
    [Reactive]
    [ReactiveLinkedProperties(nameof(StatusLabel), nameof(ForfeitVisiblity))]
    public partial MultiplayerPlayerState State { get; private set; } = state;

    public string PlayerGuid { get; } = state.Guid;
    public string PlayerName { get; } = state.PlayerName;
    public bool IsLocalPlayer { get; } = isLocalPlayer;
    public bool IsLocalPlayerAdmin { get; } = isLocalPlayerAdmin;
    public string StatusLabel => "(" + Status + ")";

    [Reactive]
    [ReactiveLinkedProperties(nameof(EditConfigVisibility), nameof(ForfeitVisiblity))]
    public partial MultiplayerGameStatus GameStatus { get; set; } = gameStatus;

    [Reactive] public partial bool IsConnectedToServer { get; set; } = isConnectedToServer;

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
