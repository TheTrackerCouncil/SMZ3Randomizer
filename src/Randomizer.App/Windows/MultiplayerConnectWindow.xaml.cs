using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using Randomizer.Multiplayer.Client;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.App.Windows
{
    /// <summary>
    /// Interaction logic for MultiplayerConnectWindow.xaml
    /// Window for creating a new multiplayer game or connecting to a previously started one
    /// </summary>
    public sealed partial class MultiplayerConnectWindow : Window, INotifyPropertyChanged
    {
        private static readonly Regex s_illegalCharacters = new(@"[^A-Z0-9\-]", RegexOptions.IgnoreCase);
        private readonly MultiplayerClientService _multiplayerClientService;
        private readonly ILogger _logger;
        private readonly string _version;

        public MultiplayerConnectWindow(MultiplayerClientService multiplayerClientService, ILogger<MultiplayerConnectWindow> logger)
        {
            _logger = logger;
            _multiplayerClientService = multiplayerClientService;
            InitializeComponent();
            DataContext = this;

            _version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? "";

            _multiplayerClientService.Connected += MultiplayerClientServiceConnected;
            _multiplayerClientService.Error += MultiplayerClientServiceError;
            _multiplayerClientService.GameCreated += MultiplayerClientServiceGameJoined;
            _multiplayerClientService.GameJoined += MultiplayerClientServiceGameJoined;
        }

        private void MultiplayerClientServiceError(string error, Exception? exception)
        {
            DisplayError(error);
        }

        private async void MultiplayerClientServiceConnected()
        {
            if (IsCreatingGame)
            {
                _logger.LogInformation("Connecting");
                await _multiplayerClientService.CreateGame(PlayerNameTextBox.Text, MultiplayerGameType, _version);
            }
            else
            {
                _logger.LogInformation("Joining");
                await _multiplayerClientService.JoinGame(GameGuid, PlayerNameTextBox.Text, _version);
            }
        }

        private void MultiplayerClientServiceGameJoined()
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
            _multiplayerClientService.GameCreated -= MultiplayerClientServiceGameJoined;
        }

        public bool IsCreatingGame { get; set; }
        public bool IsJoiningGame { get; set; }
        public bool IsConnecting { get; set; }
        public string UrlLabelText => IsCreatingGame ? "Server url:" : "Game url:";
        public bool CanEnterInput => !IsConnecting;
        public bool CanEnterGameMode => !IsConnecting && IsCreatingGame;
        public bool CanPressButton => PlayerNameTextBox.Text.Length > 0;
        public string StatusText => IsConnecting ? "Connecting..." : "";
        public MultiplayerGameType MultiplayerGameType { get; set; }

        public string ServerUrl => ServerUrlTextBox.Text.SubstringBeforeCharacter('?') ?? ServerUrlTextBox.Text;
        public string GameGuid => ServerUrlTextBox.Text.SubstringAfterCharacter('=') ?? "";

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
            if (s_illegalCharacters.IsMatch(PlayerNameTextBox.Text))
            {
                DisplayError("Player names can only contains letters, numbers, hyphens, and underscores.");
                return;
            }

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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PlayerNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OnPropertyChanged();
        }

        private void DisplayError(string message)
        {
            if (Dispatcher.CheckAccess())
            {
                MessageBox.Show(this, message, "SMZ3 Cas' Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    DisplayError(message);
                });
            }
        }
    }
}
