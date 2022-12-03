using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.App.ViewModels;
using Randomizer.App.Windows;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.App.Controls
{
    /// <summary>
    /// Interaction logic for MultiRomListPanel.xaml
    /// </summary>
    public partial class MultiRomListPanel : RomListPanel
    {
        public MultiRomListPanel(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            ILogger<MultiRomListPanel> logger,
            RandomizerContext dbContext,
            RomGenerator romGenerator) : base(serviceProvider, optionsFactory, logger, dbContext, romGenerator)
        {
            Model = new GeneratedRomsViewModel();
            DataContext = Model;
            InitializeComponent();
            UpdateList();
        }

        public GeneratedRomsViewModel Model { get; }

        protected sealed override void UpdateList()
        {

        }

        private void CreateMultiGameButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = ServiceProvider.CreateScope();
            var multiWindow = scope.ServiceProvider.GetRequiredService<MultiplayerConnectWindow>();
            multiWindow.Owner = Window.GetWindow(this);
            multiWindow.IsCreatingGame = true;
            if (multiWindow.ShowDialog() == true) NewMultiGame();
        }

        private void JoinMultiGameButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = ServiceProvider.CreateScope();
            var multiWindow = scope.ServiceProvider.GetRequiredService<MultiplayerConnectWindow>();
            multiWindow.Owner = Window.GetWindow(this);
            multiWindow.IsJoiningGame = true;
            if (multiWindow.ShowDialog() == true) NewMultiGame();
        }

        private void NewMultiGame()
        {
            OpenStatusWindow();
        }

        private void OpenStatusWindow()
        {
            using var scope = ServiceProvider.CreateScope();
            var statusWindow = scope.ServiceProvider.GetRequiredService<MultiplayerStatusWindow>();
            statusWindow.Owner = Window.GetWindow(this);
            statusWindow.ParentPanel = this;
            statusWindow.Show();
        }


    }
}
