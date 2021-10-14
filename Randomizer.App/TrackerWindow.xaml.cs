using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for TrackerWindow.xaml
    /// </summary>
    public partial class TrackerWindow : Window
    {
        private const int GridItemPx = 32;
        private const int GridItemMargin = 3;
        private readonly World _world;
        private Tracker _tracker;

        public TrackerWindow(World world)
        {
            InitializeComponent();
            _world = world;
        }

        protected virtual void RefreshGridItems()
        {
            TrackerGrid.Children.Clear();
            foreach (var item in _tracker.Items.Where(x => x.Column != null && x.Row != null))
            {
                var fileName = GetItemSpriteFileName(item);
                if (fileName == null)
                    continue;

                var image = new Image
                {
                    Source = new BitmapImage(new Uri(fileName)),
                    Opacity = item.TrackingState > 0 ? 1.0d : 0.2d,
                    Width = GridItemPx,
                    Height = GridItemPx,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Tag = item
                };

                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                Grid.SetColumn(image, item.Column.Value);
                Grid.SetRow(image, item.Row.Value);
                TrackerGrid.Children.Add(image);
            }
        }

        private static string GetItemSpriteFileName(ItemData item)
        {
            var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Sprites", "Items");
            var fileName = (string)null;

            if (item.Image != null)
            {
                fileName = Path.Combine(folder, item.Image);
                if (File.Exists(fileName))
                    return fileName;
            }

            if (item.HasStages)
            {
                fileName = Path.Combine(folder, $"{item.Name[0].Text.ToLowerInvariant()} ({item.TrackingState}).png");
                if (File.Exists(fileName))
                    return fileName;
            }

            fileName = Path.Combine(folder, $"{item.Name[0].Text.ToLowerInvariant()}.png");
            if (File.Exists(fileName))
                return fileName;

            return null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "tracker.json");
            var provider = new TrackerConfigProvider(path);
            _tracker = new Tracker(provider, _world);
            _tracker.ItemTracked += (sender, e) => Dispatcher.Invoke(() =>
            {
                StatusBarConfidence.Content = $"{e.Confidence:P2}";
                RefreshGridItems();
            });

            ResetGridSize();
            RefreshGridItems();
            _tracker.StartTracking();
        }

        private void ResetGridSize()
        {
            var columns = _tracker.Items.Max(x => x.Column);
            for (var i = 0; i <= columns; i++)
                TrackerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridItemPx + GridItemMargin) });

            var rows = _tracker.Items.Max(x => x.Row);
            for (var i = 0; i <= rows; i++)
                TrackerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridItemPx + GridItemMargin) });
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
