﻿using System;
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
        private readonly World _world;
        private Tracker _tracker;
        private DispatcherTimer _dispatcherTimer;
        private bool _pegWorldMode;
        private DateTime _startTime;

        public TrackerWindow(World world)
        {
            InitializeComponent();
            _world = world;
            _dispatcherTimer = new(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Background, (sender, _) =>
            {
                var elapsed = DateTime.Now - _startTime;
                StatusBarTimer.Content = elapsed.Hours > 0
                    ? elapsed.ToString("h':'mm':'ss")
                    : elapsed.ToString("mm':'ss");
            }, Dispatcher);
        }

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
                foreach (var peg in _tracker.Pegs)
                {
                    var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Items", peg.Pegged ? "pegged.png" : "peg.png");

                    var image = GetGridItemControl(fileName, peg.Column, peg.Row);
                    image.MouseLeftButtonUp += (sender, e) => _tracker.Peg(peg);
                    TrackerGrid.Children.Add(image);
                }
            }
            else
            {
                foreach (var item in _tracker.Items.Where(x => x.Column != null && x.Row != null))
                {
                    var fileName = GetItemSpriteFileName(item);
                    var overlay = GetOverlayImageFileName(item, _tracker.Dungeons);
                    if (fileName == null)
                        continue;

                    var image = GetGridItemControl(fileName, item.Column.Value, item.Row.Value, overlay);
                    image.MouseLeftButtonUp += (sender, e) => _tracker.TrackItem(item);
                    image.Opacity = item.TrackingState > 0 ? 1.0d : 0.2d;
                    TrackerGrid.Children.Add(image);
                }

                foreach (var dungeon in _tracker.Dungeons.Where(x => x.Column != null && x.Row != null))
                {
                    var overlayPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Dungeons", $"{dungeon.Name[0].Text.ToLowerInvariant()}.png");

                    var rewardPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Dungeons", $"{dungeon.Reward.GetDescription().ToLowerInvariant()}.png");
                    var rewardImage = GetGridItemControl(rewardPath, dungeon.Column.Value, dungeon.Row.Value, overlayPath);
                    rewardImage.Opacity = dungeon.Cleared ? 1.0d : 0.2d;
                    rewardImage.MouseLeftButtonUp += (sender, e) => _tracker.ClearDungeon(dungeon);
                    rewardImage.MouseRightButtonUp += (sender, e) => _tracker.SetDungeonReward(dungeon);

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
            _tracker.StartTracking();
            _startTime = DateTime.Now;
            _dispatcherTimer.Start();

            var trackerLocations = new TrackerLocationsWindow(_tracker);
            trackerLocations.Show();
        }

        private void InitializeTracker()
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
            _tracker.DungeonUpdated += (sender, e) => Dispatcher.Invoke(() =>
            {
                StatusBarConfidence.Content = $"{e.Confidence:P2}";
                _pegWorldMode = false;
                RefreshGridItems();
            });
            _tracker.GoModeToggledOn += (sender, e) => Dispatcher.Invoke(() =>
            {
                StatusBarConfidence.Content = $"{e.Confidence:P2}";
                GoModeBorder.BorderBrush = new SolidColorBrush(Colors.Green);
            });
        }

        private void ResetGridSize()
        {
            var columns = Math.Max(_tracker.Items.Max(x => x.Column) ?? 0,
                _tracker.Dungeons.Max(x => x.Column) ?? 0);
            for (var i = 0; i <= columns; i++)
                TrackerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridItemPx + GridItemMargin) });

            var rows = Math.Max(_tracker.Items.Max(x => x.Row) ?? 0,
                _tracker.Dungeons.Max(x => x.Row) ?? 0);
            for (var i = 0; i <= rows; i++)
                TrackerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridItemPx + GridItemMargin) });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _tracker.StopTracking();
            _dispatcherTimer.Stop();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _tracker?.Dispose();
        }
    }
}
