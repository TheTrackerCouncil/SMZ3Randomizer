using System;
using System.IO;
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
        }

        public Tracker Tracker { get; }

        public ImageSource ChestSprite { get; }

        private void Tracker_StateLoaded(object sender, EventArgs e)
        {
            DataContext = new TrackerViewModel(Tracker);
        }
    }
}
