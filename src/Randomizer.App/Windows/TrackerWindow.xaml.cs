using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using Randomizer.App.ViewModels;

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
        private readonly TrackerFactory _trackerFactory;
        private readonly ILogger<TrackerWindow> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<object> _mouseDownSenders = new();
        private bool _pegWorldMode;
        private DateTime _startTime;
        private TimeSpan _elapsedTime;
        private TrackerLocationsWindow _locationsWindow;
        private TrackerHelpWindow _trackerHelpWindow;
        private TrackerMapWindow _trackerMapWindow;
        private TrackerLocationSyncer _locationSyncer;

        public TrackerWindow(IServiceProvider serviceProvider, TrackerFactory trackerFactory, ILogger<TrackerWindow> logger)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _trackerFactory = trackerFactory;
            _logger = logger;
            _dispatcherTimer = new(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Render, (sender, _) =>
            {
                var elapsed = DateTime.Now - _startTime;
                StatusBarTimer.Content = elapsed.Hours > 0
                    ? elapsed.ToString("h':'mm':'ss")
                    : elapsed.ToString("mm':'ss");
            }, Dispatcher);

            App.RestoreWindowPositionAndSize(this);
        }

        protected enum Origin
        {
            TopLeft = 0,
            TopRight = 1,
            BottomLeft = 2,
            BottomRight = 3
        }

        public RandomizerOptions Options { get; set; }

        public Tracker Tracker { get; private set; }

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

        protected Image GetGridItemControl(string imageFileName, int column, int row, string overlayFileName)
            => GetGridItemControl(imageFileName, column, row, new Overlay(overlayFileName, 0, 0));

        protected Image GetGridItemControl(string imageFileName, int column, int row, int counter, string overlayFileName, int minCounter = 2)
        {
            var overlays = new List<Overlay>();
            if (overlayFileName != null)
                overlays.Add(new(overlayFileName, 0, 0));

            if (counter > 9)
            {
                // Limit missiles to 99 for display purposes until I can be
                // bothered to update the display code to not suck
                counter = Math.Clamp(counter, 0, 99);

                var tensDigit = counter / 10 % 10;
                overlays.Add(new(GetDigitMarkFileName(tensDigit), 0, 0)
                {
                    OriginPoint = Origin.BottomLeft
                });

                // Digit images are 10px wide, but we overlap the 2 black pixels
                // in the border
                var digit = counter % 10;
                overlays.Add(new(GetDigitMarkFileName(digit), 8, 0)
                {
                    OriginPoint = Origin.BottomLeft
                });
            }
            else if (counter >= minCounter)
            {
                var digit = counter % 10;
                overlays.Add(new(GetDigitMarkFileName(digit), 0, 0)
                {
                    OriginPoint = Origin.BottomLeft
                });
            }

            return GetGridItemControl(imageFileName, column, row, overlays.ToArray());

            static string GetDigitMarkFileName(int digit) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Sprites", "Marks", $"{digit % 10}.png");
        }

        protected Image GetGridItemControl(string imageFileName, int column, int row,
            params Overlay[] overlays)
        {
            var bitmapImage = new BitmapImage(new Uri(imageFileName));
            if (overlays.Length == 0)
            {
                return GetGridItemControl(bitmapImage, column, row);
            }

            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new ImageDrawing(bitmapImage,
                new Rect(0, 0, GridItemPx, GridItemPx)));

            foreach (var overlay in overlays)
            {
                var overlayImage = new BitmapImage(new Uri(overlay.FileName));
                var x = OffsetX(overlay.X, overlayImage.PixelWidth, overlay.OriginPoint);
                var y = OffsetY(overlay.Y, overlayImage.PixelHeight, overlay.OriginPoint);
                drawingGroup.Children.Add(new ImageDrawing(overlayImage,
                    new Rect(x, y, overlayImage.PixelWidth, overlayImage.PixelHeight)));
            }

            return GetGridItemControl(new DrawingImage(drawingGroup), column, row);

            int OffsetX(int x, int width, Origin origin) => origin switch
            {
                Origin.TopLeft or Origin.BottomLeft => x,
                Origin.TopRight or Origin.BottomRight => GridItemPx - width - x,
                _ => throw new InvalidEnumArgumentException(nameof(origin), (int)origin, typeof(Origin))
            };

            int OffsetY(int y, int height, Origin origin) => origin switch
            {
                Origin.TopLeft or Origin.TopRight => y,
                Origin.BottomLeft or Origin.BottomRight => GridItemPx - height - y,
                _ => throw new InvalidEnumArgumentException(nameof(origin), (int)origin, typeof(Origin))
            };
        }

        protected Image GetGridItemControl(ImageSource imageSource, int column, int row)
        {
            var image = new Image
            {
                Source = imageSource,
                MaxWidth = GridItemPx,
                MaxHeight = GridItemPx,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            if (Options.GeneralOptions.TrackerShadows)
            {
                image.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 315,
                    BlurRadius = 5,
                    Opacity = 0.8,
                    ShadowDepth = 2
                };
            }

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
                    image.Tag = peg;
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;
                    TrackerGrid.Children.Add(image);
                }
            }
            else
            {
                foreach (var item in Tracker.Items.Where(x => x.Column != null && x.Row != null))
                {
                    var fileName = GetItemSpriteFileName(item);
                    var overlay = GetOverlayImageFileName(item);
                    if (fileName == null)
                        continue;

                    var image = GetGridItemControl(fileName,
                        item.Column.Value, item.Row.Value,
                        item.Counter, overlay, minCounter: 2);
                    image.Tag = item;
                    image.ContextMenu = CreateContextMenu(item);
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;
                    image.Opacity = item.TrackingState > 0 ? 1.0d : 0.2d;
                    TrackerGrid.Children.Add(image);
                }

                foreach (var dungeon in Tracker.Dungeons.Where(x => x.Column != null && x.Row != null))
                {
                    var overlayPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Dungeons", $"{dungeon.Name[0].Text.ToLowerInvariant()}.png");

                    var rewardPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Dungeons", $"{(dungeon.HasReward ? dungeon.Reward.GetDescription().ToLowerInvariant() : "blank")}.png");
                    var image = GetGridItemControl(rewardPath,
                        dungeon.Column.Value, dungeon.Row.Value,
                        dungeon.TreasureRemaining, overlayPath, minCounter: 1);
                    image.Tag = dungeon;
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;
                    image.ContextMenu = CreateContextMenu(dungeon);
                    image.Opacity = dungeon.Cleared ? 1.0d : 0.2d;

                    TrackerGrid.Children.Add(image);
                }
            }
        }

        private string GetOverlayImageFileName(ItemData item)
        {
            return item switch
            {
                { InternalItemType: ItemType.Bombos } => GetMatchingDungeonNameImages(Medallion.Bombos),
                { InternalItemType: ItemType.Ether } => GetMatchingDungeonNameImages(Medallion.Ether),
                { InternalItemType: ItemType.Quake } => GetMatchingDungeonNameImages(Medallion.Quake),
                _ => null
            };

            string GetMatchingDungeonNameImages(Medallion requirement)
            {
                var names = Tracker.Dungeons.Where(x => x.Requirement == requirement)
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

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDownSenders.Add(sender);
        }

        private void Image_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if (!_mouseDownSenders.Remove(sender))
                return;

            if (sender is Image image)
            {
                if (image.Tag is ItemData item)
                {
                    Tracker.TrackItem(item);
                }
                else if (image.Tag is ZeldaDungeon dungeon)
                {
                    Tracker.MarkDungeonAsCleared(dungeon);
                }
                else if (image.Tag is Peg peg)
                {
                    Tracker.Peg(peg);
                }
            };
        }

        private ContextMenu CreateContextMenu(ItemData item)
        {
            var menu = new ContextMenu
            {
                Style = Application.Current.FindResource("DarkContextMenu") as Style
            };

            if (item.TrackingState > 0 && item.InternalItemType != ItemType.Bow && item.InternalItemType != ItemType.SilverArrows)
            {
                var untrackItem = new MenuItem
                {
                    Header = item.TrackingState > 1 ? "Remove one" : "Untrack"
                };
                untrackItem.Click += (sender, e) => Tracker.UntrackItem(item);
                menu.Items.Add(untrackItem);
            }

            if (item.HasStages)
            {
                foreach ((var stage, var name) in item.Stages)
                {
                    var setStageItem = new MenuItem
                    {
                        Header = $"Set as {name[0]}",
                        IsChecked = item.TrackingState == stage
                    };

                    setStageItem.Click += (sender, e) =>
                    {
                        item.TrackingState = stage;
                        RefreshGridItems();
                    };
                    menu.Items.Add(setStageItem);
                }
            }

            var medallion = item.InternalItemType switch
            {
                ItemType.Bombos => Medallion.Bombos,
                ItemType.Ether => Medallion.Ether,
                ItemType.Quake => Medallion.Quake,
                _ => Medallion.None
            };
            if (medallion != Medallion.None)
            {
                var turtleRock = Tracker.Dungeons.Single(x => x.Name.Contains("Turtle Rock", StringComparison.OrdinalIgnoreCase));
                var miseryMire = Tracker.Dungeons.Single(x => x.Name.Contains("Misery Mire", StringComparison.OrdinalIgnoreCase));

                var requiredByNone = new MenuItem
                {
                    Header = "Not required for any dungeon",
                    IsChecked = turtleRock.Requirement != medallion && miseryMire.Requirement != medallion
                };
                requiredByNone.Click += (sender, e) =>
                {
                    if (turtleRock.Requirement == medallion)
                        turtleRock.Requirement = Medallion.None;
                    if (miseryMire.Requirement == medallion)
                        miseryMire.Requirement = Medallion.None;
                    RefreshGridItems();
                };

                var requiredByTR = new MenuItem
                {
                    Header = "Required for Turtle Rock",
                    IsChecked = turtleRock.Requirement == medallion && miseryMire.Requirement != medallion
                };
                requiredByTR.Click += (sender, e) =>
                {
                    turtleRock.Requirement = medallion;
                    if (miseryMire.Requirement == medallion)
                        miseryMire.Requirement = Medallion.None;
                    RefreshGridItems();
                };

                var requiredByMM = new MenuItem
                {
                    Header = "Required for Misery Mire",
                    IsChecked = turtleRock.Requirement != medallion && miseryMire.Requirement == medallion
                };
                requiredByMM.Click += (sender, e) =>
                {
                    if (turtleRock.Requirement == medallion)
                        turtleRock.Requirement = Medallion.None;
                    miseryMire.Requirement = medallion;
                    RefreshGridItems();
                };

                var requiredByBoth = new MenuItem
                {
                    Header = "Required by both",
                    IsChecked = turtleRock.Requirement == medallion && miseryMire.Requirement == medallion
                };
                requiredByBoth.Click += (sender, e) =>
                {
                    turtleRock.Requirement = medallion;
                    miseryMire.Requirement = medallion;
                    RefreshGridItems();
                };

                menu.Items.Add(requiredByNone);
                menu.Items.Add(requiredByTR);
                menu.Items.Add(requiredByMM);
                menu.Items.Add(requiredByBoth);
            }

            if (item.InternalItemType is ItemType.Bow or ItemType.SilverArrows)
            {
                var bow = Tracker.Items.SingleOrDefault(x => x.InternalItemType == ItemType.Bow);
                var toggleBow = new MenuItem
                {
                    Header = bow.TrackingState > 0 ? "Untrack Bow" : "Track Bow",
                    Icon = new Image
                    {
                        Source = new BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Items", "bow.png")))
                    }
                };
                toggleBow.Click += (sender, e) =>
                {
                    if (bow.TrackingState > 0)
                        bow.Untrack();
                    else
                        bow.Track();
                    RefreshGridItems();
                };

                var silverArrows = Tracker.Items.SingleOrDefault(x => x.InternalItemType == ItemType.SilverArrows);
                var toggleSilverArrows = new MenuItem
                {
                    Header = silverArrows.TrackingState > 0 ? "Untrack Silver Arrows" : "Track Silver Arrows",
                    Icon = new Image
                    {
                        Source = new BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Items", "silver arrows.png")))
                    }
                };
                toggleSilverArrows.Click += (sender, e) =>
                {
                    if (silverArrows.TrackingState > 0)
                        silverArrows.Untrack();
                    else
                        silverArrows.Track();
                    RefreshGridItems();
                };

                menu.Items.Add(toggleBow);
                menu.Items.Add(toggleSilverArrows);
            }

            return menu.Items.Count > 0 ? menu : null;
        }

        private ContextMenu CreateContextMenu(ZeldaDungeon dungeon)
        {
            var menu = new ContextMenu
            {
                Style = Application.Current.FindResource("DarkContextMenu") as Style
            };

            if (dungeon.Cleared)
            {
                var unclear = new MenuItem
                {
                    Header = "Reset cleared status",
                };
                unclear.Click += (sender, e) =>
                {
                    Tracker.MarkDungeonAsIncomplete(dungeon);
                };
                menu.Items.Add(unclear);
            }

            if (dungeon.HasReward)
            {
                foreach (var reward in Enum.GetValues<RewardItem>())
                {
                    var item = new MenuItem
                    {
                        Header = $"Mark as {reward.GetDescription()}",
                        IsChecked = dungeon.Reward == reward,
                        Icon = new Image
                        {
                            Source = new BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "Sprites", "Dungeons", $"{reward.GetDescription().ToLowerInvariant()}.png")))
                        }
                    };
                    item.Click += (sender, e) =>
                    {
                        dungeon.Reward = reward;
                        RefreshGridItems();
                    };
                    menu.Items.Add(item);
                }
            }
            return menu;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Background = new SolidColorBrush(Options.GeneralOptions.TrackerBackgroundColor);

            InitializeTracker();
            ResetGridSize();
            RefreshGridItems();
            Tracker.StartTracking();
            _startTime = DateTime.Now;
            _dispatcherTimer.Start();

            // Show proper voice status bar icon and warn the user if no mic is available
            StatusBarConfidence.Visibility = Tracker.VoiceRecognitionEnabled ? Visibility.Visible : Visibility.Collapsed;
            StatusBarVoiceDisabled.Visibility = Tracker.VoiceRecognitionEnabled ? Visibility.Collapsed : Visibility.Visible;
            if (!Tracker.MicrophoneInitialized)
            {
                ShowNoMicrophoneWarning();
            }

            _locationSyncer = _serviceProvider.GetRequiredService<TrackerLocationSyncer>();
            _locationsWindow = _serviceProvider.GetRequiredService<TrackerLocationsWindow>();
            _locationsWindow.Show();
            _trackerMapWindow = _serviceProvider.GetRequiredService<TrackerMapWindow>();
            _trackerMapWindow.Syncer = _locationSyncer;
            _trackerMapWindow.Show();
        }

        private void InitializeTracker()
        {
            if (Options == null)
                throw new InvalidOperationException("Cannot initialize Tracker before assigning " + nameof(Options));

            Tracker = _trackerFactory.Create(Options.GeneralOptions.GetTrackerOptions());
            Tracker.ItemTracked += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = false;
                UpdateStats(e);
                RefreshGridItems();
            });
            Tracker.ToggledPegWorldModeOn += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = Tracker.PegWorldMode;
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
            Tracker.LocationCleared += (sender, e) => Dispatcher.Invoke(() =>
            {
                UpdateStats(e);
            });
            Tracker.GoModeToggledOn += (sender, e) => Dispatcher.Invoke(() =>
            {
                TrackerStatusBar.Background = Brushes.Green;
                StatusBarGoMode.Visibility = Visibility.Visible;
                UpdateStats(e);
            });
            Tracker.ActionUndone += (sender, e) => Dispatcher.Invoke(() =>
            {
                if (!Tracker.GoMode)
                {
                    TrackerStatusBar.Background = null;
                    StatusBarGoMode.Visibility = Visibility.Collapsed;
                }

                UpdateStats(e);
                RefreshGridItems();
            });
            Tracker.StateLoaded += (sender, e) => Dispatcher.Invoke(() =>
            {
                RefreshGridItems();
                ResetGridSize();
                TrackerStatusBar.Background = Tracker.GoMode ? Brushes.Green : null;
                StatusBarGoMode.Visibility = Tracker.GoMode ? Visibility.Visible : Visibility.Collapsed;
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

            TrackerGrid.ColumnDefinitions.Clear();
            for (var i = 0; i <= columns; i++)
                TrackerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridItemPx + GridItemMargin) });

            var rows = Math.Max(Tracker.Items.Max(x => x.Row) ?? 0,
                Tracker.Dungeons.Max(x => x.Row) ?? 0);

            TrackerGrid.RowDefinitions.Clear();
            for (var i = 0; i <= rows; i++)
                TrackerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridItemPx + GridItemMargin) });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Tracker.StopTracking();
            _dispatcherTimer.Stop();
            App.SaveWindowPositionAndSize(this);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _locationsWindow.Close();
            _locationsWindow = null;
            _trackerMapWindow.Close();
            _trackerMapWindow = null;
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
                _locationsWindow = new TrackerLocationsWindow(_locationSyncer);
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

        private async void LoadSavedState_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Tracker state files (*.json.gz)|*.json.gz|All files (*.*)|*.*"
            };

            while (dialog.ShowDialog(this) == true)
            {
                try
                {
                    using (var stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        await Tracker.LoadAsync(gzip);
                    }

                    break;
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, "Failed to load Tracker state from '{fileName}'.", dialog.FileName);

                    var result = MessageBox.Show(this, "Could not load Tracker using the selected saved state file. Please check you selected the right file and try again.", "SMZ3 Cas’ Randomizer", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Cancel)
                        break;
                }
            }
        }

        private async void SaveState_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Tracker state files (*.json.gz)|*.json.gz|All files (*.*)|*.*",
                OverwritePrompt = true
            };

            while (dialog.ShowDialog(this) == true)
            {
                try
                {
                    using (var stream = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var gzip = new GZipStream(stream, CompressionLevel.Fastest))
                    {
                        await Tracker.SaveAsync(gzip);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to saved Tracker state to '{fileName}'.", dialog.FileName);
                    var result = MessageBox.Show(this, "Could not save Tracker state to the selected file. Please check you have access and try again.", "SMZ3 Cas’ Randomizer", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Cancel)
                        break;
                }
            }
        }

        private void StatusBarTimer_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Reset timer on double click
            _startTime = DateTime.Now;
        }

        private void StatusBarTimer_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Pause/resume timer on right click
            if (_dispatcherTimer.IsEnabled)
            {
                _elapsedTime = DateTime.Now - _startTime;
                _dispatcherTimer.Stop();
            }
            else
            {
                _startTime = DateTime.Now - _elapsedTime;
                _dispatcherTimer.Start();
            }
        }

        /// <summary>
        /// Double clicking on the status bar icon to disable voice recognition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusBarStatusBarConfidence_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Tracker.DisableVoiceRecognition();
            StatusBarConfidence.Visibility = Tracker.VoiceRecognitionEnabled ? Visibility.Visible : Visibility.Collapsed;
            StatusBarVoiceDisabled.Visibility = Tracker.VoiceRecognitionEnabled ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Double clicking on the status bar icon to attempt to enable voice recognition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusBarVoiceDisabled_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Tracker.MicrophoneInitialized)
            {
                if (!Tracker.InitializeMicrophone())
                {
                    ShowNoMicrophoneWarning();
                    return;
                }
            }
            Tracker.EnableVoiceRecognition();
            StatusBarConfidence.Visibility = Tracker.VoiceRecognitionEnabled ? Visibility.Visible : Visibility.Collapsed;
            StatusBarVoiceDisabled.Visibility = Tracker.VoiceRecognitionEnabled ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Displays a warning to the user that we could not detect a microphone
        /// </summary>
        private void ShowNoMicrophoneWarning()
        {
            MessageBox.Show(this, "There is a problem with your microphone. Please check your sound settings to ensure you have a microphone enabled.\n\n" +
                "Voice recognition has been disabled. You can attempt to re-enable it by double clicking on the Voice Disabled text.", "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void MapMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<TrackerMapWindow>().Any())
            {
                _trackerMapWindow.Activate();
            }
            else
            {
                var scope = _serviceProvider.CreateScope();
                _trackerMapWindow = scope.ServiceProvider.GetRequiredService<TrackerMapWindow>();
                _trackerMapWindow.Syncer = _locationSyncer;
                _trackerMapWindow.Show();
            }
        }

        protected record Overlay(string FileName, int X, int Y)
        {
            public Origin OriginPoint { get; init; }
        }
    }
}
