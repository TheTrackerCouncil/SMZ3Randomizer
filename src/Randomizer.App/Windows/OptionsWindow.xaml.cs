using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

using Randomizer.App.ViewModels;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.Data.Configuration;
using Randomizer.Data.Options;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window, INotifyPropertyChanged
    {
        private readonly IChatAuthenticationService _chatAuthenticationService;
        private readonly ILogger<OptionsWindow> _logger;
        private readonly ConfigProvider _trackerConfigProvider;
        private GeneralOptions _options;
        private bool _canLogIn = true;
        private ICollection<string> _availableProfiles;


        public OptionsWindow(IChatAuthenticationService chatAuthenticationService,
            ConfigProvider configProvider,
            ILogger<OptionsWindow> logger)
        {
            InitializeComponent();
            _trackerConfigProvider = configProvider;
            _chatAuthenticationService = chatAuthenticationService;
            _logger = logger;
            AvailableProfiles = configProvider.GetAvailableProfiles();
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
                    PropertyChanged?.Invoke(this, new(nameof(Options)));
                    PropertyChanged?.Invoke(this, new(nameof(EnabledProfiles)));
                    PropertyChanged?.Invoke(this, new(nameof(DisabledProfiles)));
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

        public bool IsValidToken => !string.IsNullOrEmpty(Options.TwitchOAuthToken);

        public ICollection<string> AvailableProfiles
        {
            get => _availableProfiles;
            set
            {
                _availableProfiles = value;
                PropertyChanged?.Invoke(this, new(nameof(DisabledProfiles)));
            }
        }

        public ICollection<string> EnabledProfiles =>
            Options?.SelectedProfiles.Where(x => !string.IsNullOrEmpty(x)).ToList();

        public ICollection<string> DisabledProfiles =>
            AvailableProfiles?.Where(x => Options?.SelectedProfiles?.Contains(x) == false)?.ToList();

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

            await ValidateTwitchOAuthToken();
        }

        private void EnableProfile_Click(object sender, RoutedEventArgs e)
        {
            var newProfile = DisabledProfilesListBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(newProfile))
            {
                Options.SelectedProfiles.Add(newProfile);
                Options.SelectedProfiles.Remove(null);
                PropertyChanged?.Invoke(this, new(nameof(EnabledProfiles)));
                PropertyChanged?.Invoke(this, new(nameof(DisabledProfiles)));
            }
        }

        private void DisableProfile_Click(object sender, RoutedEventArgs e)
        {
            Options.SelectedProfiles.Remove(EnabledProfilesListBox.SelectedItem as string);
            PropertyChanged?.Invoke(this, new(nameof(EnabledProfiles)));
            PropertyChanged?.Invoke(this, new(nameof(DisabledProfiles)));
        }

        private void MoveProfileUp_Click(object sender, RoutedEventArgs e)
        {
            var index = EnabledProfilesListBox.SelectedIndex;
            if (index <= 0) return;
            var value = EnabledProfilesListBox.SelectedItem as string;
            var profiles = Options.SelectedProfiles.ToList();
            profiles.Remove(value);
            profiles.Insert(index - 1, value);
            Options.SelectedProfiles = profiles;
            PropertyChanged?.Invoke(this, new(nameof(EnabledProfiles)));
            PropertyChanged?.Invoke(this, new(nameof(DisabledProfiles)));
        }

        private void MoveProfileDown_Click(object sender, RoutedEventArgs e)
        {
            var index = EnabledProfilesListBox.SelectedIndex;
            if (index >= Options.SelectedProfiles.Count - 1) return;
            var value = EnabledProfilesListBox.SelectedItem as string;
            var profiles = Options.SelectedProfiles.ToList();
            profiles.Remove(value);
            profiles.Insert(index + 1, value);
            Options.SelectedProfiles = profiles;
            PropertyChanged?.Invoke(this, new(nameof(EnabledProfiles)));
            PropertyChanged?.Invoke(this, new(nameof(DisabledProfiles)));
        }

        private void OpenProfilesFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(_trackerConfigProvider.ConfigDirectory, "Sassy");
            Process.Start("explorer.exe", $"/select,\"{path}\"");
        }

        private void RefreshProfilesButton_Click(object sender, RoutedEventArgs e)
        {
            AvailableProfiles = _trackerConfigProvider.GetAvailableProfiles();
            PropertyChanged?.Invoke(this, new(nameof(EnabledProfiles)));
            PropertyChanged?.Invoke(this, new(nameof(DisabledProfiles)));
        }

        private async void Self_Loaded(object sender, RoutedEventArgs e)
        {
            await ValidateTwitchOAuthToken();
        }

        private async Task ValidateTwitchOAuthToken()
        {
            if (string.IsNullOrEmpty(Options.TwitchOAuthToken))
            {
                TwitchLoginFeedback.Text = "";
                TwitchLoginButton.Visibility = Visibility.Visible;
                TwitchLogoutButton.Visibility = Visibility.Collapsed;
                return;
            }

            var isValid = await _chatAuthenticationService.ValidateTokenAsync(Options.TwitchOAuthToken, default);
            if (!isValid)
            {
                Options.TwitchOAuthToken = "";
                TwitchLoginFeedback.Text = "Login expired.";
                TwitchLoginButton.Visibility = Visibility.Visible;
                TwitchLogoutButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                TwitchLoginFeedback.Text = "Logged in.";
                TwitchLoginButton.Visibility = Visibility.Collapsed;
                TwitchLogoutButton.Visibility = Visibility.Visible;
            }
        }

        private async void TwitchLogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Options.TwitchOAuthToken))
                return;

            var revoked = await _chatAuthenticationService.RevokeTokenAsync(Options.TwitchOAuthToken, default);
            Options.TwitchOAuthToken = "";
            TwitchLoginFeedback.Text = revoked ? "Logged out." : "Something went wrong.";
            TwitchLoginButton.Visibility = Visibility.Visible;
            TwitchLogoutButton.Visibility = Visibility.Collapsed;
        }
    }
}
