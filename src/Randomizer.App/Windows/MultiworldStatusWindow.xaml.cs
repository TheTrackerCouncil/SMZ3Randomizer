using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Randomizer.App.Controls;
using Randomizer.App.ViewModels;
using Randomizer.Data.Multiworld;
using Randomizer.Multiworld.Client;

namespace Randomizer.App.Windows
{
    /// <summary>
    /// Interaction logic for MultiworldStatusWindow.xaml
    /// </summary>
    public partial class MultiworldStatusWindow : Window
    {
        private readonly MultiworldClientService _multiworldClientService;
        private bool _openedConfig;

        public MultiworldStatusWindow(MultiworldClientService multiworldClientService)
        {
            _multiworldClientService = multiworldClientService;
            DataContext = Model;
            InitializeComponent();
            UpdatePlayerList();

            if (!_multiworldClientService.IsConnected)
            {
                Close();
            }

            Model.IsConnected = true;
            Model.GameUrl = _multiworldClientService.GameUrl ?? "";

            _multiworldClientService.Error += MultiworldClientServiceError;
            _multiworldClientService.GameRejoined += MultiworldClientServiceOnGameRejoined;
            _multiworldClientService.PlayerJoined += MultiworldClientServiceOnPlayerJoined;
            _multiworldClientService.PlayerRejoined += MultiworldClientServiceOnPlayerRejoined;
            _multiworldClientService.PlayerSync += MultiworldClientServiceOnPlayerSync;
            _multiworldClientService.Connected += MultiworldClientServiceOnConnected;
            _multiworldClientService.ConnectionClosed += MultiworldClientServiceOnConnectionClosed;
        }

        private void MultiworldClientServiceOnPlayerRejoined()
        {
            UpdatePlayerList();
        }

        private void MultiworldClientServiceOnGameRejoined()
        {
            UpdatePlayerList();
            Model.IsConnected = true;
        }

        private void MultiworldClientServiceOnConnected()
        {
            Task.Run(async () => await _multiworldClientService.RejoinGame());
        }

        private void MultiworldClientServiceOnConnectionClosed(Exception? exception)
        {
            Model.IsConnected = false;

        }

        protected override void OnActivated(EventArgs e)
        {
            if (_multiworldClientService.LocalPlayer?.Config == null && !_openedConfig)
            {
                ShowGenerateRomWindow();
                _openedConfig = true;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _multiworldClientService.Error -= MultiworldClientServiceError;
        }

        private void MultiworldClientServiceError(string error, Exception? exception)
        {
            if (Dispatcher.CheckAccess())
            {
                MessageBox.Show(this, error , "SMZ3 Cas' Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    MultiworldClientServiceError(error, exception);
                });
            }
        }

        private void MultiworldClientServiceOnPlayerJoined()
        {
            UpdatePlayerList();
        }

        private void MultiworldClientServiceOnPlayerSync(MultiworldPlayerState? previousState, MultiworldPlayerState state)
        {
            Model.UpdatePlayer(state);
        }

        public MultiworldStatusViewModel Model { get; set; } = new();
        public MultiRomListPanel ParentPanel { get; set; } = null!;

        protected void UpdatePlayerList()
        {
            Model.UpdateList(_multiworldClientService.Players ?? new List<MultiworldPlayerState>(), _multiworldClientService.LocalPlayer);
        }

        protected void ShowGenerateRomWindow()
        {
            if (ParentPanel.ShowGenerateRomWindow(null, true) != true) return;
            Task.Run(async () => await _multiworldClientService.SubmitConfig(ParentPanel.Options.ToConfig()));

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
            await _multiworldClientService.Reconnect();
        }
    }
}
