using System.Windows;

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
        private GeneralOptions _options;

        public OptionsWindow(IChatAuthenticationService chatAuthenticationService)
        {
            InitializeComponent();

            _chatAuthenticationService = chatAuthenticationService;
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
                var token = await _chatAuthenticationService.GetTokenInteractivelyAsync(default);
                var userName = await _chatAuthenticationService.GetUserNameAsync(token, default);

                Options.TwitchUserName = userName;
                Options.TwitchOAuthToken = token;
                Options.TwitchChannel = string.IsNullOrEmpty(Options.TwitchChannel) ? userName : Options.TwitchChannel;
            });
        }
    }
}
