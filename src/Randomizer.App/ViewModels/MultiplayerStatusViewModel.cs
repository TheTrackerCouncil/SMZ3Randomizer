using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// View model for the multiplayer status window
    /// </summary>
    public class MultiplayerStatusViewModel : INotifyPropertyChanged
    {
        private bool _isConnected;
        private string _gameUrl = "";
        private MultiplayerGameStatus? _gameStatus;
        private bool _allPlayersSubmittedConfigs;
        private GeneratedRom? _generatedRom;

        public MultiplayerStatusViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Players = new List<MultiplayerPlayerStateViewModel>()
                {
                    new MultiplayerPlayerStateViewModel(new MultiplayerPlayerState
                    {
                        Guid = "Player One",
                        PlayerName = "Player One",
                    }, true, false, true, MultiplayerGameStatus.Created),
                    new MultiplayerPlayerStateViewModel(new MultiplayerPlayerState
                    {
                        Guid = "Player Two",
                        PlayerName = "Player Two",
                    }, false, false, true, MultiplayerGameStatus.Created),
                };
            }
            else
            {
                Players = new();
            }
        }

        public string ConnectionStatus => IsConnected ? "Connected" : "Not Connected";
        public Visibility GeneratingLabelVisibility => GameStatus == MultiplayerGameStatus.Generating || (GameStatus == MultiplayerGameStatus.Started && _generatedRom == null) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ReconnectButtonVisibility => IsConnected ? Visibility.Collapsed : Visibility.Visible;
        public Visibility StartButtonVisiblity => (LocalPlayer?.IsAdmin ?? false) && GameStatus == MultiplayerGameStatus.Created ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PlayButtonsVisibility => GameStatus == MultiplayerGameStatus.Started && _generatedRom != null
            ? Visibility.Visible
            : Visibility.Collapsed;
        public bool PlayButtonsEnabled => GameStatus == MultiplayerGameStatus.Started && _generatedRom != null && LocalPlayer?.HasCompleted != true && LocalPlayer?.HasForfeited != true;
        public bool CanStartGame => IsConnected && AllPlayersSubmittedConfigs;
        public MultiplayerPlayerState? LocalPlayer { get; private set; }
        public List<MultiplayerPlayerStateViewModel> Players { get; private set; }

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
                OnPropertyChanged(nameof(CanStartGame));
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

        public MultiplayerGameStatus? GameStatus
        {
            get
            {
                return _gameStatus;
            }
            set
            {
                _gameStatus = value;
                OnPropertyChanged(nameof(GameStatus));
                OnPropertyChanged(nameof(ConnectionStatus));
                OnPropertyChanged(nameof(GeneratingLabelVisibility));
                OnPropertyChanged(nameof(StartButtonVisiblity));
                if (_gameStatus != null)
                {
                    foreach (var player in Players)
                    {
                        player.Update(_gameStatus.Value);
                    }
                }
            }
        }

        public bool AllPlayersSubmittedConfigs
        {
            get
            {
                return _allPlayersSubmittedConfigs;
            }
            set
            {
                _allPlayersSubmittedConfigs = value;
                OnPropertyChanged(nameof(AllPlayersSubmittedConfigs));
                OnPropertyChanged(nameof(CanStartGame));
            }
        }

        public GeneratedRom? GeneratedRom
        {
            get
            {
                return _generatedRom;
            }
            set
            {
                _generatedRom = value;
                OnPropertyChanged(nameof(PlayButtonsEnabled));
                OnPropertyChanged(nameof(PlayButtonsVisibility));
                OnPropertyChanged(nameof(GeneratingLabelVisibility));
            }
        }



        public void UpdateList(List<MultiplayerPlayerState> players, MultiplayerPlayerState? localPlayer)
        {
            LocalPlayer = localPlayer;
            Players = players.Select(x => new MultiplayerPlayerStateViewModel(x, x == localPlayer, localPlayer?.IsAdmin ?? false, IsConnected, GameStatus ?? MultiplayerGameStatus.Created)).ToList();
            OnPropertyChanged();
        }

        public void UpdatePlayer(MultiplayerPlayerState player, MultiplayerPlayerState? localPlayer)
        {
            LocalPlayer = localPlayer;
            var playerViewModel = Players.FirstOrDefault(x => x.PlayerGuid == player.Guid);
            if (playerViewModel != null)
            {
                playerViewModel.Update(player);
                if (player == LocalPlayer) OnPropertyChanged(nameof(PlayButtonsEnabled));
            }
            else
            {
                Players = Players.Concat(new List<MultiplayerPlayerStateViewModel>()
                {
                    new (player, false, localPlayer?.IsAdmin ?? false, IsConnected,
                        GameStatus ?? MultiplayerGameStatus.Created)
                }).ToList();
                OnPropertyChanged();
            }
        }

        public void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
