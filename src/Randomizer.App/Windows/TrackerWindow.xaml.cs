using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;

namespace Randomizer.App.Windows
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
        private readonly IUIService _uiService;
        private readonly List<object> _mouseDownSenders = new();
        private readonly IWorldAccessor _world;
        private readonly RandomizerOptions _options;
        private readonly Smz3GeneratedRomLoader _romLoader;
        private bool _pegWorldMode;
        private TrackerLocationsWindow? _locationsWindow;
        private TrackerHelpWindow? _trackerHelpWindow;
        private TrackerMapWindow? _trackerMapWindow;
        private AutoTrackerWindow? _autoTrackerHelpWindow;
        private TrackerLocationSyncer? _locationSyncer;
        private MenuItem? _autoTrackerDisableMenuItem;
        private MenuItem? _autoTrackerLuaMenuItem;
        private MenuItem? _autoTrackerUSB2SNESMenuItem;
        private UILayout _layout;
        private readonly UILayout _defaultLayout;
        private UILayout? _previousLayout;
        private Tracker? _tracker;

        public TrackerWindow(IServiceProvider serviceProvider,
            IItemService itemService,
            ILogger<TrackerWindow> logger,
            Smz3GeneratedRomLoader romLoader,
            IUIService uiService,
            OptionsFactory optionsFactory,
            IWorldAccessor world,
            ITrackerTimerService timerService
        )
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _itemService = itemService;
            _logger = logger;
            _romLoader = romLoader;
            _uiService = uiService;
            _options = optionsFactory.Create();
            _layout = uiService.GetLayout(_options.GeneralOptions.SelectedLayout);
            _defaultLayout = _layout;
            _world = world;

            foreach (var layout in uiService.SelectableLayouts)
            {
                var layoutMenuItem = new MenuItem
                {
                    Header = layout.Name,
                    Tag = layout
                };
                layoutMenuItem.Click += LayoutMenuItem_Click;
                layoutMenuItem.IsCheckable = true;
                layoutMenuItem.IsChecked = layout == _layout;
                LayoutMenu.Items.Add(layoutMenuItem);
            }

            _dispatcherTimer = new(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Render, (sender, _) =>
            {
                StatusBarTimer.Content = timerService.TimeString;
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

        public Tracker Tracker => _tracker ?? throw new InvalidOperationException("Tracker not created");

        public GeneratedRom? Rom { get; set; }

        /// <summary>
        /// Occurs when the the tracker's state has been saved
        /// </summary>
        public event EventHandler? SavedState;

        protected Image GetGridItemControl(string? imageFileName, int column, int row, string overlayFileName)
            => GetGridItemControl(imageFileName, column, row, new Overlay(overlayFileName, 0, 0));

        protected Image GetGridItemControl(string? imageFileName, int column, int row, int counter, string? overlayFileName, int minCounter = 2)
        {
            var overlays = new List<Overlay>();
            if (overlayFileName != null)
                overlays.Add(new(overlayFileName, 0, 0));

            if (counter >= minCounter)
            {
                var offset = 0;
                foreach (var digit in GetDigits(counter))
                {
                    var sprite = _uiService.GetSpritePath(digit);

                    if (sprite == null) continue;

                    overlays.Add(new(sprite, offset, 0)
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

        protected Image GetGridItemControl(string? imageFileName, int column, int row,
            params Overlay[] overlays)
        {
            imageFileName ??= _uiService.GetSpritePath("Items", "blank.png", out _);

            var bitmapImage = new BitmapImage(new Uri(imageFileName ?? ""));
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

            if (_options.GeneralOptions.TrackerShadows)
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
                var labelImage = (Image?)null;
                if (gridLocation.Image != null)
                {
                    labelImage = GetGridItemControl(_uiService.GetSpritePath("Items", gridLocation.Image, out _), gridLocation.Column, gridLocation.Row);
                    TrackerGrid.Children.Add(labelImage);
                }

                // A group of items stacked on top of each other
                if (gridLocation.Type == UIGridLocationType.Items)
                {
                    var items = new List<Item>();
                    var latestImage = (Image?)null;
                    foreach (var itemName in gridLocation.Identifiers)
                    {
                        var item = _itemService.FirstOrDefault(itemName);
                        if (item == null)
                        {
                            _logger.LogError("Item {ItemName} could not be found", itemName);
                            continue;
                        }

                        items.Add(item);
                        var fileName = _uiService.GetSpritePath(item);
                        var overlay = GetOverlayImageFileName(item);
                        if (fileName == null)
                        {
                            _logger.LogError("Image for {ItemName} could not be found", item.Name);
                            continue;
                        }

                        var counter = item.Counter;

                        // Group bottle counts together
                        if (item.Type == ItemType.Bottle)
                        {
                            var countedBottleTypes = _itemService.LocalPlayersItems()
                                .Where(x => x.Type != ItemType.Bottle && x.Type.IsInCategory(ItemCategory.Bottle))
                                .Select(x => x.Type).Distinct().ToList();
                            foreach (var type in countedBottleTypes)
                            {
                                counter += _itemService.FirstOrDefault(type)?.Counter ?? 0;
                            }
                        }

                        latestImage = GetGridItemControl(fileName,
                            gridLocation.Column, gridLocation.Row,
                            counter, overlay, minCounter: 2);
                        latestImage.Opacity = item.State.TrackingState > 0 || counter > 0 ? 1.0d : 0.2d;
                        TrackerGrid.Children.Add(latestImage);
                    }

                    if (latestImage == null) continue;

                    // If only one item, left clicking should track it
                    if (items.Count == 1)
                    {
                        latestImage.MouseLeftButtonDown += Image_MouseDown;
                        latestImage.MouseLeftButtonUp += Image_LeftClick;
                    }

                    if (labelImage != null)
                    {
                        labelImage.Opacity = items.Any(x => x.State.TrackingState > 0) ? 1.0d : 0.2d;
                    }

                    latestImage.Tag = gridLocation;
                    latestImage.ContextMenu = CreateContextMenu(items);
                }
                // If it's a Zelda dungeon
                else if (gridLocation.Type == UIGridLocationType.Dungeon)
                {
                    var dungeon = _world.World.Dungeons.FirstOrDefault(x => x.DungeonName == gridLocation.Identifiers.First());
                    if (dungeon == null)
                    {
                        _logger.LogError("Dungeon {DungeonName} could not be found", gridLocation.Identifiers.First());
                        continue;
                    }

                    var overlayPath = _uiService.GetSpritePath(dungeon);
                    var rewardPath = dungeon.MarkedReward != RewardType.None ? _uiService.GetSpritePath(dungeon.MarkedReward) : null;
                    var image = GetGridItemControl(rewardPath,
                        gridLocation.Column, gridLocation.Row,
                        dungeon.DungeonState.RemainingTreasure, overlayPath, minCounter: 1);
                    image.Tag = gridLocation;
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;

                    if (dungeon.HasReward)
                    {
                        image.ContextMenu = CreateContextMenu(dungeon);
                    }

                    image.Opacity = dungeon.DungeonState.Cleared ? 1.0d : 0.2d;

                    TrackerGrid.Children.Add(image);
                }
                // If it's a Super Metroid boss
                else if (gridLocation.Type == UIGridLocationType.SMBoss)
                {
                    var boss = _world.World.AllBosses.FirstOrDefault(x => x.Name == gridLocation.Identifiers.First());
                    if (boss == null)
                        continue;

                    var fileName = _uiService.GetSpritePath(boss.Metadata);
                    var overlay = GetOverlayImageFileName(boss.Metadata);
                    if (fileName == null)
                    {
                        _logger.LogError("Image for {BossName} could not be found", boss.Name);
                        continue;
                    }

                    var image = GetGridItemControl(fileName,
                        gridLocation.Column, gridLocation.Row);
                    image.Tag = gridLocation;
                    image.ContextMenu = CreateContextMenu(boss);
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;
                    image.Opacity = boss.State.Defeated ? 1.0d : 0.2d;
                    TrackerGrid.Children.Add(image);
                }
                // If it's a hammer peg
                else if (gridLocation.Type == UIGridLocationType.Peg)
                {
                    if (!int.TryParse(gridLocation.Identifiers.First(), out var pegNumber))
                    {
                        _logger.LogError("Could not determine peg number");
                        continue;
                    }

                    var fileName = _uiService.GetSpritePath("Items",
                        Tracker?.PegsPegged >= pegNumber ? "pegged.png" : "peg.png", out _);

                    var image = GetGridItemControl(fileName, gridLocation.Column, gridLocation.Row);
                    image.Tag = gridLocation;
                    image.MouseLeftButtonDown += Image_MouseDown;
                    image.MouseLeftButtonUp += Image_LeftClick;
                    TrackerGrid.Children.Add(image);
                }
            }
        }

        private string? GetOverlayImageFileName(BossInfo boss)
        {
            return null;
        }

        private string? GetOverlayImageFileName(Item item)
        {
            return item switch
            {
                { Type: ItemType.Bombos } => GetMatchingDungeonNameImages(item.Type),
                { Type: ItemType.Ether } => GetMatchingDungeonNameImages(item.Type),
                { Type: ItemType.Quake } => GetMatchingDungeonNameImages(item.Type),
                _ => null
            };

            string? GetMatchingDungeonNameImages(ItemType requirement)
            {
                var names = Tracker.World.Dungeons.Where(x => x.DungeonState.MarkedMedallion == requirement)
                    .Select(x => x.DungeonName)
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
                        var item = _itemService.FirstOrDefault(gridLocation.Identifiers.First());
                        if (item != null) Tracker.TrackItem(item);
                    }
                    else if (gridLocation.Type == UIGridLocationType.Dungeon)
                    {
                        var dungeon = _world.World.Dungeons.First(x => x.DungeonName == gridLocation.Identifiers.First());
                        Tracker.MarkDungeonAsCleared(dungeon);
                    }
                    else if (gridLocation.Type == UIGridLocationType.Peg)
                    {
                        Tracker.Peg();
                    }
                    else if (gridLocation.Type == UIGridLocationType.SMBoss)
                    {
                        var boss = _world.World.AllBosses.First(x => x.Name == gridLocation.Identifiers.First());
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
            }
        }

        private ContextMenu? CreateContextMenu(ICollection<Item> items)
        {
            var menu = new ContextMenu
            {
                Style = Application.Current.FindResource("DarkContextMenu") as Style
            };

            foreach (var item in items)
            {
                var sprite = _uiService.GetSpritePath(item);
                if (sprite == null) continue;

                if (item.State.TrackingState == 0 || item.Metadata.Multiple)
                {
                    var menuItem = new MenuItem
                    {
                        Header = "Track " + item.Name,
                        Icon = new Image
                        {
                            Source = new BitmapImage(new Uri(sprite))
                        }
                    };
                    menuItem.Click += (sender, e) =>
                    {
                        Tracker.TrackItem(item);
                        RefreshGridItems();
                    };
                    menu.Items.Add(menuItem);
                }

                if (item.State.TrackingState > 0)
                {
                    var menuItem = new MenuItem
                    {
                        Header = "Untrack " + item.Name,
                        Icon = new Image
                        {
                            Source = new BitmapImage(new Uri(sprite))
                        }
                    };
                    menuItem.Click += (sender, e) =>
                    {
                        Tracker.UntrackItem(item);
                        RefreshGridItems();
                    };
                    menu.Items.Add(menuItem);
                }

                if (item.Type is ItemType.Bombos or ItemType.Ether or ItemType.Quake)
                {
                    var medallion = item.Type;
                    var turtleRock = Tracker.World.TurtleRock as IDungeon;
                    var miseryMire = Tracker.World.MiseryMire as IDungeon;

                    var requiredByNone = new MenuItem
                    {
                        Header = "Not required for any dungeon",
                        IsChecked = turtleRock.MarkedMedallion != medallion && miseryMire.MarkedMedallion != medallion
                    };
                    requiredByNone.Click += (sender, e) =>
                    {
                        if (turtleRock.MarkedMedallion == medallion)
                            turtleRock.MarkedMedallion = ItemType.Nothing;
                        if (miseryMire.MarkedMedallion == medallion)
                            miseryMire.MarkedMedallion = ItemType.Nothing;
                        _locationSyncer?.OnLocationUpdated("");
                        RefreshGridItems();
                    };

                    var requiredByTR = new MenuItem
                    {
                        Header = "Required for Turtle Rock",
                        IsChecked = turtleRock.Medallion == medallion && miseryMire.Medallion != medallion
                    };
                    requiredByTR.Click += (sender, e) =>
                    {
                        turtleRock.MarkedMedallion = medallion;
                        if (miseryMire.MarkedMedallion == medallion)
                            miseryMire.MarkedMedallion = ItemType.Nothing;
                        _locationSyncer?.OnLocationUpdated("");
                        RefreshGridItems();
                    };

                    var requiredByMM = new MenuItem
                    {
                        Header = "Required for Misery Mire",
                        IsChecked = turtleRock.MarkedMedallion != medallion && miseryMire.MarkedMedallion == medallion
                    };
                    requiredByMM.Click += (sender, e) =>
                    {
                        if (turtleRock.MarkedMedallion == medallion)
                            turtleRock.MarkedMedallion = ItemType.Nothing;
                        miseryMire.MarkedMedallion = medallion;
                        _locationSyncer?.OnLocationUpdated("");
                        RefreshGridItems();
                    };

                    var requiredByBoth = new MenuItem
                    {
                        Header = "Required by both",
                        IsChecked = turtleRock.MarkedMedallion == medallion && miseryMire.MarkedMedallion == medallion
                    };
                    requiredByBoth.Click += (sender, e) =>
                    {
                        turtleRock.MarkedMedallion = medallion;
                        miseryMire.MarkedMedallion = medallion;
                        _locationSyncer?.OnLocationUpdated("");
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

        private ContextMenu? CreateContextMenu(Boss boss)
        {
            var menu = new ContextMenu
            {
                Style = Application.Current.FindResource("DarkContextMenu") as Style
            };

            if (boss.State.Defeated)
            {
                var unclear = new MenuItem
                {
                    Header = $"Revive {boss.Name}",
                };
                unclear.Click += (sender, e) =>
                {
                    Tracker.MarkBossAsNotDefeated(boss);
                };
                menu.Items.Add(unclear);
            }

            return menu.Items.Count > 0 ? menu : null;
        }

        private ContextMenu? CreateContextMenu(IDungeon dungeon)
        {
            var menu = new ContextMenu
            {
                Style = Application.Current.FindResource("DarkContextMenu") as Style
            };

            if (dungeon.DungeonState.Cleared)
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

            if (dungeon.HasReward && dungeon.GetType() != typeof(CastleTower))
            {
                foreach (var reward in Enum.GetValues<RewardType>().Where(x => x != RewardType.Agahnim))
                {
                    var sprite = _uiService.GetSpritePath(reward);
                    if (string.IsNullOrEmpty(sprite) || dungeon.DungeonState == null) continue;

                    var item = new MenuItem
                    {
                        Header = $"Mark as {reward.GetDescription()}",
                        IsChecked = dungeon.DungeonState.MarkedReward == reward,
                        Icon = new Image
                        {
                            Source = new BitmapImage(new Uri(sprite))
                        }
                    };

                    item.Click += (sender, e) =>
                    {
                        dungeon.DungeonState.MarkedReward = reward;
                        RefreshGridItems();
                    };
                    menu.Items.Add(item);
                }
            }

            return menu.Items.Count > 0 ? menu : null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Background = new SolidColorBrush(Color.FromArgb(_options.GeneralOptions.TrackerBGColor[0], _options.GeneralOptions.TrackerBGColor[1], _options.GeneralOptions.TrackerBGColor[2], _options.GeneralOptions.TrackerBGColor[3]));

            // If a rom was passed in, generate its seed to populate all locations and items
            if (GeneratedRom.IsValid(Rom))
            {
                _romLoader.LoadGeneratedRom(Rom);
            }

            InitializeTracker();
            ResetGridSize();
            RefreshGridItems();

            if (!Tracker.TryStartTracking())
            {
                ShowModuleWarning();
            }

            Tracker.ConnectToChat(_options.GeneralOptions.TwitchUserName, _options.GeneralOptions.TwitchOAuthToken,
                _options.GeneralOptions.TwitchChannel, _options.GeneralOptions.TwitchId);
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
            if (_options == null)
                throw new InvalidOperationException("Cannot initialize Tracker before assigning " + nameof(_options));

            _tracker = _serviceProvider.GetRequiredService<Tracker>();

            // If a rom was passed in with a valid tracker state, reload the state from the database
            if (GeneratedRom.IsValid(Rom))
            {
                Tracker.Load(Rom);
            }

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
                _trackerMapWindow?.UpdateMap(Tracker.CurrentMap);
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
                _layout = _previousLayout ?? _defaultLayout;
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
                Tracker.AutoTracker?.SetConnector(EmulatorConnectorType.None);
            };
            menu.Items.Add(_autoTrackerDisableMenuItem);

            _autoTrackerLuaMenuItem = new MenuItem
            {
                Header = "Lua Auto Tracker",
                IsCheckable = true
            };
            _autoTrackerLuaMenuItem.Click += (sender, e) =>
            {
                Tracker.AutoTracker?.SetConnector(EmulatorConnectorType.Lua);
            };
            menu.Items.Add(_autoTrackerLuaMenuItem);

            _autoTrackerUSB2SNESMenuItem = new MenuItem
            {
                Header = "USB2SNES Auto Tracker",
                IsCheckable = true
            };
            _autoTrackerUSB2SNESMenuItem.Click += (sender, e) =>
            {
                Tracker.AutoTracker?.SetConnector(EmulatorConnectorType.USB2SNES);
            };
            menu.Items.Add(_autoTrackerUSB2SNESMenuItem);

            var folder = new MenuItem
            {
                Header = "Show Auto Tracker Scripts Folder",
            };
            folder.Click += (sender, e) =>
            {
                var path = _options.AutoTrackerScriptsOutputPath;
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

            Tracker.AutoTracker.AutoTrackerEnabled += (sender, e) => Dispatcher.Invoke(UpdateAutoTrackerMenu);
            Tracker.AutoTracker.AutoTrackerDisabled += (sender, e) => Dispatcher.Invoke(UpdateAutoTrackerMenu);
            Tracker.AutoTracker.AutoTrackerConnected += (sender, e) => Dispatcher.Invoke(UpdateAutoTrackerMenu);
            Tracker.AutoTracker.AutoTrackerDisconnected += (sender, e) => Dispatcher.Invoke(UpdateAutoTrackerMenu);

            Tracker.AutoTracker.SetConnector(_options.AutoTrackerDefaultConnector);
        }

        private void UpdateAutoTrackerMenu()
        {
            if (Tracker.AutoTracker == null) return;
            StatusBarAutoTrackerDisabled.Visibility = !Tracker.AutoTracker.IsEnabled ? Visibility.Visible : Visibility.Collapsed;
            StatusBarAutoTrackerEnabled.Visibility = Tracker.AutoTracker.IsEnabled && !Tracker.AutoTracker.IsConnected ? Visibility.Visible : Visibility.Collapsed;
            StatusBarAutoTrackerConnected.Visibility = Tracker.AutoTracker.IsEnabled && Tracker.AutoTracker.IsConnected ? Visibility.Visible : Visibility.Collapsed;
            if (_autoTrackerDisableMenuItem != null)
                _autoTrackerDisableMenuItem.IsChecked =
                    Tracker.AutoTracker?.ConnectorType == EmulatorConnectorType.None;
            if (_autoTrackerLuaMenuItem != null)
                _autoTrackerLuaMenuItem.IsChecked =
                    Tracker.AutoTracker?.ConnectorType == EmulatorConnectorType.Lua;
            if (_autoTrackerUSB2SNESMenuItem != null)
                _autoTrackerUSB2SNESMenuItem.IsChecked =
                    Tracker.AutoTracker?.ConnectorType == EmulatorConnectorType.USB2SNES;
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

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Tracker.IsDirty)
            {
                if (Tracker.World.Config.MultiWorld)
                {
                    await SaveStateAsync();
                }
                else if (MessageBox.Show("You have unsaved changes in your tracker. Do you want to save?", "SMZ3 Cas’ Randomizer",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    await SaveStateAsync();
                }
            }
            Tracker.StopTracking();
            _dispatcherTimer.Stop();
            App.SaveWindowPositionAndSize(this);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _locationsWindow?.Close();
            _locationsWindow = null;
            _trackerMapWindow?.Close();
            _trackerMapWindow = null;
            _tracker?.Dispose();
        }

        private void LocationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_locationsWindow != null && Application.Current.Windows.OfType<TrackerLocationsWindow>().Any())
            {
                _locationsWindow.Activate();
            }
            else if (_locationSyncer != null)
            {
                _locationsWindow = new TrackerLocationsWindow(_locationSyncer, _uiService);
                _locationsWindow.Show();
            }
            else
            {
                ShowErrorWindow("Unable to open locations window.");
            }
        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_trackerHelpWindow != null && Application.Current.Windows.OfType<TrackerHelpWindow>().Any())
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
                await Task.Run(() => Tracker.Load(Rom));

                Tracker.StartTimer(true);
                if (_dispatcherTimer.IsEnabled)
                {
                    _dispatcherTimer.Start();
                }
            }
            else
            {
                ShowErrorWindow("Could not save tracker state.");
            }
        }

        private async Task SaveStateAsync()
        {
            // If there is a rom, save it to the database
            if (GeneratedRom.IsValid(Rom))
            {
                await Tracker.SaveAsync(Rom);
            }

            SavedState?.Invoke(this, EventArgs.Empty);
        }

        private async void SaveStateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await SaveStateAsync();
        }

        private void StatusBarTimer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Reset timer on double click
            Tracker.ResetTimer();
        }

        private void StatusBarTimer_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Pause/resume timer on right click
            Tracker.ToggleTimer();
        }

        /// <summary>
        /// Double clicking on the status bar icon to disable voice recognition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusBarStatusBarConfidence_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
        private void StatusBarVoiceDisabled_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

        private void ShowErrorWindow(string baseErrorMessage)
        {
            var logFileLocation = Environment.ExpandEnvironmentVariables("%LocalAppData%\\SMZ3CasRandomizer");
            MessageBox.Show($"{baseErrorMessage}\n\n" +
                $"Please try again. If the problem persists, please see the log files in '{logFileLocation}' and " +
                "post them in Discord or on GitHub at https://github.com/Vivelin/SMZ3Randomizer/issues.", "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MapMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_trackerMapWindow != null && Application.Current.Windows.OfType<TrackerMapWindow>().Any())
            {
                _trackerMapWindow.Activate();
            }
            else if (_locationSyncer != null)
            {
                var scope = _serviceProvider.CreateScope();
                _trackerMapWindow = scope.ServiceProvider.GetRequiredService<TrackerMapWindow>();
                _trackerMapWindow.Syncer = _locationSyncer;
                _trackerMapWindow.Show();
            }
            else
            {
                ShowErrorWindow("Unable to open map window.");
            }
        }

        protected record Overlay(string FileName, int X, int Y)
        {
            public Origin OriginPoint { get; init; }
        }

        protected void OpenAutoTrackerHelp()
        {
            if (_autoTrackerHelpWindow != null && Application.Current.Windows.OfType<AutoTrackerWindow>().Any())
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
            if (sender is MenuItem { Tag: UILayout layout })
            {
                 _layout = layout;
                _options.GeneralOptions.SelectedLayout = layout.Name;
                _options.Save();
                ResetGridSize();
                RefreshGridItems();
                foreach (var layoutMenuItem in LayoutMenu.Items.OfType<MenuItem>())
                {
                    layoutMenuItem.IsChecked = layoutMenuItem.Tag == _layout;
                }
            }
        }
    }
}
