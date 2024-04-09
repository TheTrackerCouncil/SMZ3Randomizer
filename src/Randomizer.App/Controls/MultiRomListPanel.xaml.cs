using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.App.ViewModels;
using Randomizer.App.Windows;
using Randomizer.Data.Interfaces;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Infrastructure;

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
            IRomGenerationService romGenerationService,
            IGameDbService gameDbService,
            RomLauncherService romLauncherService) : base(serviceProvider, optionsFactory, logger, romGenerationService, gameDbService, romLauncherService)
        {
            Model = new MultiplayerGamesViewModel();
            DataContext = Model;
            InitializeComponent();
            UpdateList();
        }

        public MultiplayerGamesViewModel Model { get; }

        public sealed override void UpdateList()
        {
            var models = GameDbService.GetMultiplayerGamesList()
                .OrderByDescending(x => x.Id)
                .ToList();
            Model.UpdateList(models);
        }

        private void CreateMultiGameButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = ServiceProvider.CreateScope();
            var multiWindow = scope.ServiceProvider.GetRequiredService<MultiplayerConnectWindow>();
            multiWindow.Owner = Window.GetWindow(this);
            multiWindow.IsCreatingGame = true;
            multiWindow.Options = Options;
            if (multiWindow.ShowDialog() == true) OpenStatusWindow(null);
            UpdateList();
        }

        private void JoinMultiGameButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = ServiceProvider.CreateScope();
            var multiWindow = scope.ServiceProvider.GetRequiredService<MultiplayerConnectWindow>();
            multiWindow.Owner = Window.GetWindow(this);
            multiWindow.IsJoiningGame = true;
            if (multiWindow.ShowDialog() == true) OpenStatusWindow(null);
            UpdateList();
        }

        /// <summary>
        /// Opens the multiplayer status window
        /// </summary>
        /// <param name="game"></param>
        private void OpenStatusWindow(MultiplayerGameDetails? game)
        {
            using var scope = ServiceProvider.CreateScope();
            var statusWindow = scope.ServiceProvider.GetRequiredService<MultiplayerStatusWindow>();
            statusWindow.Owner = Window.GetWindow(this);
            statusWindow.ParentPanel = this;
            statusWindow.MultiplayerGameDetails = game;
            statusWindow.Show();
        }

        /// <summary>
        /// Menu item for deleting a multiplayer game and rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void DeleteRomMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: MultiplayerGameDetails details })
                return;

            if (!GameDbService.DeleteMultiplayerGame(details, out var error))
            {
                ShowErrorMessageBox(error);
            }

            UpdateList();
        }

        /// <summary>
        /// The user has clicked on a quick launch button for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: MultiplayerGameDetails details })
                return;

            OpenStatusWindow(details);
        }

        /// <summary>
        /// Right click menu to open the folder for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: MultiplayerGameDetails details } || details.GeneratedRom == null)
                return;

            OpenFolder(details.GeneratedRom);
        }

        /// <summary>
        /// Menu item for viewing the spoiler log for a rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewSpoilerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: MultiplayerGameDetails details } || details.GeneratedRom == null)
                return;

            OpenSpoilerLog(details.GeneratedRom);
        }

        private void ProgressionLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { Tag: MultiplayerGameDetails details } || details.GeneratedRom == null)
                return;

            OpenProgressionLog(details.GeneratedRom);
        }
    }
}
