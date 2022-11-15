using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Randomizer.App.Controls;
using Randomizer.App.ViewModels;
using Randomizer.Data.Multiworld;
using Randomizer.Data.Options;
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

            _multiworldClientService.Error += MultiworldClientServiceError;
            _multiworldClientService.PlayerJoined += MultiworldClientServiceOnPlayerJoined;
            _multiworldClientService.PlayerSync += MultiworldClientServiceOnPlayerSync;
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
            MessageBox.Show(this, error, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MultiworldClientServiceOnPlayerJoined()
        {
            UpdatePlayerList();
        }

        private void MultiworldClientServiceOnPlayerSync(MultiworldPlayerState? previousState, MultiworldPlayerState state)
        {
            Model.UpdatePlayer(state);
        }

        public MultiworldPlayersViewModel Model { get; set; } = new();
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
    }
}
