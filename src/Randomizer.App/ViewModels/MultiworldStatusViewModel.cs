using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Randomizer.Data.Multiworld;
using Randomizer.SMZ3.Tracking.AutoTracking;

namespace Randomizer.App.ViewModels
{
    public class MultiworldStatusViewModel : INotifyPropertyChanged
    {
        private bool _isConnected;
        private string _gameUrl = "";

        public MultiworldStatusViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Players = new List<MultiworldPlayerStateViewModel>()
                {
                    new MultiworldPlayerStateViewModel(new MultiworldPlayerState
                    {
                        Guid = "Player One",
                        PlayerName = "Player One",
                    }, true, false, true),
                    new MultiworldPlayerStateViewModel(new MultiworldPlayerState
                    {
                        Guid = "Player Two",
                        PlayerName = "Player Two",
                    }, false, false, true),
                };
            }
            else
            {
                Players = new();
            }
        }


        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(ConnectionStatus));
                OnPropertyChanged(nameof(ReconnectButtonVisibility));
                Players.ForEach(x => x.IsConnectedToServer = value);
            }
        }

        public string GameUrl
        {
            get
            {
                return _gameUrl;
            }
            set
            {
                _gameUrl = value;
                OnPropertyChanged(nameof(GameUrl));
            }
        }

        public string ConnectionStatus => IsConnected ? "Connected" : "Not Connected";

        public Visibility ReconnectButtonVisibility => IsConnected ? Visibility.Collapsed : Visibility.Visible;

        public List<MultiworldPlayerStateViewModel> Players { get; private set; }

        public void UpdateList(List<MultiworldPlayerState> players, MultiworldPlayerState? localPlayer)
        {
            Players = players.Select(x => new MultiworldPlayerStateViewModel(x, x == localPlayer, localPlayer?.IsAdmin ?? false, IsConnected)).ToList();
            OnPropertyChanged();
        }

        public void UpdatePlayer(MultiworldPlayerState player, MultiworldPlayerState? localPlayer)
        {
            var playerViewModel = Players.FirstOrDefault(x => x.PlayerGuid == player.Guid);
            if (playerViewModel != null)
                playerViewModel.Update(player);
            else
            {
                Players.Add(new MultiworldPlayerStateViewModel(player, false, localPlayer?.IsAdmin ?? false, IsConnected));
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
