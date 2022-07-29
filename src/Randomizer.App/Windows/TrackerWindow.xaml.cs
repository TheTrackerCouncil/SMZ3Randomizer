using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
using Randomizer.App.Windows;
using Randomizer.Shared;
using Randomizer.Shared.Models;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Services;

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
        private readonly ILogger<TrackerWindow> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IItemService _itemService;
        private readonly List<object> _mouseDownSenders = new();
        private bool _pegWorldMode;
        private TrackerLocationsWindow _locationsWindow;
        private TrackerHelpWindow _trackerHelpWindow;
        private TrackerMapWindow _trackerMapWindow;
        private AutoTrackerWindow _autoTrackerHelpWindow;
        private TrackerLocationSyncer _locationSyncer;
        private RomGenerator _romGenerator;
        private MenuItem _autoTrackerDisableMenuItem;
        private MenuItem _autoTrackerLuaMenuItem;
        private MenuItem _autoTrackerUSB2SNESMenuItem;

        public TrackerWindow(IServiceProvider serviceProvider,
            IItemService itemService,
            ILogger<TrackerWindow> logger,
            RomGenerator romGenerator
        )
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _itemService = itemService;
            _logger = logger;
            _romGenerator = romGenerator;

            _dispatcherTimer = new(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Render, (sender, _) =>
            {
                var elapsed = Tracker.TotalElapsedTime;
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

        public GeneratedRom Rom { get; set; }

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

        public static string GetItemSpriteFileName(BossInfo boss)
        {
            var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Sprites", "Items");
            var fileName = (string)null;

            if (boss.Image != null)
            {
                fileName = Path.Combine(folder, boss.Image);
                if (File.Exists(fileName))
                    return fileName;
            }

            fileName = Path.Combine(folder, $"{boss.Name[0].Text.ToLowerInvariant()}.png");
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

            if (counter >= minCounter)
            {
                var offset = 0;
                foreach (var digit in GetDigits(counter))
                {
                    overlays.Add(new(GetDigitMarkFileName(digit), offset, 0)
                    {
                        OriginPoint = Origin.BottomLeft
                    });
                    offset += 8;
                }
            }

            return GetGridItemControl(imageFileName, column, row, overlays.ToArray());

            static string GetDigitMarkFileName(int digit) => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Sprites", "Marks", $"{digit % 10}.png");
        }

        internal static IEnumerable<int> GetDigits(int value)
        {
            var numDigits = value.ToString("0", CultureInfo.InvariantCulture).Length;
            for (var i = numDigits; i > 0; i--)
            {
                yield return value / (int)Math.Pow(10, i - 1) % 10;
            }
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
                foreach (var item in _itemService.AllItems().Where(x => x.Column != null && x.Row != null))
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

                foreach (var dungeon in Tracker.WorldInfo.Dungeons.Where(x => x.Column != null && x.Row != null))
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

                foreach (var boss in Tracker.WorldInfo.Bosses.Where(x => x.Column != null && x.Row != null))
                {
                    var fileName = GetItemSpriteFileName(boss);
                    var overlay = GetOverlayImageFileName(boss);
                    if (fileName == null)
                        continue;

                    var image = GetGridItemControl(fileName,
                        boss.Column.Value, boss.Row.Value);
                    image.Tag = boss;
                    image.ContextMenu = CreateContextMenu(boss);
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;
                    image.Opacity = boss.Defeated ? 1.0d : 0.2d;
                    TrackerGrid.Children.Add(image);
                }
            }
        }

        private string GetOverlayImageFileName(BossInfo boss)
        {
            return null;
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
                var names = Tracker.WorldInfo.Dungeons.Where(x => x.Requirement == requirement)
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
                else if (image.Tag is DungeonInfo dungeon)
                {
                    Tracker.MarkDungeonAsCleared(dungeon);
                }
                else if (image.Tag is Peg peg)
                {
                    Tracker.Peg(peg);
                }
                else if (image.Tag is BossInfo boss)
                {
                    Tracker.MarkBossAsDefeated(boss);
                }
                else
                {
                    _logger.LogError("Unrecognized image tag type {TagType}", image.Tag.GetType());
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
                var turtleRock = Tracker.WorldInfo.Dungeon<TurtleRock>();
                var miseryMire = Tracker.WorldInfo.Dungeon<MiseryMire>();

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
                    _locationSyncer.OnLocationUpdated("");
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
                    _locationSyncer.OnLocationUpdated("");
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
                    _locationSyncer.OnLocationUpdated("");
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
                    _locationSyncer.OnLocationUpdated("");
                    RefreshGridItems();
                };

                menu.Items.Add(requiredByNone);
                menu.Items.Add(requiredByTR);
                menu.Items.Add(requiredByMM);
                menu.Items.Add(requiredByBoth);
            }

            if (item.InternalItemType is ItemType.Bow or ItemType.SilverArrows)
            {
                var bow = _itemService.GetOrDefault(ItemType.Bow);
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
                    Tracker.UpdateTrackerProgression = true;
                    RefreshGridItems();
                };

                var silverArrows = _itemService.GetOrDefault(ItemType.SilverArrows);
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
                    Tracker.UpdateTrackerProgression = true;
                    RefreshGridItems();
                };

                menu.Items.Add(toggleBow);
                menu.Items.Add(toggleSilverArrows);
            }

            if (item.InternalItemType == ItemType.Flute || "Duck".Equals(item.Name[0], StringComparison.OrdinalIgnoreCase))
            {
                var flute = _itemService.GetOrDefault(ItemType.Flute);
                var toggleFlute = new MenuItem
                {
                    Header = flute.TrackingState > 0 ? "Untrack Flute" : "Track Flute",
                    Icon = new Image
                    {
                        Source = new BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Items", "flute.png")))
                    }
                };
                toggleFlute.Click += (sender, e) =>
                {
                    if (flute.TrackingState > 0)
                        flute.Untrack();
                    else
                        flute.Track();
                    Tracker.UpdateTrackerProgression = true;
                    RefreshGridItems();
                };

                var duck = _itemService.FindOrDefault("Duck");
                var toggleDuck = new MenuItem
                {
                    Header = duck.TrackingState > 0 ? "Untrack Duck" : "Track Duck",
                    Icon = new Image
                    {
                        Source = new BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Sprites", "Items", "duck.png")))
                    }
                };
                toggleDuck.Click += (sender, e) =>
                {
                    if (duck.TrackingState > 0)
                        duck.Untrack();
                    else
                        duck.Track();
                    Tracker.UpdateTrackerProgression = true;
                    RefreshGridItems();
                };

                menu.Items.Add(toggleFlute);
                menu.Items.Add(toggleDuck);
            }

            return menu.Items.Count > 0 ? menu : null;
        }

        private ContextMenu CreateContextMenu(BossInfo boss)
        {
            var menu = new ContextMenu
            {
                Style = Application.Current.FindResource("DarkContextMenu") as Style
            };

            if (boss.Defeated)
            {
                var unclear = new MenuItem
                {
                    Header = $"Revive {boss.Name[0]}",
                };
                unclear.Click += (sender, e) =>
                {
                    Tracker.MarkBossAsNotDefeated(boss);
                };
                menu.Items.Add(unclear);
            }

            return menu;
        }

        private ContextMenu CreateContextMenu(DungeonInfo dungeon)
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

            // If a rom was passed in, generate its seed to populate all locations and items
            if (GeneratedRom.IsValid(Rom))
            {
                var config = SMZ3.Tracking.TrackerState.LoadConfig(Rom);
                Options = Options.Clone();
                Options.LogicConfig = config.LogicConfig;
                Options.SeedOptions.Race = config.Race;
                Options.SeedOptions.Keysanity = config.Keysanity;
                Options.SeedOptions.ConfigString = config.SettingsString;
                Options.SeedOptions.CopySeedAndRaceSettings = config.CopySeedAndRaceSettings;
                if (Rom.GeneratorVersion == 0) Options.LogicConfig.FireRodDarkRooms = true;
                _romGenerator.GenerateSeed(Options, Rom.Seed);
            }

            InitializeTracker();
            ResetGridSize();
            RefreshGridItems();

            // If a rom was passed in with a valid tracker state, reload the state from the database
            if (GeneratedRom.IsValid(Rom))
            {
                Tracker.Load(Rom);
            }

            if (!Tracker.TryStartTracking())
            {
                ShowModuleWarning();
            }

            Tracker.ConnectToChat(Options.GeneralOptions.TwitchUserName, Options.GeneralOptions.TwitchOAuthToken,
                Options.GeneralOptions.TwitchChannel, Options.GeneralOptions.TwitchId);
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

            InitializeAutoTracker();
        }

        private void InitializeTracker()
        {
            if (Options == null)
                throw new InvalidOperationException("Cannot initialize Tracker before assigning " + nameof(Options));

            Tracker = _serviceProvider.GetRequiredService<Tracker>();
            Tracker.SpeechRecognized += (sender, e) => Dispatcher.Invoke(() =>
            {
                UpdateStats(e);
            });
            Tracker.ItemTracked += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = false;
                RefreshGridItems();
            });
            Tracker.ToggledPegWorldModeOn += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = Tracker.PegWorldMode;
                RefreshGridItems();
            });
            Tracker.PegPegged += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = Tracker.Pegs.Any(x => !x.Pegged);
                RefreshGridItems();
            });
            Tracker.DungeonUpdated += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = false;
                RefreshGridItems();
            });
            Tracker.BossUpdated += (sender, e) => Dispatcher.Invoke(() =>
            {
                _pegWorldMode = false;
                RefreshGridItems();
            });
            Tracker.GoModeToggledOn += (sender, e) => Dispatcher.Invoke(() =>
            {
                TrackerStatusBar.Background = Brushes.Green;
                StatusBarGoMode.Visibility = Visibility.Visible;
            });
            Tracker.ActionUndone += (sender, e) => Dispatcher.Invoke(() =>
            {
                if (!Tracker.GoMode)
                {
                    TrackerStatusBar.Background = null;
                    StatusBarGoMode.Visibility = Visibility.Collapsed;
                }

                RefreshGridItems();
            });
            Tracker.StateLoaded += (sender, e) => Dispatcher.Invoke(() =>
            {
                RefreshGridItems();
                ResetGridSize();
                TrackerStatusBar.Background = Tracker.GoMode ? Brushes.Green : null;
                StatusBarGoMode.Visibility = Tracker.GoMode ? Visibility.Visible : Visibility.Collapsed;
            });
            Tracker.MapUpdated += (sender, e) => Dispatcher.Invoke(() =>
            {
                _trackerMapWindow.UpdateMap(Tracker.CurrentMap);
            });
        }

        private ContextMenu CreateAutoTrackerMenu(bool enableAutoTracker)
        {
            var menu = new ContextMenu
            {
                Style = Application.Current.FindResource("DarkContextMenu") as Style
            };

            _autoTrackerDisableMenuItem = new MenuItem
            {
                Header = "Disable Auto Tracker",
                IsCheckable = true
            };
            _autoTrackerDisableMenuItem.Click += (sender, e) =>
            {
                Tracker.AutoTracker?.SetConnector(SMZ3.Tracking.AutoTracking.EmulatorConnectorType.None);
            };
            menu.Items.Add(_autoTrackerDisableMenuItem);

            _autoTrackerLuaMenuItem = new MenuItem
            {
                Header = "Lua Auto Tracker",
                IsCheckable = true
            };
            _autoTrackerLuaMenuItem.Click += (sender, e) =>
            {
                Tracker.AutoTracker?.SetConnector(SMZ3.Tracking.AutoTracking.EmulatorConnectorType.Lua);
            };
            menu.Items.Add(_autoTrackerLuaMenuItem);

            _autoTrackerUSB2SNESMenuItem = new MenuItem
            {
                Header = "USB2SNES Auto Tracker",
                IsCheckable = true
            };
            _autoTrackerUSB2SNESMenuItem.Click += (sender, e) =>
            {
                Tracker.AutoTracker?.SetConnector(SMZ3.Tracking.AutoTracking.EmulatorConnectorType.USB2SNES);
            };
            menu.Items.Add(_autoTrackerUSB2SNESMenuItem);

            var folder = new MenuItem
            {
                Header = "Show Auto Tracker Scripts Folder",
            };
            folder.Click += (sender, e) =>
            {
                var path = Options.AutoTrackerScriptsOutputPath;
                if (string.IsNullOrEmpty(path))
                {
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "AutoTrackerScripts");
                }
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            };
            menu.Items.Add(folder);

            var help = new MenuItem
            {
                Header = "Open Auto Tracker Help",
            };
            help.Click += (sender, e) =>
            {
                OpenAutoTrackerHelp();
            };
            menu.Items.Add(help);

            return menu;
        }

        public void InitializeAutoTracker()
        {
            var menu = CreateAutoTrackerMenu(true);
            StatusBarAutoTrackerEnabled.ContextMenu = menu;
            StatusBarAutoTrackerConnected.ContextMenu = menu;
            StatusBarAutoTrackerDisabled.ContextMenu = menu;

            if (Tracker.AutoTracker == null)
            {
                _logger.LogError("Auto tracker not found");
                return;
            }
            
            Tracker.AutoTracker.AutoTrackerEnabled += (sender, e) => Dispatcher.Invoke(() => UpdateAutoTrackerMenu());
            Tracker.AutoTracker.AutoTrackerDisabled += (sender, e) => Dispatcher.Invoke(() => UpdateAutoTrackerMenu());
            Tracker.AutoTracker.AutoTrackerConnected += (sender, e) => Dispatcher.Invoke(() => UpdateAutoTrackerMenu());
            Tracker.AutoTracker.AutoTrackerDisconnected += (sender, e) => Dispatcher.Invoke(() => UpdateAutoTrackerMenu());

            Tracker.AutoTracker.SetConnector(Options.AutoTrackerDefaultConnector);
        }

        private void UpdateAutoTrackerMenu()
        {
            if (Tracker.AutoTracker == null) return;
            StatusBarAutoTrackerDisabled.Visibility = !Tracker.AutoTracker.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            StatusBarAutoTrackerEnabled.Visibility = Tracker.AutoTracker.IsEnabled && !Tracker.AutoTracker.IsConnected ? Visibility.Visible : Visibility.Collapsed;
            StatusBarAutoTrackerConnected.Visibility = Tracker.AutoTracker.IsEnabled && Tracker.AutoTracker.IsConnected ? Visibility.Visible : Visibility.Collapsed;
            _autoTrackerDisableMenuItem.IsChecked = Tracker.AutoTracker?.ConnectorType == SMZ3.Tracking.AutoTracking.EmulatorConnectorType.None;
            _autoTrackerLuaMenuItem.IsChecked = Tracker.AutoTracker?.ConnectorType == SMZ3.Tracking.AutoTracking.EmulatorConnectorType.Lua;
            _autoTrackerUSB2SNESMenuItem.IsChecked = Tracker.AutoTracker?.ConnectorType == SMZ3.Tracking.AutoTracking.EmulatorConnectorType.USB2SNES;
        }

        private void UpdateStats(TrackerEventArgs e)
        {
            if (e.Confidence != null)
                StatusBarConfidence.Content = $"{e.Confidence:P2}";
            StatusBarRecognizedPhrase.ToolTip = $"“{e.Phrase}”";
            RecognizedPhraseText.Text = $"“{e.Phrase}”";
        }

        private void ResetGridSize()
        {
            var columns = Math.Max(_itemService.AllItems().Max(x => x.Column) ?? 0,
                Tracker.WorldInfo.Dungeons.Max(x => x.Column) ?? 0);

            TrackerGrid.ColumnDefinitions.Clear();
            for (var i = 0; i <= columns; i++)
                TrackerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridItemPx + GridItemMargin) });

            var rows = Math.Max(_itemService.AllItems().Max(x => x.Row) ?? 0,
                Tracker.WorldInfo.Dungeons.Max(x => x.Row) ?? 0);

            TrackerGrid.RowDefinitions.Clear();
            for (var i = 0; i <= rows; i++)
                TrackerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridItemPx + GridItemMargin) });
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Tracker.IsDirty)
            {
                if (MessageBox.Show("You have unsaved changes in your tracker. Do you want to save?", "SMZ3 Cas’ Randomizer",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await SaveState();
                }
            }
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

        private void AutoTrackerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenAutoTrackerHelp();
        }

        private async void LoadSavedStateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // If there is a valid rom, then load the state from the db
            if (GeneratedRom.IsValid(Rom))
            {
                Tracker.Load(Rom);
                Tracker.StartTimer(true);
                if (_dispatcherTimer.IsEnabled)
                {
                    _dispatcherTimer.Start();
                }
            }
            // Otherwise save it to a file
            else
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
        }

        private async Task SaveState()
        {
            // If there is a rom, save it to the database
            if (GeneratedRom.IsValid(Rom))
            {
                await Tracker.SaveAsync(Rom);
            }
            // Otherwise open the save file dialog
            else
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

            SavedState.Invoke(this, null);
        }

        private async void SaveStateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await SaveState();
        }

        private void StatusBarTimer_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Reset timer on double click
            Tracker.ResetTimer();
        }

        private void StatusBarTimer_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Pause/resume timer on right click
            if (!Tracker.IsTimerPaused)
            {
                Tracker.PauseTimer();
            }
            else
            {
                Tracker.StartTimer();
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

            try
            {
                Tracker.EnableVoiceRecognition();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error enabling voice recognition");
                ShowModuleWarning();
            }
            
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

        private void ShowModuleWarning()
        {
            MessageBox.Show(this, "There was a problem with loading one or more of the tracker modules.\n" +
                    "Some tracking functionality may be limited.", "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        /// <summary>
        /// Occurs when the the tracker's state has been saved
        /// </summary>
        public event EventHandler SavedState;

        protected void OpenAutoTrackerHelp()
        {
            if (Application.Current.Windows.OfType<AutoTrackerWindow>().Any())
            {
                _autoTrackerHelpWindow.Activate();
            }
            else
            {
                _autoTrackerHelpWindow = new AutoTrackerWindow();
                _autoTrackerHelpWindow.Show();
            }
        }
    }
}
