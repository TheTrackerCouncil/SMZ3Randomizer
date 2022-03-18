using System;
using System.Windows;

using Microsoft.Extensions.Logging;

using Randomizer.App.ViewModels;
using Randomizer.SMZ3.ChatIntegration;

using SharpYaml.Tokens;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private readonly IChatAuthenticationService _chatAuthenticationService;
        private readonly ILogger<OptionsWindow> _logger;
        private GeneralOptions _options;

        public OptionsWindow(IChatAuthenticationService chatAuthenticationService,
            ILogger<OptionsWindow> logger)
        {
            InitializeComponent();

            _chatAuthenticationService = chatAuthenticationService;
            _logger = logger;
        }

        public GeneralOptions Options
        {
            get => _options;
            set
            {
                if (value != _options)
                {
                    _options = value;
                    DataContext = value;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private async void TwitchLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.Invoke(async () =>
            {
                try
                {
                    var token = await _chatAuthenticationService.GetTokenInteractivelyAsync(default);
                    var userName = await _chatAuthenticationService.GetUserNameAsync(token, default);

                    Options.TwitchUserName = userName;
                    Options.TwitchOAuthToken = token;
                    Options.TwitchChannel = string.IsNullOrEmpty(Options.TwitchChannel) ? userName : Options.TwitchChannel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unknown error occurred while logging in with Twitch");
                    MessageBox.Show(this, "An unexpected error occurred while trying to log you in with Twitch. " +
                        "Please try again or report this issue with the log file.", "SMZ3 Cas’ Randomizer",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
    }
}
