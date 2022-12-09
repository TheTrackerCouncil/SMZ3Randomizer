using System.ComponentModel;
using System.Windows;
using Randomizer.Shared.Multiplayer;
using Randomizer.Shared;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// View model for an individual player in the muliplayer status window
    /// </summary>
    public class MultiplayerPlayerStateViewModel : INotifyPropertyChanged
    {
        private bool _isConnectedToServer;

        public MultiplayerPlayerStateViewModel(MultiplayerPlayerState state, bool isLocalPlayer, bool isLocalPlayerAdmin, bool isConnectedToServer, MultiplayerGameStatus gameStatus)
        {
            State = state;
            PlayerGuid = state.Guid;
            PlayerName = state.PlayerName;
            IsLocalPlayer = isLocalPlayer;
            IsLocalPlayerAdmin = isLocalPlayerAdmin;
            _isConnectedToServer = isConnectedToServer;
            GameStatus = gameStatus;
        }

        public MultiplayerPlayerState State { get; private set; }
        public string PlayerGuid { get; }
        public string PlayerName { get; }
        public bool IsLocalPlayer { get; }
        public bool IsLocalPlayerAdmin { get;  }
        public string StatusLabel => "(" + Status + ")";
        public MultiplayerGameStatus GameStatus { get; set; }

        public bool IsConnectedToServer
        {
            get
            {
                return _isConnectedToServer;
            }
            set
            {
                _isConnectedToServer = value;
                OnPropertyChanged(nameof(IsConnectedToServer));
                OnPropertyChanged(nameof(StatusLabel));
            }
        }

        public string Status
        {
            get
            {
                if (!IsConnectedToServer) return IsLocalPlayer ? "Disconnected" : "Unknown";
                return State.IsConnected ? State.Status.GetDescription() : "Disconnected";
            }
        }

        public Visibility EditConfigVisibility => GameStatus == MultiplayerGameStatus.Created && IsLocalPlayer
            ? Visibility.Visible
            : Visibility.Collapsed;

        public Visibility ForfeitVisiblity =>
            (IsLocalPlayer || IsLocalPlayerAdmin) && GameStatus != MultiplayerGameStatus.Generating &&
            !State.HasForfeited && !State.HasCompleted
                ? Visibility.Visible
                : Visibility.Collapsed;

        public void Update(MultiplayerPlayerState state)
        {
            State = state;
            OnPropertyChanged();
        }

        public void Update(MultiplayerGameStatus status)
        {
            GameStatus = status;
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
