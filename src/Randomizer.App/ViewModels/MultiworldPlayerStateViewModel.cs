using System.ComponentModel;
using System.Windows;
using Randomizer.Data.Multiworld;

namespace Randomizer.App.ViewModels
{
    public class MultiworldPlayerStateViewModel : INotifyPropertyChanged
    {
        public MultiworldPlayerStateViewModel(MultiworldPlayerState state, bool isLocalPlayer)
        {
            State = state;
            PlayerGuid = state.Guid;
            PlayerName = state.PlayerName;
            IsLocalPlayer = isLocalPlayer;
        }

        public MultiworldPlayerState State { get; }
        public string PlayerGuid { get; }
        public string PlayerName { get; }
        public bool IsLocalPlayer { get; }
        public string StatusLabel => "(" + Status + ")";
        public string Status { get; private set; } = "Connected";

        public Visibility EditConfigVisibility => IsLocalPlayer ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ForfeitVisiblity => IsLocalPlayer || State.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        public void Update(MultiworldPlayerState state)
        {
            Status = "Updated";
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
