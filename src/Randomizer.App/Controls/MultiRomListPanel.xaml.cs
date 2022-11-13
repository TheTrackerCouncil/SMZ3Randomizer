using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.App.ViewModels;
using Randomizer.App.Windows;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.ChatIntegration;

namespace Randomizer.App.Controls
{
    /// <summary>
    /// Interaction logic for MultiRomListPanel.xaml
    /// </summary>
    public partial class MultiRomListPanel : RomListPanel
    {
        public MultiRomListPanel(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            ILogger<RomListPanel> logger,
            RandomizerContext dbContext,
            RomGenerator romGenerator) : base(serviceProvider, optionsFactory, logger, dbContext, romGenerator)
        {
            Model = new GeneratedRomsViewModel();
            DataContext = Model;
            InitializeComponent();
            UpdateList();
        }

        public GeneratedRomsViewModel Model { get; }

        protected override void UpdateList()
        {

        }

        private void CreateMultiGameButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = ServiceProvider.CreateScope();
            var multiWindow = scope.ServiceProvider.GetRequiredService<MultiworldWindow>();
            multiWindow.Owner = Window.GetWindow(this);
            multiWindow.IsCreatingGame = true;
            multiWindow.ShowDialog();
        }

        private void JoinMultiGameButton_Click(object sender, RoutedEventArgs e)
        {
            using var scope = ServiceProvider.CreateScope();
            var multiWindow = scope.ServiceProvider.GetRequiredService<MultiworldWindow>();
            multiWindow.Owner = Window.GetWindow(this);
            multiWindow.IsJoiningGame = true;
            multiWindow.ShowDialog();
        }
    }
}
