using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

using Randomizer.App.ViewModels;
using Randomizer.SMZ3.ChatIntegration;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window, INotifyPropertyChanged
    {
        private readonly IChatAuthenticationService _chatAuthenticationService;
        private readonly ILogger<OptionsWindow> _logger;
        private GeneralOptions _options;
        private bool _canLogIn = true;

        public OptionsWindow(IChatAuthenticationService chatAuthenticationService,
            ILogger<OptionsWindow> logger)
        {
            InitializeComponent();

            _chatAuthenticationService = chatAuthenticationService;
            _logger = logger;
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public bool IsLoggingIn
        {
            get => _canLogIn;
            set
            {
                if (_canLogIn != value)
                {
                    _canLogIn = value;
                    PropertyChanged?.Invoke(this, new(nameof(IsLoggingIn)));
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private async void TwitchLoginButton_Click(object sender, RoutedEventArgs e)
        {
            IsLoggingIn = false;
            Cursor = Cursors.AppStarting;
            try
            {
                await Dispatcher.Invoke(async () =>
                {
                    try
                    {
                        var token = await _chatAuthenticationService.GetTokenInteractivelyAsync(default);
                        var userData = await _chatAuthenticationService.GetAuthenticatedUserDataAsync(token, default);

                        Options.TwitchUserName = userData.Name;
                        Options.TwitchOAuthToken = token;
                        Options.TwitchChannel = string.IsNullOrEmpty(Options.TwitchChannel) ? userData.Name : Options.TwitchChannel;
                        Options.TwitchId = userData.Id;

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
            finally
            {
                IsLoggingIn = true;
                Cursor = null;
            }
        }
    }
}
