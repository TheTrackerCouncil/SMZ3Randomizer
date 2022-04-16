using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

using Randomizer.App.ViewModels;
using Randomizer.SMZ3.Msu;

namespace Randomizer.App.Windows
{
    /// <summary>
    /// Interaction logic for MusicPackManagerWindow.xaml
    /// </summary>
    [NotAService]
    public partial class MusicPackManagerWindow : Window
    {
        private readonly MusicPackFactory _musicPackFactory;

        public MusicPackManagerWindow(Window owner, RandomizerOptions options, MusicPackFactory musicPackFactory)
        {
            InitializeComponent();
            MsuPath.Text = options.GeneralOptions.MsuPath;

            Owner = owner;
            Options = options;
            _musicPackFactory = musicPackFactory;
        }

        public RandomizerOptions Options { get; }

        public ObservableCollection<MusicPack> MusicPacks { get; } = new();

        private async void AutoDetectButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var msuFile in Directory.EnumerateFiles(Options.GeneralOptions.MsuPath, "*.msu", SearchOption.AllDirectories))
            {
                var pack = _musicPackFactory.AutoDetect(msuFile);
                MusicPacks.Add(pack);

                var packDir = Path.GetDirectoryName(msuFile);
                var packFile = Path.Combine(packDir, $"{Path.GetFileNameWithoutExtension(msuFile)}.msu.yml");
                if (!File.Exists(packFile))
                {
                    pack.FileName = packFile;
                    await _musicPackFactory.SaveAsync(pack, packFile);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
