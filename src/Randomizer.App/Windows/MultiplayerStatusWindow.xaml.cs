using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Randomizer.App.Controls;
using Randomizer.App.ViewModels;
using Randomizer.Data.Options;
using Randomizer.Shared.Multiplayer;
using Randomizer.Multiplayer.Client;

namespace Randomizer.App.Windows
{
    /// <summary>
    /// Interaction logic for MultiplayerStatusWindow.xaml
    /// </summary>
    public partial class MultiplayerStatusWindow : Window
    {
        private readonly MultiplayerClientService _multiplayerClientService;
        private readonly MultiplayerGameService _multiplayerGameService;
        private bool _openedConfig;

        public MultiplayerStatusWindow(MultiplayerClientService multiplayerClientService, MultiplayerGameService multiplayerGameService)
        {
            _multiplayerClientService = multiplayerClientService;
            _multiplayerGameService = multiplayerGameService;
            DataContext = Model;
            InitializeComponent();
            UpdatePlayerList();

            if (!_multiplayerClientService.IsConnected)
            {
                Close();
            }

            Model.IsConnected = true;
            Model.GameUrl = _multiplayerClientService.GameUrl ?? "";

            _multiplayerGameService.UpdateGameType(_multiplayerClientService.GameType ?? MultiplayerGameType.Multiworld);

            _multiplayerClientService.Error += MultiplayerClientServiceError;
            _multiplayerClientService.GameRejoined += MultiplayerClientServiceOnGameRejoined;
            _multiplayerClientService.GameForfeit += MultiplayerClientServiceOnGameForfeit;
            _multiplayerClientService.PlayerJoined += MultiplayerClientServiceOnPlayerJoined;
            _multiplayerClientService.PlayerRejoined += MultiplayerClientServiceOnPlayerRejoined;
            _multiplayerClientService.PlayerSync += MultiplayerClientServiceOnPlayerSync;
            _multiplayerClientService.Connected += MultiplayerClientServiceOnConnected;
            _multiplayerClientService.ConnectionClosed += MultiplayerClientServiceOnConnectionClosed;
            _multiplayerClientService.PlayerForfeit += MultiplayerClientServiceOnPlayerForfeit;
        }

        private void MultiplayerClientServiceOnGameForfeit()
        {
            if (_multiplayerClientService.GameStatus != MultiplayerGameStatus.Created) return;
            Task.Run(async () => await _multiplayerClientService.Disconnect());
            Close();
        }

        private void MultiplayerClientServiceOnPlayerForfeit()
        {
            Model.GameStatus = _multiplayerClientService.GameStatus;
            UpdatePlayerList();
        }

        private void MultiplayerClientServiceOnPlayerRejoined()
        {
            UpdatePlayerList();
        }

        private void MultiplayerClientServiceOnGameRejoined()
        {
            Model.IsConnected = true;
            Model.GameStatus = _multiplayerClientService.GameStatus;
            UpdatePlayerList();
        }

        private void MultiplayerClientServiceOnConnected()
        {
            Task.Run(async () => await _multiplayerClientService.RejoinGame());
        }

        private void MultiplayerClientServiceOnConnectionClosed(Exception? exception)
        {
            Model.IsConnected = false;

        }

        protected override void OnActivated(EventArgs e)
        {
            if (_multiplayerClientService.LocalPlayer?.Config == null && !_openedConfig)
            {
                ShowGenerateRomWindow();
                _openedConfig = true;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _multiplayerClientService.Error -= MultiplayerClientServiceError;
            _multiplayerClientService.GameRejoined -= MultiplayerClientServiceOnGameRejoined;
            _multiplayerClientService.GameForfeit -= MultiplayerClientServiceOnGameForfeit;
            _multiplayerClientService.PlayerJoined -= MultiplayerClientServiceOnPlayerJoined;
            _multiplayerClientService.PlayerRejoined -= MultiplayerClientServiceOnPlayerRejoined;
            _multiplayerClientService.PlayerSync -= MultiplayerClientServiceOnPlayerSync;
            _multiplayerClientService.Connected -= MultiplayerClientServiceOnConnected;
            _multiplayerClientService.ConnectionClosed -= MultiplayerClientServiceOnConnectionClosed;
            _multiplayerClientService.PlayerForfeit -= MultiplayerClientServiceOnPlayerForfeit;
            Task.Run(async () => await _multiplayerClientService.Disconnect());
        }

        private void MultiplayerClientServiceError(string error, Exception? exception)
        {
            if (Dispatcher.CheckAccess())
            {
                MessageBox.Show(this, error , "SMZ3 Cas' Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    MultiplayerClientServiceError(error, exception);
                });
            }
        }

        private void MultiplayerClientServiceOnPlayerJoined()
        {
            UpdatePlayerList();
        }

        private void MultiplayerClientServiceOnPlayerSync(MultiplayerPlayerState? previousState, MultiplayerPlayerState state)
        {
            Model.GameStatus = _multiplayerClientService.GameStatus;
            Model.UpdatePlayer(state, _multiplayerClientService.LocalPlayer);
            CheckPlayerConfigs();
        }

        public MultiplayerStatusViewModel Model { get; set; } = new();
        public MultiRomListPanel ParentPanel { get; set; } = null!;

        protected void UpdatePlayerList()
        {
            Model.GameStatus = _multiplayerClientService.GameStatus;
            Model.UpdateList(_multiplayerClientService.Players ?? new List<MultiplayerPlayerState>(), _multiplayerClientService.LocalPlayer);
            CheckPlayerConfigs();
        }

        protected void CheckPlayerConfigs()
        {
            if (_multiplayerClientService.LocalPlayer?.IsAdmin == true)
            {
                Model.AllPlayersSubmittedConfigs =
                    _multiplayerClientService.Players?.All(x => x.Config != null) ?? false;
            }
        }

        protected void ShowGenerateRomWindow()
        {
            if (ParentPanel.ShowGenerateRomWindow(null, true) != true) return;
            var config = ParentPanel.Options.ToConfig();
            config.PlayerGuid = _multiplayerClientService.CurrentPlayerGuid!;
            config.PlayerName = _multiplayerClientService.LocalPlayer!.PlayerName;
            Task.Run(async () => await _multiplayerClientService.SubmitConfig(Config.ToConfigString(config)));
        }

        private void UpdateConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGenerateRomWindow();
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_multiplayerClientService.Players == null)
            {
                MessageBox.Show(this, "No players found to start the game" , "SMZ3 Cas' Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            var states = _multiplayerGameService.CreateWorld(_multiplayerClientService.Players!);
        }

        private void OpenTrackerButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGenerateRomWindow();
        }

        private async void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await _multiplayerClientService.Reconnect();
        }

        private async void ForfeitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: MultiplayerPlayerState state })
                return;
            await _multiplayerClientService.ForfeitGame(state.Guid);
        }

        private void CopyUrlButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentPanel.CopyTextToClipboard(_multiplayerClientService.GameUrl ?? "");
        }
    }
}
