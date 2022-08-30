using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Randomizer.App.ViewModels;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for TrackerLocationsWindow.xaml
    /// </summary>
    public partial class TrackerLocationsWindow : Window
    {
        public TrackerLocationsWindow(TrackerLocationSyncer syncer, IUIService uiService)
        {
            DataContext = new TrackerViewModel(syncer, uiService);

            InitializeComponent();

            ChestSprite = new BitmapImage(new Uri(uiService.GetSpritePath("Items", "chest.png", out _)));

            KeySprite = new BitmapImage(new Uri(uiService.GetSpritePath("Items", "key.png", out _)));

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
