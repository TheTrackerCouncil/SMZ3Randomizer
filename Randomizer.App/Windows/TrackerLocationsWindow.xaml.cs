using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Randomizer.App.ViewModels;
using Randomizer.SMZ3.Tracking;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for TrackerLocationsWindow.xaml
    /// </summary>
    public partial class TrackerLocationsWindow : Window
    {
        public TrackerLocationsWindow(TrackerLocationSyncer syncer)
        {
            DataContext = new TrackerViewModel(syncer);

            InitializeComponent();

            ChestSprite = new BitmapImage(new Uri(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Sprites", "Items", "chest.png")));

            KeySprite = new BitmapImage(new Uri(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Sprites", "Items", "key.png")));

            App.RestoreWindowPositionAndSize(this);
        }

        public ImageSource ChestSprite { get; }

        public ImageSource KeySprite { get; }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.SaveWindowPositionAndSize(this);
        }
    }
}
