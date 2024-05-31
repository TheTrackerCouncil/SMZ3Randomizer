using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.Abstractions;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.CrossPlatform.Views;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;
using SnesConnectorLibrary;

namespace Randomizer.CrossPlatform.Services;

public class TrackerWindowService(
    TrackerBase tracker,
    IUIService uiService,
    IItemService itemService,
    OptionsFactory optionsFactory,
    IWorldAccessor world,
    ITrackerTimerService trackerTimerService,
    IServiceProvider serviceProvider,
    ILogger<TrackerWindowService> logger) : ControlService
{
    private RandomizerOptions? _options;
    private TrackerWindow _window = null!;
    private readonly DispatcherTimer _dispatcherTimer = new();
    private readonly TrackerWindowViewModel _model = new();
    private readonly Dictionary<string, TrackerWindowBossPanelViewModel> _bossModels = new();
    private readonly Dictionary<string, TrackerWindowDungeonPanelViewModel> _dungeonModels = new();
    private readonly Dictionary<string, TrackerWindowItemPanelViewModel> _itemModels = new();
    private readonly List<TrackerWindowItemPanelViewModel> _medallions = new();
    private TrackerMapWindow? _trackerMapWindow;
    private TrackerLocationsWindow? _trackerLocationsWindow;
    private TrackerHelpWindow? _trackerHelpWindow;
    private UILayout? _defaultLayout;
    private Dictionary<int, TrackerWindowPanelViewModel> _pegWorldImages = new();

    public TrackerWindowViewModel GetViewModel(TrackerWindow parent)
    {
        _window = parent;

        _model.Layouts = uiService.SelectableLayouts;

        var bytes = Options.GeneralOptions.TrackerBGColor;
        _model.Background = new SolidColorBrush(Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]));
        _model.OpenTrackWindow = Options.GeneralOptions.DisplayMsuTrackWindow;

        LocationViewModel.KeyImage = uiService.GetSpritePath("Items", "key.png", out _);
        RegionViewModel.ChestImage = uiService.GetSpritePath("Items", "chest.png", out _);

        _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
        _dispatcherTimer.Tick += (_, _) => _model.TimeString = trackerTimerService.TimeString;
        _dispatcherTimer.Start();

        tracker.BossUpdated += (_, args) =>
        {
            if (args.Boss == null)
            {
                return;
            }

            if (_bossModels.TryGetValue(args.Boss.Name, out var bossPanel))
            {
                bossPanel.BossDefeated = args.Boss.State.Defeated;
            }
        };

        tracker.DungeonUpdated += (_, args) =>
        {
            if (args.Dungeon == null)
            {
                return;
            }

            if (_dungeonModels.TryGetValue(args.Dungeon.DungeonName, out var dungeonPanel))
            {
                var rewardImage = args.Dungeon.MarkedReward != RewardType.None
                    ? uiService.GetSpritePath(args.Dungeon.MarkedReward)
                    : null;
                dungeonPanel.RewardImage = rewardImage;
                dungeonPanel.DungeonCleared = args.Dungeon.DungeonState.Cleared;
                dungeonPanel.DungeonTreasure = args.Dungeon.DungeonState.RemainingTreasure;
            }

            if (args.Dungeon.NeedsMedallion)
            {
                foreach (var medallion in _medallions)
                {
                    var item = medallion.Items?.Keys.FirstOrDefault();
                    medallion.IsMMRequirement = world.World.MiseryMire.DungeonState.MarkedMedallion == item?.Type;
                    medallion.IsTRRequirement = world.World.TurtleRock.DungeonState.MarkedMedallion == item?.Type;
                }
            }
        };

        tracker.ItemTracked += (_, args) =>
        {
            if (args.Item == null)
            {
                foreach (var item in itemService.LocalPlayersItems())
                {
                    if (_itemModels.TryGetValue(item.Name, out var itemsPanel))
                    {
                        var itemPath = uiService.GetSpritePath(item);
                        itemsPanel.UpdateItem(item, itemPath);
                    }
                }
            }
            else
            {
                if (_itemModels.TryGetValue(args.Item.Name, out var itemsPanel))
                {
                    var itemPath = uiService.GetSpritePath(args.Item);
                    itemsPanel.UpdateItem(args.Item, itemPath);
                }
            }

            if (_model.PegWorldMode)
            {
                TogglePegWorld(false);
            }
        };

        tracker.GoModeToggledOn += (sender, args) =>
        {
            _model.IsInGoMode = true;
        };

        tracker.GoModeToggledOff += (sender, args) =>
        {
            _model.IsInGoMode = false;
        };

        tracker.SpeechRecognized += (sender, args) =>
        {
            _model.SpeechConfidence = $"{args.Confidence:P2}";
            _model.SpeechPhrase = $"“{args.Phrase}”";
        };

        tracker.VoiceRecognitionEnabledChanged += (sender, args) =>
        {
            _model.VoiceEnabled = tracker.VoiceRecognitionEnabled;
            if (!_model.VoiceEnabled)
            {
                _model.SpeechConfidence = "Voice Disabled";
                _model.SpeechPhrase = "";
            }
            else
            {
                _model.SpeechConfidence = "";
                _model.SpeechPhrase = "";
            }
        };

        tracker.ToggledPegWorldModeOn += (sender, args) =>
        {
            TogglePegWorld(tracker.PegWorldMode);
        };

        tracker.PegPegged += (sender, args) =>
        {
            UpdatePegs();
        };

        tracker.ToggledShaktoolMode += (sender, args) =>
        {
            ToggleShaktoolMode(tracker.ShaktoolMode);
        };

        tracker.DisableVoiceRecognition();

        if (TrackerWindowPanelViewModel.NumberImagePaths.Count == 0)
        {
            for (var i = 0; i < 10; i++)
            {
                var sprite = uiService.GetSpritePath(i);
                if (!string.IsNullOrEmpty(sprite))
                {
                    TrackerWindowPanelViewModel.NumberImagePaths[i] = sprite;
                }
            }

            var localWorld = world.World;
            TrackerWindowItemPanelViewModel.RequirementImages[ItemDungeonRequirement.MM] =
                uiService.GetSpritePath("Dungeons", $"{localWorld.MiseryMire.Name} wide.png".ToLower(), out _) ?? "";
            TrackerWindowItemPanelViewModel.RequirementImages[ItemDungeonRequirement.TR] =
                uiService.GetSpritePath("Dungeons", $"{localWorld.TurtleRock.Name} wide.png".ToLower(), out _) ?? "";
            TrackerWindowItemPanelViewModel.RequirementImages[ItemDungeonRequirement.Both] =
                uiService.GetSpritePath("Dungeons", $"both wide.png", out _) ?? "";
        }

        SetLayout(uiService.SelectableLayouts.FirstOrDefault(x => x.Name == Options.GeneralOptions.SelectedLayout) ??
                  uiService.SelectableLayouts.First());

        return _model;
    }

    public event EventHandler? LayoutSet;

    public void OpenTrackerMapWindow()
    {
        if (_trackerMapWindow != null)
        {
            return;
        }

        _trackerMapWindow = serviceProvider.GetRequiredService<TrackerMapWindow>();
        _trackerMapWindow.Show(_window);
        _trackerMapWindow.Closed += (_, _) => _trackerMapWindow = null;
    }

    public void OpenTrackerLocationsWindow()
    {
        if (_trackerLocationsWindow != null)
        {
            return;
        }

        _trackerLocationsWindow = serviceProvider.GetRequiredService<TrackerLocationsWindow>();
        _trackerLocationsWindow.Show(_window);
        _trackerLocationsWindow.OutOfLogicChanged += TrackerLocationsWindowOnOutOfLogicChanged;
        _trackerLocationsWindow.Closed += (_, _) =>
        {
            _trackerLocationsWindow.OutOfLogicChanged -= TrackerLocationsWindowOnOutOfLogicChanged;
            _trackerLocationsWindow = null;
        };
    }

    private void TrackerLocationsWindowOnOutOfLogicChanged(object? sender, OutOfLogicChangedEventArgs e)
    {
        _trackerMapWindow?.UpdateShowOutOfLogic(e.ShowOutOfLogic);
    }

    public void SetRom(GeneratedRom rom)
    {
        if (!GeneratedRom.IsValid(rom))
        {
            return;
        }

        _model.Rom = rom;
        var romPath = Path.Combine(Options.RomOutputPath, rom.RomPath);
        tracker.Load(rom, romPath);
    }

    public void StartTracker()
    {
        if (!tracker.TryStartTracking())
        {
            _ = OpenMessageWindow("There was a problem with loading one or more of the tracker modules.\n" +
                              "Some tracking functionality may be limited.", MessageWindowIcon.Warning);
        }

        if (tracker.AutoTracker == null)
        {
            logger.LogError("Auto tracker not initialized");
            return;
        }

        tracker.ConnectToChat(Options.GeneralOptions.TwitchUserName, Options.GeneralOptions.TwitchOAuthToken,
            Options.GeneralOptions.TwitchChannel, Options.GeneralOptions.TwitchId);

        _model.AutoTrackerConnected = tracker.AutoTracker.IsConnected;
        _model.AutoTrackerEnabled = tracker.AutoTracker.IsEnabled;

        tracker.AutoTracker.AutoTrackerEnabled += AutoTrackerOnAutoTrackerEnabled;
        tracker.AutoTracker.AutoTrackerDisabled += AutoTrackerOnAutoTrackerEnabled;
        tracker.AutoTracker.AutoTrackerConnected += AutoTrackerOnAutoTrackerEnabled;
        tracker.AutoTracker.AutoTrackerDisconnected += AutoTrackerOnAutoTrackerEnabled;
        tracker.AutoTracker.AutoTrackerConnectorChanged += AutoTrackerOnAutoTrackerConnectorChanged;

        tracker.AutoTracker.SetConnector(Options.GeneralOptions.SnesConnectorSettings);
    }

    private void AutoTrackerOnAutoTrackerConnectorChanged(object? sender, EventArgs e)
    {
        _model.SnesConnectorType = tracker.AutoTracker!.ConnectorType;
    }

    private void AutoTrackerOnAutoTrackerEnabled(object? sender, EventArgs e)
    {
        _model.AutoTrackerConnected = tracker.AutoTracker!.IsConnected;
        _model.AutoTrackerEnabled = tracker.AutoTracker.IsEnabled;
        _model.SnesConnectorType = tracker.AutoTracker.ConnectorType;
    }

    private void TogglePegWorld(bool enable)
    {
        if (_model.PegWorldMode == enable) return;
        _model.PegWorldMode = enable;
        if (enable)
        {
            _model.PrevLayout = _model.CurrentLayout;
            SetLayout(uiService.GetLayout("Peg World"));
        }
        else
        {
            SetLayout(_model.PrevLayout ?? _defaultLayout!);
            _model.PrevLayout = null;
        }
    }

    private void UpdatePegs()
    {
        var peggedFileName = uiService.GetSpritePath("Items", "pegged.png", out _);
        var unpeggedFileName = uiService.GetSpritePath("Items", "peg.png", out _);
        for (var i = 1; i <= PegWorldModeModule.TotalPegs; i++)
        {
            _pegWorldImages[i].Images =
            [
                new TrackerWindowPanelImage()
                {
                    ImagePath = i <= tracker.PegsPegged ? peggedFileName! : unpeggedFileName!,
                    IsActive = true
                }
            ];
        }
    }

    private void ToggleShaktoolMode(bool enable)
    {
        if (_model.ShaktoolMode == enable) return;
        _model.ShaktoolMode = enable;
        if (enable)
        {
            _model.PrevLayout = _model.CurrentLayout;
            SetLayout(uiService.GetLayout("Shak"));
        }
        else
        {
            SetLayout(_model.PrevLayout ?? _defaultLayout!);
            _model.PrevLayout = null;
        }
    }

    public async Task LoadRomAsync()
    {
        // If there is a valid rom, then load the state from the db
        if (GeneratedRom.IsValid(_model.Rom))
        {
            var romPath = Path.Combine(Options.RomOutputPath, _model.Rom.RomPath);
            await Task.Run(() => tracker.Load(_model.Rom, romPath));

            tracker.StartTimer(true);
            if (_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Start();
            }
        }
        else
        {
            await DisplayError("Could not save tracker state.");
        }
    }

    public async Task SaveStateAsync()
    {
        // If there is a rom, save it to the database
        if (GeneratedRom.IsValid(tracker.Rom))
        {
            await tracker.SaveAsync();
        }
    }

    public async Task Shutdown()
    {
        if (tracker.IsDirty)
        {
            if (tracker.World.Config.MultiWorld)
            {
                await SaveStateAsync();
            }
            else
            {
                var response = await OpenMessageWindow("You have unsaved changes in your tracker. Do you want to save?",
                    MessageWindowIcon.Warning, MessageWindowButtons.YesNo);

                if (response?.PressedAcceptButton == true)
                {
                    await SaveStateAsync();
                }
            }
        }

        _trackerMapWindow?.Close();
        _trackerLocationsWindow?.Close();
        tracker.StopTracking();
        _dispatcherTimer.Stop();
        _window.Close();
    }

    public void SetLayout(UILayout layout, bool automatic = false)
    {
        _bossModels.Clear();
        _dungeonModels.Clear();
        _itemModels.Clear();
        _medallions.Clear();

        _model.LayoutName = layout.Name;
        _model.CurrentLayout = layout;

        var models = new List<TrackerWindowPanelViewModel>();

        foreach (var spot in layout.GridLocations)
        {
            var panel = GetPanelViewModel(spot);
            if (panel != null)
            {
                models.Add(panel);
            }
        }

        _model.Panels = models;

        if (!automatic)
        {
            ITaskService.Run(() =>
            {
                Options.GeneralOptions.SelectedLayout = layout.Name;
                Options.Save();
            });
        }

        LayoutSet?.Invoke(this, EventArgs.Empty);
    }


    public void ToggleTimer()
    {
        tracker.ToggleTimer();
    }

    public void ResetTimer()
    {
        tracker.ResetTimer();
    }

    public void SetConnector(SnesConnectorType snesConnectorType)
    {
        tracker.AutoTracker?.SetConnector(Options.GeneralOptions.SnesConnectorSettings, snesConnectorType);
    }

    public void OpenAutoTrackerFolder()
    {
        var path = Options.AutoTrackerScriptsOutputPath;
        if (string.IsNullOrEmpty(path))
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "AutoTrackerScripts");
        }

        CrossPlatformTools.OpenDirectory(path);
    }

    public void OpenTrackerHelpWindow()
    {
        if (_trackerHelpWindow != null)
        {
            return;
        }

        _trackerHelpWindow = serviceProvider.GetRequiredService<TrackerHelpWindow>();
        _trackerHelpWindow.Show(_window);
        _trackerHelpWindow.Closed += (_, _) => _trackerHelpWindow = null;
    }

    private TrackerWindowPanelViewModel? GetPanelViewModel(UIGridLocation? gridLocation)
    {
        return gridLocation?.Type switch
        {
            UIGridLocationType.Items => GetItemPanelViewModel(gridLocation),
            UIGridLocationType.Dungeon => GetDungeonPanelViewModel(gridLocation),
            UIGridLocationType.SMBoss => GetBossPanelViewModel(gridLocation),
            UIGridLocationType.Peg => GetPegPanelViewModel(gridLocation),
            UIGridLocationType.Shak => GetShakPanelViewModel(gridLocation),
            _ => null
        };
    }

    private TrackerWindowPanelViewModel GetItemPanelViewModel(UIGridLocation gridLocation)
    {
        var items = new Dictionary<Item, string>();
        List<Item>? connectedItems = null;

        var labelImage = uiService.GetSpritePath("Items", gridLocation.Image ?? "", out _);

        foreach (var itemName in gridLocation.Identifiers)
        {
            var item = itemService.FirstOrDefault(itemName);
            if (item == null)
            {
                logger.LogError("Item {ItemName} could not be found", itemName);
                continue;
            }

            if (item.Type == ItemType.Bottle)
            {
                connectedItems = itemService.LocalPlayersItems()
                    .Where(x => x.Type.IsInCategory(ItemCategory.Bottle)).ToList();
            }

            var fileName = uiService.GetSpritePath(item);
            if (fileName == null)
            {
                logger.LogError("Image for {ItemName} could not be found", item.Name);
                continue;
            }

            items[item] = fileName;
        }

        var model = new TrackerWindowItemPanelViewModel()
        {
            Items = items,
            ConnectedItems = connectedItems,
            LabelImage = labelImage,
            IsLabelActive = items.Keys.Any(x => x.State.TrackingState > 0),
            Row = gridLocation.Row,
            Column = gridLocation.Column,
            IsMedallion = items.Keys.First().Type is ItemType.Bombos or ItemType.Ether or ItemType.Quake
        };

        foreach (var item in items.Keys.Concat(connectedItems ?? []))
        {
            _itemModels[item.Name] = model;
        }

        if (model.IsMedallion)
        {
            model.IsMMRequirement = world.World.MiseryMire.DungeonState.MarkedMedallion == items.Keys.First().Type;
            model.IsTRRequirement = world.World.TurtleRock.DungeonState.MarkedMedallion == items.Keys.First().Type;
            _medallions.Add(model);
        }

        model.UpdateItem(null, null);

        if (items.Count == 1)
        {
            model.Clicked += (_, _) => tracker.TrackItem(items.Keys.First());
        }

        model.ItemGiven += (_, args) => tracker.TrackItem(args.Item);
        model.ItemRemoved += (_, args) => tracker.UntrackItem(args.Item);
        model.ItemSetAsDungeonRequirement += (_, args) =>
        {
            var item = items.Keys.First();

            if (args.IsMMRequirement && world.World.MiseryMire.DungeonState.MarkedMedallion != item.Type)
            {
                tracker.SetDungeonRequirement(world.World.MiseryMire, item.Type);
            }
            else if (!args.IsMMRequirement && world.World.MiseryMire.DungeonState.MarkedMedallion == item.Type)
            {
                tracker.SetDungeonRequirement(world.World.MiseryMire);
            }

            if (args.IsTRRequirement && world.World.TurtleRock.DungeonState.MarkedMedallion != item.Type)
            {
                tracker.SetDungeonRequirement(world.World.TurtleRock, item.Type);
            }
            else if (!args.IsTRRequirement && world.World.TurtleRock.DungeonState.MarkedMedallion == item.Type)
            {
                tracker.SetDungeonRequirement(world.World.TurtleRock);
            }
        };

        return model;
    }

    private TrackerWindowPanelViewModel? GetDungeonPanelViewModel(UIGridLocation gridLocation)
    {
        var dungeon = world.World.Dungeons.FirstOrDefault(x => x.DungeonName == gridLocation.Identifiers.First());
        if (dungeon == null)
        {
            logger.LogError("Dungeon {DungeonName} could not be found", gridLocation.Identifiers.First());
            return null;
        }

        var dungeonImage = uiService.GetSpritePath(dungeon);
        var rewardImage = dungeon.MarkedReward != RewardType.None ? uiService.GetSpritePath(dungeon.MarkedReward) : null;

        var model = new TrackerWindowDungeonPanelViewModel()
        {
            Dungeon = dungeon,
            DungeonImage = dungeonImage,
            RewardImage = rewardImage,
            Row = gridLocation.Row,
            Column = gridLocation.Column,
            DungeonCleared = dungeon.DungeonState.Cleared,
            DungeonTreasure = dungeon.DungeonState.RemainingTreasure
        };

        model.Clicked += (_, _) => tracker.MarkDungeonAsCleared(dungeon);
        model.ResetCleared += (_, _) => tracker.MarkDungeonAsIncomplete(dungeon);
        model.TreasureCleared += (_, _) => tracker.ClearDungeon(dungeon);
        model.RewardSet += (_, args) => tracker.SetDungeonReward(dungeon, args.RewardType);

        _dungeonModels.Add(dungeon.DungeonName, model);

        return model;
    }

    private TrackerWindowPanelViewModel? GetBossPanelViewModel(UIGridLocation gridLocation)
    {
        var boss = world.World.AllBosses.FirstOrDefault(x => x.Name == gridLocation.Identifiers.First());
        if (boss == null)
        {
            logger.LogError("Boss {BossName} could not be found", gridLocation.Identifiers.First());
            return null;
        }

        var fileName = uiService.GetSpritePath(boss.Metadata);
        if (string.IsNullOrEmpty(fileName))
        {
            logger.LogError("Image for {BossName} could not be found", boss.Name);
            return null;
        }

        var model = new TrackerWindowBossPanelViewModel
        {
            Boss = boss,
            BossImage = fileName,
            BossDefeated = boss.State.Defeated,
            Row = gridLocation.Row,
            Column = gridLocation.Column
        };

        _bossModels.Add(boss.Name, model);

        model.Clicked += (_, _) => tracker.MarkBossAsDefeated(boss);
        model.BossRevived += (_, _) => tracker.MarkBossAsNotDefeated(boss);

        return model;
    }

    private TrackerWindowPanelViewModel? GetPegPanelViewModel(UIGridLocation gridLocation)
    {
        if (!int.TryParse(gridLocation.Identifiers.First(), out var pegNumber))
        {
            logger.LogError("Could not determine peg number");
            return null;
        }

        var fileName = uiService.GetSpritePath("Items",
            tracker.PegsPegged >= pegNumber ? "pegged.png" : "peg.png", out _);

        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var model = new TrackerWindowPanelViewModel
        {
            Row = gridLocation.Row,
            Column = gridLocation.Column,
            Images =
            [
                new TrackerWindowPanelImage { ImagePath = fileName, IsActive = true }
            ]
        };

        _pegWorldImages[pegNumber] = model;

        model.Clicked += (_, _) => tracker.Peg();

        return model;
    }

    private TrackerWindowPanelViewModel? GetShakPanelViewModel(UIGridLocation gridLocation)
    {
        if (!int.TryParse(gridLocation.Identifiers.First(), out var pegNumber))
        {
            logger.LogError("Could not determine peg number");
            return null;
        }

        var fileName = uiService.GetSpritePath("Items", "shakspin.gif", out _);

        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var model = new TrackerWindowPanelViewModel
        {
            Row = gridLocation.Row,
            Column = gridLocation.Column,
            Images =
            [
                new TrackerWindowPanelImage { ImagePath = fileName, IsActive = true }
            ]
        };

        model.Clicked += (_, _) => tracker.StopShaktoolMode();

        return model;
    }

    private async Task DisplayError(string message)
    {
        await OpenMessageWindow(message);
    }

    private async Task<MessageWindowResult?> OpenMessageWindow(string message, MessageWindowIcon icon = MessageWindowIcon.Error, MessageWindowButtons buttons = MessageWindowButtons.OK)
    {
        var window = new MessageWindow(new MessageWindowRequest()
        {
            Message = message,
            Title = "SMZ3 Cas' Randomizer",
            Icon = icon,
            Buttons = buttons
        });

        await window.ShowDialog(_window);

        return window.DialogResult;
    }

    private RandomizerOptions Options
    {
        get
        {
            if (_options != null)
            {
                return _options;
            }

            _options = optionsFactory.Create();
            return _options;
        }
    }

    public void ToggleSpeechRecognition()
    {
        if (tracker.VoiceRecognitionEnabled)
        {
            tracker.DisableVoiceRecognition();
        }
        else
        {
            tracker.EnableVoiceRecognition();
        }
    }
}
