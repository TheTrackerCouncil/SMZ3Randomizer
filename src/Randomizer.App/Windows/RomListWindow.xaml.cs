using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.App.Controls;
using Randomizer.App.ViewModels;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.ChatIntegration;

namespace Randomizer.App.Windows
{
    /// <summary>
    /// Interaction logic for RomListWindow.xaml
    /// </summary>
    public partial class RomListWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RomListWindow> _logger;
        private readonly RandomizerContext _dbContext;
        private readonly RomGenerator _romGenerator;
        private readonly IChatAuthenticationService _chatAuthenticationService;

        public RomListWindow(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            ILogger<RomListWindow> logger,
            RandomizerContext dbContext,
            RomGenerator romGenerator,
            IChatAuthenticationService chatAuthenticationService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dbContext = dbContext;
            _romGenerator = romGenerator;
            InitializeComponent();
            Options = optionsFactory.Create();
            _chatAuthenticationService = chatAuthenticationService;

            App.RestoreWindowPositionAndSize(this);
        }

        public GeneratedRomsViewModel Model { get; } = new();
        public RandomizerOptions Options { get; }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Options.GeneralOptions.Validate())
            {
                MessageBox.Show(this, "If this is your first time using the randomizer," +
                    " there are some required options you need to configure before you " +
                    "can start playing randomized SMZ3 games. Please do so now.",
                    "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Information);
                OptionsMenuItem_Click(this, new RoutedEventArgs());
                return;
            }

            if (!string.IsNullOrEmpty(Options.GeneralOptions.TwitchOAuthToken))
            {
                var isTokenValid = await _chatAuthenticationService.ValidateTokenAsync(Options.GeneralOptions.TwitchOAuthToken, default);
                if (!isTokenValid)
                {
                    Options.GeneralOptions.TwitchOAuthToken = string.Empty;
                    MessageBox.Show(this, "Your Twitch login has expired. Please" +
                        " go to Options and log in with Twitch again to re-enable" +
                        " chat integration features.", "SMZ3 Cas’ Randomizer",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            var soloPanel = _serviceProvider.GetService<SoloRomListPanel>();
            if (soloPanel != null)  SoloTab.Children.Add(soloPanel);
            var multiPanel = _serviceProvider.GetService<MultiRomListPanel>();
            if (multiPanel != null) MultiTab.Children.Add(multiPanel);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                Options.Save(OptionsFactory.GetFilePath());
                App.SaveWindowPositionAndSize(this);
            }
            catch
            {
                // Oh well
            }
        }

        private void OptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();
            var optionsDialog = scope.ServiceProvider.GetRequiredService<OptionsWindow>();
            optionsDialog.Options = Options.GeneralOptions;
            optionsDialog.ShowDialog();

            try
            {
                Options.Save(OptionsFactory.GetFilePath());
            }
            catch
            {
                // Oh well
            }

        }

        /// <summary>
        /// Launches the about menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();
            var aboutWindow = scope.ServiceProvider.GetRequiredService<AboutWindow>();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

    }
}
