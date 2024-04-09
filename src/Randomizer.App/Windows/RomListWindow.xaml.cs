using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using GitHubReleaseChecker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.App.Controls;
using Randomizer.App.ViewModels;
using Randomizer.Data.Options;
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
        private readonly IChatAuthenticationService _chatAuthenticationService;
        private readonly IGitHubReleaseCheckerService _gitHubReleaseCheckerService;
        private string _gitHubReleaseUrl = "";

        public RomListWindow(IServiceProvider serviceProvider,
            OptionsFactory optionsFactory,
            ILogger<RomListWindow> logger,
            IChatAuthenticationService chatAuthenticationService,
            IGitHubReleaseCheckerService gitHubReleaseCheckerService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            InitializeComponent();
            Options = optionsFactory.Create();
            _chatAuthenticationService = chatAuthenticationService;
            _gitHubReleaseCheckerService = gitHubReleaseCheckerService;

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

            _ = CheckForUpdates();
        }

        private async Task CheckForUpdates()
        {
            if (!Options.GeneralOptions.CheckForUpdatesOnStartup) return;

            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            try
            {
                var gitHubRelease = await _gitHubReleaseCheckerService
                    .GetGitHubReleaseToUpdateToAsync("TheTrackerCouncil", "SMZ3Randomizer", version ?? "", false);

                if (gitHubRelease != null)
                {
                    if (gitHubRelease.Url != Options.GeneralOptions.IgnoredUpdateUrl)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            UpdateNotificationBorder.Visibility = Visibility.Visible;
                            _gitHubReleaseUrl = gitHubRelease.Url;
                        });
                    }
                }
                else
                {
                    gitHubRelease = await _gitHubReleaseCheckerService
                        .GetGitHubReleaseToUpdateToAsync("Vivelin", "SMZ3Randomizer", version ?? "", false);

                    if (gitHubRelease != null && gitHubRelease.Url != Options.GeneralOptions.IgnoredUpdateUrl)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            UpdateNotificationBorder.Visibility = Visibility.Visible;
                            _gitHubReleaseUrl = gitHubRelease.Url;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GitHub release");
            }
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
            optionsDialog.ShowDialog();

            if (!string.IsNullOrEmpty(Options.GeneralOptions.RomOutputPath) && !Directory.Exists(Options.GeneralOptions.RomOutputPath))
            {
                try
                {
                    Directory.CreateDirectory(Options.GeneralOptions.RomOutputPath);
                }
                catch
                {
                    // Oh well, next check will warn the user
                }
            }

            if (!Options.GeneralOptions.Validate())
            {
                MessageBox.Show(this, "Missing required settings. Verify that you have entered valid " +
                                      "roms and a valid output folder.",
                "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
            }


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

        private void UpdateNotificationHyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _gitHubReleaseUrl,
                UseShellExecute = true
            });
        }

        private void CloseUpdateNotificationButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateNotificationBorder.Visibility = Visibility.Collapsed;
        }

        private void IgnoreVersionHyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Options.GeneralOptions.IgnoredUpdateUrl = _gitHubReleaseUrl;
            Options.Save();
            UpdateNotificationBorder.Visibility = Visibility.Collapsed;
        }

        private void StopUpdateCheckHyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Options.GeneralOptions.CheckForUpdatesOnStartup = false;
            Options.Save();
            UpdateNotificationBorder.Visibility = Visibility.Collapsed;
        }
    }
}
