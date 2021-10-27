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
        public TrackerLocationsWindow(Tracker tracker)
        {
            Tracker = tracker;
            Tracker.StateLoaded += Tracker_StateLoaded;
            DataContext = new TrackerViewModel(tracker);

            InitializeComponent();

            ChestSprite = new BitmapImage(new Uri(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Sprites", "Items", "chest.png")));

            KeySprite = new BitmapImage(new Uri(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Sprites", "Items", "key.png")));
        }

        public Tracker Tracker { get; }

        public ImageSource ChestSprite { get; }

        public ImageSource KeySprite { get; }

        private void Tracker_StateLoaded(object sender, EventArgs e)
        {
            DataContext = new TrackerViewModel(Tracker);

            // If we loaded a state, we need to resetup the map window to point to the latest tracker view model
            if (Application.Current.Windows.OfType<TrackerMapWindow>().Any())
            {
                TrackerMapWindow trackerMapWindow = Application.Current.Windows.OfType<TrackerMapWindow>().First();
                trackerMapWindow.SetupTrackerViewModel((TrackerViewModel)DataContext);
                trackerMapWindow.TrackerMapViewModel.OnPropertyChanged();
            }
        }
    }
}
