using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Options;
using Randomizer.Multiplayer.Client;
using Randomizer.Shared;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.App.Windows
{
    /// <summary>
    /// Interaction logic for MultiplayerConnectWindow.xaml
    /// Window for creating a new multiplayer game or connecting to a previously started one
    /// </summary>
    public sealed partial class MultiplayerConnectWindow : Window, INotifyPropertyChanged
    {
        private static readonly Regex s_illegalCharacters = new(@"[^A-Z0-9\-]", RegexOptions.IgnoreCase);

        private static readonly List<string> s_defaultServers = new()
        {
            "https://smz3.celestialrealm.net",
#if DEBUG
            "http://192.168.50.100:5000",
            "http://localhost:5000"
#endif
        };

        private readonly MultiplayerClientService _multiplayerClientService;
        private readonly ILogger _logger;
        private readonly string _version;
        private readonly ICommunicator _communicator;

        public MultiplayerConnectWindow(MultiplayerClientService multiplayerClientService, ILogger<MultiplayerConnectWindow> logger, ICommunicator communicator)
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

            _communicator = communicator;
        }

        private void MultiplayerClientServiceError(string error, Exception? exception)
        {
            DisplayError(error);
        }

        private async void MultiplayerClientServiceConnected()
        {
            if (IsCreatingGame)
            {
                _logger.LogInformation("Connected to server successfully. Creating new game.");
                if (Options != null) Options.MultiplayerUrl = Url;
                await _multiplayerClientService.CreateGame(DisplayName, PhoneticName, MultiplayerGameType, _version, AsyncGame, SendItemsOnComplete);
            }
            else
            {
                _logger.LogInformation("Connected to Server successfully. Joining game.");
                await _multiplayerClientService.JoinGame(GameGuid, DisplayName, PhoneticName, _version);
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
        public string UrlLabelText => IsCreatingGame ? "Server Url:" : "Game Url:";
        public bool CanEnterInput => !IsConnecting;
        public bool CanEnterGameMode => !IsConnecting && IsCreatingGame;
        public bool CanPressButton => PlayerNameTextBox.Text.Length > 0 && Url.Length > 0;
        public string StatusText => IsConnecting ? "Connecting..." : "";
        public RandomizerOptions? Options { get; set; }
        public Visibility ServerListVisibility => IsCreatingGame ? Visibility.Visible : Visibility.Collapsed;
        public bool AsyncGame { get; set; }
        public bool SendItemsOnComplete { get; set; }

        private string _url = "";
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                OnPropertyChanged(nameof(Url));
                OnPropertyChanged(nameof(CanPressButton));
            }
        }
        public string DisplayName => PlayerNameTextBox.Text;
        public string PhoneticName { get; set; } = "";
        public MultiplayerGameType MultiplayerGameType { get; set; }

        public string ServerUrl => Url.SubstringBeforeCharacter('?') ?? ServerUrlTextBox.Text;
        public string GameGuid => Url.SubstringAfterCharacter('=') ?? "";

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

        private void MultiplayerConnectWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsCreatingGame)
            {
                if (!string.IsNullOrEmpty(Options?.MultiplayerUrl))
                    Url = Options.MultiplayerUrl;
                else
                    Url = s_defaultServers.First();

                if (ServerListButton.ContextMenu != null)
                {
                    foreach (var url in s_defaultServers)
                    {
                        var menuItem = new MenuItem { Header = url };
                        menuItem.Click += ServerMenuItemClick;
                        ServerListButton.ContextMenu.Items.Add(menuItem);
                    }

                }
                OnPropertyChanged(nameof(Url));
                OnPropertyChanged(nameof(CanPressButton));
            }

#if DEBUG
            // Added this so that we can use this box for easily testing how tracker messages sound
            PhoneticNameTextBox.MaxLength = 1000;
#endif
        }

        private void ServerMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem) return;
            Url = menuItem.Header as string ?? s_defaultServers.First();
            OnPropertyChanged(nameof(Url));
        }

        private void PhoneticNameTestButton_OnClick(object sender, RoutedEventArgs e)
        {
            _communicator.Say(string.IsNullOrEmpty(PhoneticName) ? DisplayName : PhoneticName);
        }

        private void ServerListButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ServerListButton.ContextMenu == null) return;
            ServerListButton.ContextMenu.DataContext = ServerListButton.DataContext;
            ServerListButton.ContextMenu.IsOpen = true;
        }
    }
}
