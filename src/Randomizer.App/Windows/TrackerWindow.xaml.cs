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
using Randomizer.SMZ3.Tracking.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;

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
        private readonly IWorldService _worldService;
        private readonly IUIService _uiService;
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
        private UILayout _layout;
        private UILayout _previousLayout;
        private RandomizerOptions _options;

        public TrackerWindow(IServiceProvider serviceProvider,
            IItemService itemService,
            ILogger<TrackerWindow> logger,
            RomGenerator romGenerator,
            IWorldService worldService,
            IUIService uiService,
            OptionsFactory optionsFactory
        )
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _itemService = itemService;
            _worldService = worldService;
            _logger = logger;
            _romGenerator = romGenerator;
            _uiService = uiService;
            _options = optionsFactory.Create();
            _layout = uiService.GetLayout(_options.GeneralOptions.SelectedLayout);

            foreach(var layout in uiService.SelectableLayouts)
            {
                var layoutMenuItem = new MenuItem();
                layoutMenuItem.Header = layout.Name;
                layoutMenuItem.Tag = layout;
                layoutMenuItem.Click += LayoutMenuItem_Click;
                layoutMenuItem.IsCheckable = true;
                layoutMenuItem.IsChecked = layout == _layout;
                LayoutMenu.Items.Add(layoutMenuItem);
            }

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
                    overlays.Add(new(_uiService.GetSpritePath(digit), offset, 0)
                    {
                        OriginPoint = Origin.BottomLeft
                    });
                    offset += 8;
                }
            }

            return GetGridItemControl(imageFileName, column, row, overlays.ToArray());
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
            if (imageFileName == null)
            {
                imageFileName = _uiService.GetSpritePath("Items", "blank.png", out _);
            }

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
            Grid.SetColumn(image, column - 1);
            Grid.SetRow(image, row - 1);
            return image;
        }

        protected virtual void RefreshGridItems()
        {
            TrackerGrid.Children.Clear();

            foreach (var gridLocation in _layout.GridLocations)
            {
                var labelImage = (Image)null;
                if (gridLocation.Image != null)
                {
                    labelImage = GetGridItemControl(_uiService.GetSpritePath("Items", gridLocation.Image, out _), gridLocation.Column, gridLocation.Row);
                    TrackerGrid.Children.Add(labelImage);
                }

                // A group of items stacked on top of each other
                if (gridLocation.Type == UIGridLocationType.Items)
                {
                    var items = new List<ItemData>();
                    Image latestImage = null;
                    foreach (var itemName in gridLocation.Identifiers)
                    {
                        var item = _itemService.FindOrDefault(itemName);
                        if (item == null)
                        {
                            _logger.LogError($"Item {itemName} could not be found");
                            continue;
                        }

                        items.Add(item);
                        var fileName = _uiService.GetSpritePath(item);
                        var overlay = GetOverlayImageFileName(item);
                        if (fileName == null)
                        {
                            _logger.LogError($"Image for {item.Item} could not be found");
                            continue;
                        }

                        latestImage = GetGridItemControl(fileName,
                            gridLocation.Column, gridLocation.Row,
                            item.Counter, overlay, minCounter: 2);
                        latestImage.Opacity = item.TrackingState > 0 ? 1.0d : 0.2d;
                        TrackerGrid.Children.Add(latestImage);
                    }

                    // If only one item, left clicking should track it
                    if (items.Count == 1)
                    {
                        latestImage.MouseLeftButtonDown += Image_MouseDown;
                        latestImage.MouseLeftButtonUp += Image_LeftClick;
                    }

                    if (labelImage != null)
                    {
                        labelImage.Opacity = items.Any(x => x.TrackingState > 0) ? 1.0d : 0.2d;
                    }

                    latestImage.Tag = gridLocation;
                    latestImage.ContextMenu = CreateContextMenu(items);
                }
                // If it's a Zelda dungeon
                else if (gridLocation.Type == UIGridLocationType.Dungeon)
                {
                    var dungeon = _worldService.Dungeon(gridLocation.Identifiers.First());
                    if (dungeon == null)
                    {
                        _logger.LogError($"Dungeon {gridLocation.Identifiers.First()} could not be found");
                        continue;
                    }

                    var overlayPath = _uiService.GetSpritePath(dungeon);
                    var rewardPath = dungeon.HasReward ? _uiService.GetSpritePath(dungeon.Reward) : null;
                    var image = GetGridItemControl(rewardPath,
                        gridLocation.Column, gridLocation.Row,
                        dungeon.TreasureRemaining, overlayPath, minCounter: 1);
                    image.Tag = gridLocation;
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;

                    if (dungeon.HasReward)
                    {
                        image.ContextMenu = CreateContextMenu(dungeon);
                    }

                    image.Opacity = dungeon.Cleared ? 1.0d : 0.2d;

                    TrackerGrid.Children.Add(image);
                }
                // If it's a Super Metroid boss
                else if (gridLocation.Type == UIGridLocationType.SMBoss)
                {
                    var boss = _worldService.Boss(gridLocation.Identifiers.First());
                    if (boss == null)
                        continue;

                    var fileName = _uiService.GetSpritePath(boss);
                    var overlay = GetOverlayImageFileName(boss);
                    if (fileName == null)
                    {
                        _logger.LogError($"Image for {boss.Boss} could not be found");
                        continue;
                    }

                    var image = GetGridItemControl(fileName,
                        gridLocation.Column, gridLocation.Row);
                    image.Tag = gridLocation;
                    image.ContextMenu = CreateContextMenu(boss);
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;
                    image.Opacity = boss.Defeated ? 1.0d : 0.2d;
                    TrackerGrid.Children.Add(image);
                }
                // If it's a hammer peg
                else if (gridLocation.Type == UIGridLocationType.Peg)
                {
                    int pegNumber = 0;
                    if (!int.TryParse(gridLocation.Identifiers.First(), out pegNumber))
                    {
                        _logger.LogError("Could not determine peg number");
                        continue;
                    }
                     
                    var fileName = _uiService.GetSpritePath("Items",
                        Tracker.PegsPegged >= pegNumber ? "pegged.png" : "peg.png", out _);

                    var image = GetGridItemControl(fileName, gridLocation.Column, gridLocation.Row);
                    image.Tag = gridLocation;
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;
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
                    return _uiService.GetSpritePath("Dungeons", $"{names[0]}.png", out _);
                }
                else if (names.Count > 1)
                {
                    return _uiService.GetSpritePath("Dungeons", "both.png", out _);
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
                if (image.Tag is UIGridLocation gridLocation)
                {
                    if (gridLocation.Type == UIGridLocationType.Items)
                    {
                        var item = _itemService.FindOrDefault(gridLocation.Identifiers.First());
                        Tracker.TrackItem(item);
                    }
                    else if (gridLocation.Type == UIGridLocationType.Dungeon)
                    {
                        var dungeon = _worldService.Dungeon(gridLocation.Identifiers.First());
                        Tracker.MarkDungeonAsCleared(dungeon);
                    }
                    else if (gridLocation.Type == UIGridLocationType.Peg)
                    {
                        Tracker.Peg();
                    }
                    else if (gridLocation.Type == UIGridLocationType.SMBoss)
                    {
                        var boss = _worldService.Boss(gridLocation.Identifiers.First());
                        Tracker.MarkBossAsDefeated(boss);
                    }
                    else
                    {
                        _logger.LogError("Unrecognized UIGridLocationType tag type {TagType}", gridLocation.Type);
                    }
                }
                else
                {
                    _logger.LogError("Unrecognized image tag type {TagType}", image.Tag.GetType());
                }
            };
        }

        private ContextMenu CreateContextMenu(ICollection<ItemData> items)
        {
            var menu = new ContextMenu
            {
                Style = Application.Current.FindResource("DarkContextMenu") as Style
            };

            foreach (var item in items)
            {
                if (item.TrackingState == 0 || item.Multiple)
                {
                    var menuItem = new MenuItem
                    {
                        Header = "Track " + item.Name[0],
                        Icon = new Image
                        {
                            Source = new BitmapImage(new Uri(_uiService.GetSpritePath(item)))
                        }
                    };
                    menuItem.Click += (sender, e) =>
                    {
                        Tracker.TrackItem(item);
                        RefreshGridItems();
                    };
                    menu.Items.Add(menuItem);
                }

                if (item.TrackingState > 0)
                {
                    var menuItem = new MenuItem
                    {
                        Header = "Untrack " + item.Name[0],
                        Icon = new Image
                        {
                            Source = new BitmapImage(new Uri(_uiService.GetSpritePath(item)))
                        }
                    };
                    menuItem.Click += (sender, e) =>
                    {
                        Tracker.UntrackItem(item);
                        RefreshGridItems();
                    };
                    menu.Items.Add(menuItem);
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

            return menu.Items.Count > 0 ? menu : null;
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

            if (dungeon.HasReward && dungeon.Type != typeof(CastleTower))
            {
                foreach (var reward in Enum.GetValues<RewardItem>().Where(x => x != RewardItem.Agahnim && x != RewardItem.NonGreenPendant))
                {
                    var item = new MenuItem
                    {
                        Header = $"Mark as {reward.GetDescription()}",
                        IsChecked = dungeon.Reward == reward,
                        Icon = new Image
                        {
                            Source = new BitmapImage(new Uri(_uiService.GetSpritePath(reward)))
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

            return menu.Items.Count > 0 ? menu : null;
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
                Options.SeedOptions.KeysanityMode = config.KeysanityMode;
                Options.SeedOptions.ItemPlacementRule = config.ItemPlacementRule;
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
                TogglePegWorld(false);
                RefreshGridItems();
            });
            Tracker.ToggledPegWorldModeOn += (sender, e) => Dispatcher.Invoke(() =>
            {
                TogglePegWorld(Tracker.PegWorldMode);
                RefreshGridItems();
            });
            Tracker.PegPegged += (sender, e) => Dispatcher.Invoke(() =>
            {
                TogglePegWorld(Tracker.PegsPegged < PegWorldModeModule.TotalPegs);
                RefreshGridItems();
            });
            Tracker.DungeonUpdated += (sender, e) => Dispatcher.Invoke(() =>
            {
                TogglePegWorld(false);
                RefreshGridItems();
            });
            Tracker.BossUpdated += (sender, e) => Dispatcher.Invoke(() =>
            {
                TogglePegWorld(false);
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

        private void TogglePegWorld(bool enable)
        {
            if (_pegWorldMode == enable) return;
            _pegWorldMode = enable;
            if (_pegWorldMode)
            {
                _previousLayout = _layout;
                _layout = _uiService.GetLayout("Peg World");
            }
            else
            {
                _layout = _previousLayout;
                _previousLayout = null;
            }
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
            var columns = _layout.GridLocations.Max(x => x.Column);

            TrackerGrid.ColumnDefinitions.Clear();
            for (var i = 0; i <= columns; i++)
                TrackerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridItemPx + GridItemMargin) });

            var rows = _layout.GridLocations.Max(x => x.Row);

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
                _locationsWindow = new TrackerLocationsWindow(_locationSyncer, _uiService);
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

        private void LayoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is UILayout layout)
            {
                 _layout = layout;
                _options.GeneralOptions.SelectedLayout = layout.Name;
                _options.Save();
                ResetGridSize();
                RefreshGridItems();
                foreach (var item in LayoutMenu.Items)
                {
                    var layoutMenuItem = item as MenuItem;
                    layoutMenuItem.IsChecked = layoutMenuItem.Tag == _layout;
                }
            };
        }
    }
}
