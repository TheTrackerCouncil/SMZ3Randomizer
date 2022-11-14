using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using Randomizer.Multiworld.Client;

namespace Randomizer.App.Windows
{
    /// <summary>
    /// Interaction logic for MultiworldConnectWindow.xaml
    /// </summary>
    public partial class MultiworldConnectWindow : Window, INotifyPropertyChanged
    {
        private readonly MultiworldClientService _multiworldClientService;
        private readonly ILogger _logger;

        public MultiworldConnectWindow(MultiworldClientService multiworldClientService, ILogger<MultiworldConnectWindow> logger)
        {
            _logger = logger;
            _multiworldClientService = multiworldClientService;
            InitializeComponent();
            DataContext = this;

            _logger.LogInformation("Opening window");
            _multiworldClientService.Connected += MultiworldClientServiceConnected;
            _multiworldClientService.Error += MultiworldClientServiceError;
            _multiworldClientService.GameCreated += MultiworldClientServiceGameCreated;
            _multiworldClientService.GameJoined += MultiworldClientServiceGameJoined;
        }

        private void MultiworldClientServiceError(string error, Exception? exception)
        {
            MessageBox.Show(this, error, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            if (IsConnecting)
            {
                IsConnecting = false;
                OnPropertyChanged();
            }
        }

        private async void MultiworldClientServiceConnected()
        {
            if (IsCreatingGame)
            {
                _logger.LogInformation("Connecting");
                await _multiworldClientService.CreateGame(PlayerNameTextBox.Text);
            }
            else
            {
                _logger.LogInformation("Joining");
                await _multiworldClientService.JoinGame(GameGuid, PlayerNameTextBox.Text);
            }
        }

        private void MultiworldClientServiceGameCreated(string gameGuid, string playerGuid, string playerKey)
        {
            SaveAndClose(gameGuid, playerGuid, playerKey);
        }

        private void MultiworldClientServiceGameJoined(string playerGuid, string playerKey)
        {
            SaveAndClose(_multiworldClientService.CurrentGameGuid!, playerGuid, playerKey);
        }

        private void SaveAndClose(string gameGuid, string playerGuid, string playerKey)
        {
            DialogResult = true;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _multiworldClientService.Connected -= MultiworldClientServiceConnected;
            _multiworldClientService.Error -= MultiworldClientServiceError;
            _multiworldClientService.GameJoined -= MultiworldClientServiceGameJoined;
            _multiworldClientService.GameCreated -= MultiworldClientServiceGameCreated;
        }

        public bool IsCreatingGame { get; set; }
        public bool IsJoiningGame { get; set; }
        public bool IsConnecting { get; set; }
        public string UrlLabelText => IsCreatingGame ? "Server url" : "Game url";
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
            _multiworldClientService.Error -= MultiworldClientServiceError;
            await _multiworldClientService.Disconnect();
            IsConnecting = false;
            Close();
        }

        private async void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsConnecting)
            {
                await _multiworldClientService.Disconnect();
                IsConnecting = false;
                OnPropertyChanged();
            }
            else
            {
                IsConnecting = true;
                OnPropertyChanged();
                await _multiworldClientService.Connect(ServerUrl);
            }
        }

        private void ServerUrlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            IsJoiningGame = ServerUrlTextBox.Text.Contains("?game=");
            OnPropertyChanged();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PlayerNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OnPropertyChanged();
        }
    }
}
