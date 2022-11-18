using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Randomizer.App.Controls;
using Randomizer.App.ViewModels;
using Randomizer.Data.Multiplayer;
using Randomizer.Multiplayer.Client;

namespace Randomizer.App.Windows
{
    /// <summary>
    /// Interaction logic for MultiplayerStatusWindow.xaml
    /// </summary>
    public partial class MultiplayerStatusWindow : Window
    {
        private readonly MultiplayerClientService _multiplayerClientService;
        private bool _openedConfig;

        public MultiplayerStatusWindow(MultiplayerClientService multiplayerClientService)
        {
            _multiplayerClientService = multiplayerClientService;
            DataContext = Model;
            InitializeComponent();
            UpdatePlayerList();

            if (!_multiplayerClientService.IsConnected)
            {
                Close();
            }

            Model.IsConnected = true;
            Model.GameUrl = _multiplayerClientService.GameUrl ?? "";

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
            UpdatePlayerList();
        }

        private void MultiplayerClientServiceOnPlayerRejoined()
        {
            UpdatePlayerList();
        }

        private void MultiplayerClientServiceOnGameRejoined()
        {
            UpdatePlayerList();
            Model.IsConnected = true;
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
            Model.UpdatePlayer(state, _multiplayerClientService.LocalPlayer);
        }

        public MultiplayerStatusViewModel Model { get; set; } = new();
        public MultiRomListPanel ParentPanel { get; set; } = null!;

        protected void UpdatePlayerList()
        {
            Model.UpdateList(_multiplayerClientService.Players ?? new List<MultiplayerPlayerState>(), _multiplayerClientService.LocalPlayer);
        }

        protected void ShowGenerateRomWindow()
        {
            if (ParentPanel.ShowGenerateRomWindow(null, true) != true) return;
            Task.Run(async () => await _multiplayerClientService.SubmitConfig(ParentPanel.Options.ToConfig()));

        }

        private void UpdateConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGenerateRomWindow();
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGenerateRomWindow();
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
