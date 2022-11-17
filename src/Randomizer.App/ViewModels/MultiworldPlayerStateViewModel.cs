using System.ComponentModel;
using System.Windows;
using Randomizer.Data.Multiworld;
using Randomizer.Shared;

namespace Randomizer.App.ViewModels
{
    public class MultiworldPlayerStateViewModel : INotifyPropertyChanged
    {
        private bool _isConnectedToServer;
        public MultiworldPlayerStateViewModel(MultiworldPlayerState state, bool isLocalPlayer, bool isLocalPlayerAdmin, bool isConnectedToServer)
        {
            State = state;
            PlayerGuid = state.Guid;
            PlayerName = state.PlayerName;
            IsLocalPlayer = isLocalPlayer;
            IsLocalPlayerAdmin = isLocalPlayerAdmin;
            _isConnectedToServer = isConnectedToServer;
        }

        public MultiworldPlayerState State { get; private set; }
        public string PlayerGuid { get; }
        public string PlayerName { get; }
        public bool IsLocalPlayer { get; }
        public bool IsLocalPlayerAdmin { get;  }
        public string StatusLabel => "(" + Status + ")";

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
                return State.Status.GetDescription();
            }
        }

        public Visibility EditConfigVisibility => IsLocalPlayer ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ForfeitVisiblity => IsLocalPlayer || IsLocalPlayerAdmin ? Visibility.Visible : Visibility.Collapsed;

        public void Update(MultiworldPlayerState state)
        {
            State = state;
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
