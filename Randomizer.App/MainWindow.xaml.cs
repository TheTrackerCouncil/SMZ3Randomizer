using System;
using System.IO;
using System.Threading;
using System.Windows;

using Randomizer.App.ViewModels;
using Randomizer.SMZ3;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new RandomizerOptions(this);
        }

        protected RandomizerOptions Options => DataContext as RandomizerOptions;

        private void GenerateRomButton_Click(object sender, RoutedEventArgs e)
        {
            var randomizer = new SMZ3.Randomizer();
            var options = Options.ToDictionary();
            var seed = randomizer.GenerateSeed(options, Options.Seed, CancellationToken.None);
        }
    }
}