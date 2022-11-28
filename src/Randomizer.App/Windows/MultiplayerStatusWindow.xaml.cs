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
    /// This window lists all of the players and allows players to update their configs and forfeit
    /// </summary>
    public partial class MultiplayerStatusWindow : Window
    {
        private readonly MultiplayerClientService _multiplayerClientService;
        private readonly MultiplayerGameService _multiplayerGameService;
        private readonly RomGenerator _romGenerator;

        public MultiplayerStatusWindow(MultiplayerClientService multiplayerClientService, MultiplayerGameService multiplayerGameService, RomGenerator romGenerator)
        {
            _multiplayerClientService = multiplayerClientService;
            _multiplayerGameService = multiplayerGameService;
            _romGenerator = romGenerator;
            DataContext = Model;
            InitializeComponent();
            UpdatePlayerList();

            if (!_multiplayerClientService.IsConnected)
            {
                Close();
            }

            Model.IsConnected = true;
            Model.GameUrl = _multiplayerClientService.GameUrl ?? "";
            Model.GameStatus = _multiplayerClientService.GameStatus ?? MultiplayerGameStatus.Created;
            Model.UpdateList(_multiplayerClientService.Players ?? new List<MultiplayerPlayerState>(),
                _multiplayerClientService.LocalPlayer);

            _multiplayerGameService.UpdateGameType(_multiplayerClientService.GameType ?? MultiplayerGameType.Multiworld);

            _multiplayerClientService.Error += MultiplayerClientServiceError;
            _multiplayerClientService.GameRejoined += MultiplayerClientServiceOnGameRejoined;
            _multiplayerClientService.GameForfeit += MultiplayerClientServiceOnGameForfeit;
            _multiplayerClientService.PlayerListUpdated += MultiplayerClientServiceOnPlayerListUpdated;
            _multiplayerClientService.PlayerUpdated += MultiplayerClientServiceOnPlayerSync;
            _multiplayerClientService.Connected += MultiplayerClientServiceOnConnected;
            _multiplayerClientService.ConnectionClosed += MultiplayerClientServiceOnConnectionClosed;
            _multiplayerClientService.GameStateUpdated += MultiplayerClientServiceOnGameStateUpdated;
            _multiplayerClientService.SeedDataGenerated += MultiplayerClientServiceOnSeedDataGenerated;
        }

        private async void MultiplayerClientServiceOnSeedDataGenerated(string seed, string validationhash)
        {
            var seedData = _multiplayerGameService.RegenerateSeed(seed, validationhash, out var error);
            if (!string.IsNullOrEmpty(error))
            {
                DisplayError(error);
                return;
            }

            var rom = await _romGenerator.GeneratePreSeededRomAsync(ParentPanel.Options, seedData!, _multiplayerClientService.GameType!.Value, _multiplayerClientService.GameUrl!);
            DisplayError("Rom generated");
        }

        private void MultiplayerClientServiceOnGameStateUpdated()
        {
            Model.GameStatus = _multiplayerClientService.GameStatus;
        }

        private async void MultiplayerClientServiceOnGameForfeit()
        {
            if (_multiplayerClientService.GameStatus != MultiplayerGameStatus.Created) return;
            await _multiplayerClientService.Disconnect();
            Close();
        }

        private void MultiplayerClientServiceOnGameRejoined()
        {
            Model.IsConnected = true;
        }

        private async void MultiplayerClientServiceOnConnected()
        {
            await _multiplayerClientService.RejoinGame();
        }

        private void MultiplayerClientServiceOnConnectionClosed(string message, Exception? exception)
        {
            Model.IsConnected = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _multiplayerClientService.Error -= MultiplayerClientServiceError;
            _multiplayerClientService.GameRejoined -= MultiplayerClientServiceOnGameRejoined;
            _multiplayerClientService.GameForfeit -= MultiplayerClientServiceOnGameForfeit;
            _multiplayerClientService.PlayerListUpdated -= MultiplayerClientServiceOnPlayerListUpdated;
            _multiplayerClientService.PlayerUpdated -= MultiplayerClientServiceOnPlayerSync;
            _multiplayerClientService.Connected -= MultiplayerClientServiceOnConnected;
            _multiplayerClientService.ConnectionClosed -= MultiplayerClientServiceOnConnectionClosed;
            _multiplayerClientService.GameStateUpdated -= MultiplayerClientServiceOnGameStateUpdated;
            _multiplayerClientService.SeedDataGenerated -= MultiplayerClientServiceOnSeedDataGenerated;
            Task.Run(async () => await _multiplayerClientService.Disconnect());
        }

        private void MultiplayerClientServiceError(string error, Exception? exception)
        {
            DisplayError(error);
        }

        private void MultiplayerClientServiceOnPlayerListUpdated()
        {
            UpdatePlayerList();
        }

        private void MultiplayerClientServiceOnPlayerSync(MultiplayerPlayerState state)
        {
            Model.UpdatePlayer(state, _multiplayerClientService.LocalPlayer);
            CheckPlayerConfigs();
        }

        public MultiplayerStatusViewModel Model { get; set; } = new();
        public MultiRomListPanel ParentPanel { get; set; } = null!;

        protected void UpdatePlayerList()
        {
            Model.UpdateList(_multiplayerClientService.Players ?? new List<MultiplayerPlayerState>(), _multiplayerClientService.LocalPlayer);
            CheckPlayerConfigs();
        }

        protected void CheckPlayerConfigs()
        {
            if (_multiplayerClientService.GameStatus == MultiplayerGameStatus.Created && _multiplayerClientService.LocalPlayer?.IsAdmin == true)
            {
                Model.AllPlayersSubmittedConfigs =
                    _multiplayerClientService.Players?.All(x => x.Config != null) ?? false;
            }
        }

        protected async Task ShowGenerateRomWindow()
        {
            if (ParentPanel.ShowGenerateRomWindow(null, true) != true) return;
            var config = ParentPanel.Options.ToConfig();
            config.PlayerGuid = _multiplayerClientService.CurrentPlayerGuid!;
            config.PlayerName = _multiplayerClientService.LocalPlayer!.PlayerName;
            await _multiplayerClientService.SubmitConfig(Config.ToConfigString(config));
        }

        private async void UpdateConfigButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowGenerateRomWindow();
        }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_multiplayerClientService.Players == null)
            {
                DisplayError("No players found to start the game.");
            }

            await _multiplayerClientService.UpdateGameState(MultiplayerGameStatus.Generating);

            var error = await _multiplayerGameService.GenerateSeed();

            if (error != null)
            {
                DisplayError(error);
            }
        }

        private async void OpenTrackerButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowGenerateRomWindow();
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

        private void DisplayError(string message)
        {
            if (Dispatcher.CheckAccess())
            {
                MessageBox.Show(this, message , "SMZ3 Cas' Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    DisplayError(message);
                });
            }
        }
    }
}
