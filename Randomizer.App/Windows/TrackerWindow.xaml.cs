using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        private readonly DispatcherTimer _dispatcherTimer;
        private bool _pegWorldMode;
        private DateTime _startTime;

        private TrackerLocationsWindow _locationsWindow;
        private TrackerHelpWindow _trackerHelpWindow;

        public TrackerWindow(Tracker tracker)
        {
            InitializeComponent();

            Tracker = tracker;
            _dispatcherTimer = new(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Background, (sender, _) =>
            {
                var elapsed = DateTime.Now - _startTime;
                StatusBarTimer.Content = elapsed.Hours > 0
                    ? elapsed.ToString("h':'mm':'ss")
                    : elapsed.ToString("mm':'ss");
            }, Dispatcher);
        }

        public Tracker Tracker { get; }

        public static string GetItemSpriteFileName(ItemData item)
        {
            var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Sprites", "Items");
            var fileName = (string)null;

            if (item.Image != null)
            {
                fileName = Path.Combine(folder, item.Image);
                if (File.Exists(fileName))
                    return fileName;
            }

            if (item.HasStages || item.Multiple)
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

        protected static Image GetGridItemControl(string imageFileName, int column, int row,
                    string overlayFileName = null, double overlayOffsetX = 0, double overlayOffsetY = 0)
        {
            var bitmapImage = new BitmapImage(new Uri(imageFileName));
            if (overlayFileName == null)
            {
                return GetGridItemControl(bitmapImage, column, row);
            }

            var overlayImage = new BitmapImage(new Uri(overlayFileName));
            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new ImageDrawing(bitmapImage,
                new Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight)));
            drawingGroup.Children.Add(new ImageDrawing(overlayImage,
                new Rect(overlayOffsetX, overlayOffsetY, overlayImage.PixelWidth, overlayImage.PixelHeight)));
            return GetGridItemControl(new DrawingImage(drawingGroup), column, row);
        }

        protected static Image GetGridItemControl(ImageSource imageSource, int column, int row)
        {
            var image = new Image
            {
                Source = imageSource,
                MaxWidth = GridItemPx,
                MaxHeight = GridItemPx,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            Grid.SetColumn(image, column);
            Grid.SetRow(image, row);
            return image;
        }

        protected virtual void RefreshGridItems()
        {
            TrackerGrid.Children.Clear();

            if (_pegWorldMode)
            {
                foreach (var peg in Tracker.Pegs)
                {
                    var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Items", peg.Pegged ? "pegged.png" : "peg.png");

                    var image = GetGridItemControl(fileName, peg.Column, peg.Row);
                    image.MouseLeftButtonUp += (sender, e) => Tracker.Peg(peg);
                    TrackerGrid.Children.Add(image);
                }
            }
            else
            {
                foreach (var item in Tracker.Items.Where(x => x.Column != null && x.Row != null))
                {
                    var fileName = GetItemSpriteFileName(item);
                    var overlay = GetOverlayImageFileName(item, Tracker.Dungeons);
                    if (fileName == null)
                        continue;

                    var image = GetGridItemControl(fileName, item.Column.Value, item.Row.Value, overlay);
                    image.MouseLeftButtonUp += (sender, e) => Tracker.TrackItem(item);
                    image.Opacity = item.TrackingState > 0 ? 1.0d : 0.2d;
                    TrackerGrid.Children.Add(image);
                }

                foreach (var dungeon in Tracker.Dungeons.Where(x => x.Column != null && x.Row != null))
                {
                    var overlayPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Dungeons", $"{dungeon.Name[0].Text.ToLowerInvariant()}.png");

                    var rewardPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Dungeons", $"{dungeon.Reward.GetDescription().ToLowerInvariant()}.png");
                    var rewardImage = GetGridItemControl(rewardPath, dungeon.Column.Value, dungeon.Row.Value, overlayPath);
                    rewardImage.Opacity = dungeon.Cleared ? 1.0d : 0.2d;
                    rewardImage.MouseLeftButtonUp += (sender, e) => Tracker.ClearDungeon(dungeon);
                    rewardImage.MouseRightButtonUp += (sender, e) => Tracker.SetDungeonReward(dungeon);

                    TrackerGrid.Children.Add(rewardImage);
                }
            }
        }

        private static string GetOverlayImageFileName(ItemData item, IEnumerable<ZeldaDungeon> dungeons)
        {
            return item.InternalItemType switch
            {
                ItemType.Bombos => GetMatchingDungeonNameImages(Medallion.Bombos, dungeons),
                ItemType.Ether => GetMatchingDungeonNameImages(Medallion.Ether, dungeons),
                ItemType.Quake => GetMatchingDungeonNameImages(Medallion.Quake, dungeons),
                _ => null
            };

            static string GetMatchingDungeonNameImages(Medallion requirement, IEnumerable<ZeldaDungeon> dungeons)
            {
                var names = dungeons.Where(x => x.Requirement == requirement)
                    .Select(x => x.Name[0])
                    .ToList();

                if (names.Count == 1)
                {
                    return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Dungeons", $"{names[0]}.png");
                }
                else if (names.Count > 1)
                {
                    return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Dungeons", "both.png");
                }

                return null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTracker();
            ResetGridSize();
            RefreshGridItems();
            Tracker.StartTracking();
            _startTime = DateTime.Now;
            _dispatcherTimer.Start();

            _locationsWindow = new TrackerLocationsWindow(Tracker);
            _locationsWindow.Show();
        }

        private void InitializeTracker()
        {
            Tracker.ItemTracked += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = false;
                UpdateStats(e);
                RefreshGridItems();
            });
            Tracker.ToggledPegWorldModeOn += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = true;
                UpdateStats(e);
                RefreshGridItems();
            });
            Tracker.PegPegged += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = Tracker.Pegs.Any(x => !x.Pegged);
                UpdateStats(e);
                RefreshGridItems();
            });
            Tracker.DungeonUpdated += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = false;
                UpdateStats(e);
                RefreshGridItems();
            });
            Tracker.MarkedLocationsUpdated += (sender, e) => Dispatcher.Invoke(() =>
            {
                UpdateStats(e);
            });
            Tracker.GoModeToggledOn += (sender, e) => Dispatcher.Invoke(() =>
            {
                GoModeBorder.BorderBrush = new SolidColorBrush(Colors.Green);
                UpdateStats(e);
            });
            Tracker.ActionUndone += (sender, e) => Dispatcher.Invoke(() =>
            {
                if (!Tracker.GoMode)
                    GoModeBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);

                UpdateStats(e);
                RefreshGridItems();
            });
        }

        private void UpdateStats(TrackerEventArgs e)
        {
            if (e.Confidence != null)
                StatusBarConfidence.Content = $"{e.Confidence:P2}";
        }

        private void ResetGridSize()
        {
            var columns = Math.Max(Tracker.Items.Max(x => x.Column) ?? 0,
                Tracker.Dungeons.Max(x => x.Column) ?? 0);
            for (var i = 0; i <= columns; i++)
                TrackerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridItemPx + GridItemMargin) });

            var rows = Math.Max(Tracker.Items.Max(x => x.Row) ?? 0,
                Tracker.Dungeons.Max(x => x.Row) ?? 0);
            for (var i = 0; i <= rows; i++)
                TrackerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridItemPx + GridItemMargin) });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Tracker.StopTracking();
            _dispatcherTimer.Stop();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Tracker?.Dispose();
        }

        private void LocationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<TrackerLocationsWindow>().Any())
            {
                _locationsWindow.Activate();
            }
            else
            {
                _locationsWindow = new TrackerLocationsWindow(Tracker);
                _locationsWindow.Show();
            }
        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<TrackerHelpWindow>().Any())
            {
                _trackerHelpWindow.Activate();
            }
            else
            {
                _trackerHelpWindow = new TrackerHelpWindow(Tracker);
                _trackerHelpWindow.Show();
            }
        }
    }
}
