using System.Windows;

using Randomizer.App.ViewModels;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    [NotAService]
    public partial class OptionsWindow : Window
    {
        public OptionsWindow(GeneralOptions options)
        {
            InitializeComponent();

            Options = options;
            DataContext = Options;
        }

        public GeneralOptions Options { get; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
