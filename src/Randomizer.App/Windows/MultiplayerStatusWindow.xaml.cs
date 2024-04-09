using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Randomizer.App.Controls;
using Randomizer.App.ViewModels;
using Randomizer.Data.Interfaces;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Multiplayer;
using Randomizer.Multiplayer.Client;
using Randomizer.Shared;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Generation;

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
        private readonly IRomGenerationService _romGenerationService;
        private readonly SpriteService _spriteService;
        private string _previousError = "";

        public MultiplayerStatusWindow(MultiplayerClientService multiplayerClientService,
            MultiplayerGameService multiplayerGameService, IRomGenerationService romGenerationService, SpriteService spriteService)
        {
            _multiplayerClientService = multiplayerClientService;
            _multiplayerGameService = multiplayerGameService;
            _romGenerationService = romGenerationService;
            _spriteService = spriteService;
            DataContext = Model;
            InitializeComponent();

            _multiplayerClientService.Error += MultiplayerClientServiceError;
            _multiplayerClientService.GameRejoined += MultiplayerClientServiceOnGameRejoined;
            _multiplayerClientService.PlayerForfeited += MultiplayerClientServiceOnPlayerForfeit;
            _multiplayerClientService.PlayerListUpdated += MultiplayerClientServiceOnPlayerListUpdated;
            _multiplayerClientService.PlayerUpdated += MultiplayerClientServiceOnPlayerSync;
            _multiplayerClientService.Connected += MultiplayerClientServiceOnConnected;
            _multiplayerClientService.ConnectionClosed += MultiplayerClientServiceOnConnectionClosed;
            _multiplayerClientService.GameStateUpdated += MultiplayerClientServiceOnGameStateUpdated;
            _multiplayerClientService.GameStarted += MultiplayerClientServiceOnGameStarted;
        }

        public MultiplayerStatusViewModel Model { get; set; } = new();
        public MultiRomListPanel ParentPanel { get; set; } = null!;
        public MultiplayerGameDetails? MultiplayerGameDetails { get; set; }

        private async void MultiplayerClientServiceOnGameStarted(List<MultiplayerPlayerGenerationData> playerGenerationData)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => MultiplayerClientServiceOnGameStarted(playerGenerationData));
                return;
            }

            // Regenerate the seed using all of the data for the players that came from the server
            var seedData = _multiplayerGameService.RegenerateSeed(playerGenerationData, out var error);
            if (!string.IsNullOrEmpty(error))
            {
                DisplayError(error);
                return;
            }

            var rom = await _romGenerationService.GeneratePreSeededRomAsync(ParentPanel.Options, seedData!, _multiplayerClientService.DatabaseGameDetails!);
            if (rom.Rom != null)
            {
                Model.GeneratedRom = rom.Rom;
                DisplayMessage("Rom successfully generated.\nTo begin, launch tracker and the rom, then start auto tracking.");
            }
        }

        private void MultiplayerClientServiceOnGameStateUpdated()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(MultiplayerClientServiceOnGameStateUpdated);
                return;
            }
            Model.GameStatus = _multiplayerClientService.GameStatus;
        }

        private async void MultiplayerClientServiceOnPlayerForfeit(MultiplayerPlayerState playerState, bool isLocalPlayer)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => MultiplayerClientServiceOnPlayerForfeit(playerState, isLocalPlayer));
                return;
            }
            if (!isLocalPlayer || _multiplayerClientService.GameStatus != MultiplayerGameStatus.Created) return;
            await _multiplayerClientService.Disconnect();
            Close();
        }

        private void MultiplayerClientServiceOnGameRejoined()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(MultiplayerClientServiceOnGameRejoined);
                return;
            }
            Model.IsConnected = true;
            Model.GameUrl = _multiplayerClientService.GameUrl ?? "";
            Model.GameStatus = _multiplayerClientService.GameStatus ?? MultiplayerGameStatus.Created;
            UpdatePlayerList();
            Model.Refresh();
        }

        private async void MultiplayerClientServiceOnConnected()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(MultiplayerClientServiceOnConnected);
                return;
            }
            await _multiplayerClientService.RejoinGame();
        }

        private void MultiplayerClientServiceOnConnectionClosed(string message, Exception? exception)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => MultiplayerClientServiceOnConnectionClosed(message, exception));
                return;
            }
            Model.IsConnected = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _multiplayerClientService.Error -= MultiplayerClientServiceError;
            _multiplayerClientService.GameRejoined -= MultiplayerClientServiceOnGameRejoined;
            _multiplayerClientService.PlayerListUpdated -= MultiplayerClientServiceOnPlayerListUpdated;
            _multiplayerClientService.PlayerUpdated -= MultiplayerClientServiceOnPlayerSync;
            _multiplayerClientService.Connected -= MultiplayerClientServiceOnConnected;
            _multiplayerClientService.ConnectionClosed -= MultiplayerClientServiceOnConnectionClosed;
            _multiplayerClientService.GameStateUpdated -= MultiplayerClientServiceOnGameStateUpdated;
            _multiplayerClientService.GameStarted -= MultiplayerClientServiceOnGameStarted;
            _multiplayerClientService.PlayerForfeited -= MultiplayerClientServiceOnPlayerForfeit;
            Task.Run(async () => await _multiplayerClientService.Disconnect());
            ParentPanel.CloseTracker();
            ParentPanel.UpdateList();
        }

        private void MultiplayerClientServiceError(string error, Exception? exception)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => MultiplayerClientServiceError(error, exception));
                return;
            }
            DisplayError(error);
        }

        private void MultiplayerClientServiceOnPlayerListUpdated()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(MultiplayerClientServiceOnPlayerListUpdated);
                return;
            }
            UpdatePlayerList();
        }

        private void MultiplayerClientServiceOnPlayerSync(MultiplayerPlayerState state, MultiplayerPlayerState? previousState, bool isLocalPlayer)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => MultiplayerClientServiceOnPlayerSync(state,  previousState, isLocalPlayer));
                return;
            }
            Model.UpdatePlayer(state, _multiplayerClientService.LocalPlayer);
            CheckPlayerConfigs();
        }

        protected void UpdatePlayerList()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(UpdatePlayerList);
                return;
            }
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
            config.Seed = ""; // Not currently supported in multiplayer
            config.SettingsString = ""; // Not currently supported in multiplayer
            config.PlayerGuid = _multiplayerClientService.CurrentPlayerGuid!;
            config.PlayerName = _multiplayerClientService.LocalPlayer!.PlayerName;
            config.PhoneticName = _multiplayerClientService.LocalPlayer!.PhoneticName;
            config.Race = false;  // Not currently supported in multiplayer
            config.ItemPlacementRule = ItemPlacementRule.Anywhere; // Not currently supported in multiplayer
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

            await _multiplayerClientService.UpdateGameStatus(MultiplayerGameStatus.Generating);

            var error = await _multiplayerGameService.GenerateSeed();

            // If an error happened, set it back to being to the initial state
            if (error != null)
            {
                DisplayError(error);
                await _multiplayerClientService.UpdateGameStatus(MultiplayerGameStatus.Created);
            }
        }

        private void OpenTrackerButton_Click(object sender, RoutedEventArgs e)
        {
            if (Model.GeneratedRom != null) ParentPanel.QuickLaunchRom(Model.GeneratedRom!);
        }

        private async void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await _multiplayerClientService.Reconnect();
        }

        private async void ForfeitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: MultiplayerPlayerState state })
                return;
            await _multiplayerClientService.ForfeitPlayerGame(state.Guid);
        }

        private void CopyUrlButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentPanel.CopyTextToClipboard(_multiplayerClientService.GameUrl ?? "");
        }

        private MessageBoxResult? DisplayError(string message)
        {
            if (message == _previousError)
            {
                return null;
            }

            _previousError = message;
            return DisplayMessage(message, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private MessageBoxResult DisplayMessage(string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            if (Dispatcher.CheckAccess())
            {
                return MessageBox.Show(this, message , "SMZ3 Cas' Randomizer", buttons, image);
            }
            else
            {
                var output = MessageBoxResult.OK;
                Dispatcher.Invoke(() =>
                {
                    output = DisplayMessage(message, buttons, image);
                });
                return output;
            }
        }

        private async void MultiplayerStatusWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_multiplayerClientService.IsConnected && MultiplayerGameDetails != null)
            {
                Model.GeneratedRom = MultiplayerGameDetails.GeneratedRom;
                await _multiplayerClientService.Reconnect(MultiplayerGameDetails);
            }
            else if (_multiplayerClientService.IsConnected)
            {
                MultiplayerClientServiceOnGameRejoined();
            }
        }

        /// <summary>
        /// Right click menu to play a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Model.GeneratedRom != null)
                ParentPanel.LaunchRom(Model.GeneratedRom);
        }

        /// <summary>
        /// Right click menu to open the folder for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Model.GeneratedRom != null)
                ParentPanel.OpenFolder(Model.GeneratedRom);
        }

        /// <summary>
        /// Menu item for opening the tracker for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenTrackerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Model.GeneratedRom != null)
                ParentPanel.LaunchTracker(Model.GeneratedRom);
        }

        /// <summary>
        /// Menu item for viewing the spoiler log for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewSpoilerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Model.GeneratedRom != null)
                ParentPanel.OpenSpoilerLog(Model.GeneratedRom);
        }

        /// <summary>
        /// Opens the launch options drop down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchOptions_OnClick(object sender, RoutedEventArgs e)
        {
            if (LaunchOptions.ContextMenu == null) return;
            LaunchOptions.ContextMenu.DataContext = LaunchOptions.DataContext;
            LaunchOptions.ContextMenu.IsOpen = true;
        }
    }
}
