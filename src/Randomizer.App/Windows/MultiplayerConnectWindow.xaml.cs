using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using Randomizer.Multiplayer.Client;

namespace Randomizer.App.Windows
{
    /// <summary>
    /// Interaction logic for MultiplayerConnectWindow.xaml
    /// </summary>
    public sealed partial class MultiplayerConnectWindow : Window, INotifyPropertyChanged
    {
        private readonly MultiplayerClientService _multiplayerClientService;
        private readonly ILogger _logger;

        public MultiplayerConnectWindow(MultiplayerClientService multiplayerClientService, ILogger<MultiplayerConnectWindow> logger)
        {
            _logger = logger;
            _multiplayerClientService = multiplayerClientService;
            InitializeComponent();
            DataContext = this;

            _logger.LogInformation("Opening window");
            _multiplayerClientService.Connected += MultiplayerClientServiceConnected;
            _multiplayerClientService.Error += MultiplayerClientServiceError;
            _multiplayerClientService.GameCreated += MultiplayerClientServiceGameCreated;
            _multiplayerClientService.GameJoined += MultiplayerClientServiceGameJoined;
        }

        private void MultiplayerClientServiceError(string error, Exception? exception)
        {
            if (Dispatcher.CheckAccess())
            {
                MessageBox.Show(this, error, "SMZ3 Cas' Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
                if (IsConnecting)
                {
                    IsConnecting = false;
                    OnPropertyChanged();
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    MultiplayerClientServiceError(error, exception);
                });
            }
        }

        private async void MultiplayerClientServiceConnected()
        {
            if (IsCreatingGame)
            {
                _logger.LogInformation("Connecting");
                await _multiplayerClientService.CreateGame(PlayerNameTextBox.Text);
            }
            else
            {
                _logger.LogInformation("Joining");
                await _multiplayerClientService.JoinGame(GameGuid, PlayerNameTextBox.Text);
            }
        }

        private void MultiplayerClientServiceGameCreated(string gameGuid, string playerGuid, string playerKey)
        {
            SaveAndClose(gameGuid, playerGuid, playerKey);
        }

        private void MultiplayerClientServiceGameJoined(string playerGuid, string playerKey)
        {
            SaveAndClose(_multiplayerClientService.CurrentGameGuid!, playerGuid, playerKey);
        }

        private void SaveAndClose(string gameGuid, string playerGuid, string playerKey)
        {
            DialogResult = true;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _multiplayerClientService.Connected -= MultiplayerClientServiceConnected;
            _multiplayerClientService.Error -= MultiplayerClientServiceError;
            _multiplayerClientService.GameJoined -= MultiplayerClientServiceGameJoined;
            _multiplayerClientService.GameCreated -= MultiplayerClientServiceGameCreated;
        }

        public bool IsCreatingGame { get; set; }
        public bool IsJoiningGame { get; set; }
        public bool IsConnecting { get; set; }
        public string UrlLabelText => IsCreatingGame ? "Server url:" : "Game url:";
        public bool CanEnterInput => !IsConnecting;
        public bool CanEnterGameMode => !IsConnecting && IsCreatingGame;
        public bool CanPressButton => PlayerNameTextBox.Text.Length > 0;
        public string StatusText => IsConnecting ? "Connecting..." : "";

        public string ServerUrl => ServerUrlTextBox.Text.Contains('?')
            ? ServerUrlTextBox.Text[..ServerUrlTextBox.Text.IndexOf("?", StringComparison.Ordinal)]
            : ServerUrlTextBox.Text;

        public string GameGuid => ServerUrlTextBox.Text.Contains('?')
            ? ServerUrlTextBox.Text[(ServerUrlTextBox.Text.IndexOf("=", StringComparison.Ordinal) + 1)..]
            : "";

        public string GameButtonText
        {
            get
            {
                if (IsConnecting) return "Cancel Connecting";
                return IsJoiningGame? "Join Game" : "Create Game";
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _multiplayerClientService.Error -= MultiplayerClientServiceError;
            await _multiplayerClientService.Disconnect();
            IsConnecting = false;
            Close();
        }

        private async void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsConnecting)
            {
                await _multiplayerClientService.Disconnect();
                IsConnecting = false;
                OnPropertyChanged();
            }
            else
            {
                IsConnecting = true;
                OnPropertyChanged();
                await _multiplayerClientService.Connect(ServerUrl);
            }
        }

        private void ServerUrlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            IsJoiningGame = ServerUrlTextBox.Text.Contains("?game=");
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PlayerNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OnPropertyChanged();
        }
    }
}
