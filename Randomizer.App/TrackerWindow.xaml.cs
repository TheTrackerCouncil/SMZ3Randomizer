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
        private const int GridItemPx = 32;
        private const int GridItemMargin = 3;
        private readonly World _world;
        private Tracker _tracker;
        private bool _pegWorldMode;

        public TrackerWindow(World world)
        {
            InitializeComponent();
            _world = world;
        }

        protected virtual void RefreshGridItems()
        {
            TrackerGrid.Children.Clear();

            if (_pegWorldMode)
            {
                foreach (var peg in _tracker.Pegs)
                {
                    var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Items", peg.Pegged ? "pegged.png" : "peg.png");

                    var image = GetGridItemControl(fileName, peg.Column, peg.Row);
                    image.MouseLeftButtonUp += (sender, e) => _tracker.Peg(peg);
                    image.Tag = peg;
                    TrackerGrid.Children.Add(image);
                }
            }
            else
            {
                foreach (var item in _tracker.Items.Where(x => x.Column != null && x.Row != null))
                {
                    var fileName = GetItemSpriteFileName(item);
                    if (fileName == null)
                        continue;

                    var image = GetGridItemControl(fileName, item.Column.Value, item.Row.Value);
                    image.MouseLeftButtonUp += (sender, e) => _tracker.TrackItem(item);
                    image.Opacity = item.TrackingState > 0 ? 1.0d : 0.2d;
                    image.Tag = item;
                    TrackerGrid.Children.Add(image);
                }
            }

            static Image GetGridItemControl(string imageFileName, int column, int row)
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(imageFileName)),
                    Width = GridItemPx,
                    Height = GridItemPx,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                Grid.SetColumn(image, column);
                Grid.SetRow(image, row);
                return image;
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
                _pegWorldMode = false;
                RefreshGridItems();
            });
            _tracker.ToggledPegWorldModeOn += (sender, e) => Dispatcher.Invoke(() =>
            {
                StatusBarConfidence.Content = $"{e.Confidence:P2}";
                _pegWorldMode = true;
                RefreshGridItems();
            });
            _tracker.PegPegged += (sender, e) => Dispatcher.Invoke(() =>
            {
                StatusBarConfidence.Content = $"{e.Confidence:P2}";
                _pegWorldMode = _tracker.Pegs.Any(x => !x.Pegged);
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
