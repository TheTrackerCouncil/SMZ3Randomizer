using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for TrackerWindow.xaml
    /// </summary>
    public partial class TrackerWindow : Window
    {
        private readonly World _world;
        private Tracker _tracker;

        public TrackerWindow(World world)
        {
            InitializeComponent();
            _world = world;
        }

        protected virtual void Log(string text)
        {
            Dispatcher.Invoke(() =>
            {
                StatusBarDebugOutput.Content = text;
            });
        }

        protected virtual void RefreshGridItems()
        {
            TrackerGrid.Children.Clear();
            foreach (var item in _tracker.Items.Where(x => x.Column != null && x.Row != null))
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Items", item.Image))),
                    Opacity = item.TrackingState > 0 ? 1.0d : 0.5d,
                    Tag = item
                };

                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                Grid.SetColumn(image, item.Column.Value);
                Grid.SetRow(image, item.Row.Value);
                TrackerGrid.Children.Add(image);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "tracker.json");
            var provider = new TrackerConfigProvider(path);
            _tracker = new Tracker(provider, _world);
            _tracker.ItemTracked += (sender, e) => Dispatcher.Invoke(() =>
            {
                StatusBarDebugOutput.Content = $"Confidence: {e.Confidence:P2}";
                RefreshGridItems();
            });

            ResetGridSize();
            RefreshGridItems();
            _tracker.StartTracking();
        }

        private void ResetGridSize()
        {
            for (var i = 0; i < _tracker.Items.Max(x => x.Column); i++)
                TrackerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });

            for (var i = 0; i < _tracker.Items.Max(x => x.Row); i++)
                TrackerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _tracker.StopTracking();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _tracker?.Dispose();
        }
    }
}
