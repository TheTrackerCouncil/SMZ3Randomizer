using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.Tracking.VoiceCommands;
using TrackerCouncil.Smz3.UI.ViewModels;
using TrackerCouncil.Smz3.UI.Views;

namespace TrackerCouncil.Smz3.UI.Services;

public class TrackerWindowService(
    TrackerBase tracker,
    IUIService uiService,
    OptionsFactory optionsFactory,
    IWorldAccessor world,
    ITrackerTimerService trackerTimerService,
    IServiceProvider serviceProvider,
    IWorldQueryService worldQueryService,
    ILogger<TrackerWindowService> logger) : ControlService
{
    private RandomizerOptions? _options;
    private TrackerWindow _window = null!;
    private readonly DispatcherTimer _dispatcherTimer = new();
    private readonly TrackerWindowViewModel _model = new();
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
        _model.OpenSpeechWindow = Options.GeneralOptions.DisplayTrackerSpeechWindow;
        _model.AddShadows = Options.GeneralOptions.TrackerShadows;
        _model.DisplayTimer = Options.GeneralOptions.TrackerTimerEnabled;

        LocationViewModel.KeyImage = uiService.GetSpritePath("Items", "key.png", out _);
        RegionViewModel.ChestImage = uiService.GetSpritePath("Items", "chest.png", out _);

        _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
        _dispatcherTimer.Tick += (_, _) => _model.TimeString = trackerTimerService.TimeString;
        _dispatcherTimer.Start();

        tracker.ModeTracker.GoModeToggledOn += (sender, args) =>
        {
            _model.IsInGoMode = true;
        };

        tracker.ModeTracker.GoModeToggledOff += (sender, args) =>
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

        tracker.ModeTracker.ToggledPegWorldModeOn += (sender, args) =>
        {
            TogglePegWorld(tracker.ModeTracker.PegWorldMode);
        };

        tracker.ModeTracker.PegPegged += (sender, args) =>
        {
            UpdatePegs();
        };

        tracker.ModeTracker.ToggledShaktoolMode += (sender, args) =>
        {
            ToggleShaktoolMode(tracker.ModeTracker.ShaktoolMode);
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

        _defaultLayout =
            uiService.SelectableLayouts.FirstOrDefault(x => x.Name == Options.GeneralOptions.SelectedLayout) ??
            uiService.SelectableLayouts.First();

        SetLayout(_defaultLayout);

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
                    ImagePath = i <= tracker.ModeTracker.PegsPegged ? peggedFileName! : unpeggedFileName!,
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
            if (tracker.World.Config.GameMode == GameMode.Multiworld)
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

            if (layout.Name == "Shak")
            {
                break;
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

    public TrackerSpeechWindow OpenTrackerSpeechWindow()
    {
        var window = serviceProvider.GetRequiredService<TrackerSpeechWindow>();
        window.Show(_window);
        return window;
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
        var allItems = new List<Item>();
        List<Item>? connectedItems = null;

        var labelImage = uiService.GetSpritePath("Items", gridLocation.Image ?? "", out _);

        foreach (var itemName in gridLocation.Identifiers)
        {
            var currentItems = worldQueryService.LocalPlayersItems().Where(x => x.Is(itemName)).ToList();
            allItems.AddRange(currentItems);
            var item = currentItems.FirstOrDefault();
            if (item == null)
            {
                logger.LogError("Item {ItemName} could not be found", itemName);
                continue;
            }

            if (item.Type == ItemType.Bottle)
            {
                connectedItems = worldQueryService.LocalPlayersItems()
                    .Where(x => x.Type.IsInCategory(ItemCategory.Bottle)).ToList();
                allItems.AddRange(connectedItems);
            }

            var fileName = uiService.GetSpritePath(item);
            if (fileName == null)
            {
                logger.LogError("Image for {ItemName} could not be found", item.Name);
                continue;
            }

            items[item] = fileName;
        }

        var replacementImages = gridLocation.ReplacementImages == null
            ? []
            : gridLocation.ReplacementImages.ToDictionary(x => x.Key,
                x => uiService.GetSpritePath("Items", x.Value, out _));

        var model = new TrackerWindowItemPanelViewModel()
        {
            Items = items,
            ItemReplacementImages = replacementImages,
            ConnectedItems = connectedItems,
            LabelImage = labelImage,
            IsLabelActive = items.Keys.Any(x => x.TrackingState > 0),
            Row = gridLocation.Row,
            Column = gridLocation.Column,
            AddShadows = _model.AddShadows,
            IsMedallion = items.Keys.First().Type.IsInCategory(ItemCategory.Medallion)
        };

        foreach (var item in allItems)
        {
            item.UpdatedItemState += (sender, args) =>
            {
                var sprite = uiService.GetSpritePath(item);
                model.UpdateItem(item, sprite);
            };
        }

        if (model.IsMedallion)
        {
            var miseryMire = world.World.MiseryMire;
            model.IsMMRequirement = miseryMire.PrerequisiteState.MarkedItem == items.Keys.First().Type;
            miseryMire.UpdatedPrerequisite += (_, _) =>
            {
                model.IsMMRequirement = miseryMire.PrerequisiteState.MarkedItem == items.Keys.First().Type;
            };

            var turtleRock = world.World.TurtleRock;
            model.IsTRRequirement = turtleRock.PrerequisiteState.MarkedItem == items.Keys.First().Type;
            turtleRock.UpdatedPrerequisite += (_, _) =>
            {
                model.IsTRRequirement = turtleRock.PrerequisiteState.MarkedItem == items.Keys.First().Type;
            };
        }

        model.UpdateItem(null, null);

        if (items.Count == 1)
        {
            model.Clicked += (_, _) => tracker.ItemTracker.TrackItem(items.Keys.First());
        }

        model.ItemGiven += (_, args) => tracker.ItemTracker.TrackItem(args.Item);
        model.ItemRemoved += (_, args) => tracker.ItemTracker.UntrackItem(args.Item);
        model.ItemSetAsDungeonRequirement += (_, args) =>
        {
            var item = items.Keys.First();

            if (args.IsMMRequirement && world.World.MiseryMire.PrerequisiteState.MarkedItem != item.Type)
            {
                tracker.PrerequisiteTracker.SetDungeonRequirement(world.World.MiseryMire, item.Type);
            }
            else if (!args.IsMMRequirement && world.World.MiseryMire.PrerequisiteState.MarkedItem == item.Type)
            {
                tracker.PrerequisiteTracker.SetDungeonRequirement(world.World.MiseryMire);
            }

            if (args.IsTRRequirement && world.World.TurtleRock.PrerequisiteState.MarkedItem != item.Type)
            {
                tracker.PrerequisiteTracker.SetDungeonRequirement(world.World.TurtleRock, item.Type);
            }
            else if (!args.IsTRRequirement && world.World.TurtleRock.PrerequisiteState.MarkedItem == item.Type)
            {
                tracker.PrerequisiteTracker.SetDungeonRequirement(world.World.TurtleRock);
            }
        };

        return model;
    }

    private TrackerWindowPanelViewModel? GetDungeonPanelViewModel(UIGridLocation gridLocation)
    {
        var dungeon = world.World.TreasureRegions.FirstOrDefault(x => x.Name == gridLocation.Identifiers.First());
        if (dungeon == null)
        {
            logger.LogError("Dungeon {DungeonName} could not be found", gridLocation.Identifiers.First());
            return null;
        }

        var rewardRegion = dungeon as IHasReward;
        var bossRegion = dungeon as IHasBoss;

        var dungeonImage = uiService.GetSpritePath(dungeon);
        var rewardImage = rewardRegion?.RewardType.GetCategories().Length > 0 ? uiService.GetSpritePath(rewardRegion.MarkedReward) : null;

        var model = new TrackerWindowDungeonPanelViewModel()
            {
                Region = dungeon as Region,
                DungeonImage = dungeonImage,
                RewardImage = rewardImage,
                Row = gridLocation.Row,
                Column = gridLocation.Column,
                AddShadows = _model.AddShadows,
                DungeonCleared = bossRegion?.BossDefeated == true,
                DungeonTreasure = dungeon.RemainingTreasure,
            };

        if (bossRegion != null)
        {
            bossRegion.Boss.UpdatedBossState += (_, _) => model.DungeonCleared = bossRegion.BossDefeated;
            model.Clicked += (_, _) => tracker.BossTracker.MarkBossAsDefeated(bossRegion);
            model.ResetCleared += (_, _) => tracker.BossTracker.MarkBossAsNotDefeated(bossRegion);
        }

        dungeon.UpdatedTreasure += (_, _) => model.DungeonTreasure = dungeon.RemainingTreasure;
        model.TreasureCleared += (_, _) => tracker.TreasureTracker.ClearDungeon(dungeon);

        if (rewardRegion != null)
        {
            rewardRegion.Reward.UpdatedRewardState += (_, _) =>
            {
                var newImage = rewardRegion.MarkedReward.GetCategories().Length > 0
                    ? uiService.GetSpritePath(rewardRegion.MarkedReward)
                    : null;
                model.RewardImage = newImage;
            };

            model.RewardSet += (_, args) => tracker.RewardTracker.SetAreaReward(rewardRegion, args.RewardType);
        }

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

        IHasReward? rewardRegion = null;
        string? rewardImage = null;

        // If this is a parsed AP/Mainline rom, also show the reward for the boss
        if (world.World.Config.RomGenerator != RomGenerator.Cas)
        {
            rewardRegion = boss.Region as IHasReward;
            rewardImage = rewardRegion?.RewardType.GetCategories().Length > 0
                ? uiService.GetBossRewardPath(rewardRegion.MarkedReward)
                : null;
        }

        var model = new TrackerWindowBossPanelViewModel
        {
            Boss = boss,
            BossImage = fileName,
            BossDefeated = boss.Defeated,
            RewardRegion = rewardRegion,
            RewardImage = rewardImage,
            Row = gridLocation.Row,
            Column = gridLocation.Column,
            AddShadows = _model.AddShadows
        };

        boss.UpdatedBossState += (_, _) =>
        {
            model.BossDefeated = boss.Defeated;
        };

        model.Clicked += (_, _) => tracker.BossTracker.MarkBossAsDefeated(boss);
        model.BossRevived += (_, _) => tracker.BossTracker.MarkBossAsNotDefeated(boss);

        if (rewardRegion != null)
        {
            rewardRegion.Reward.UpdatedRewardState += (_, _) =>
            {
                model.RewardImage = rewardRegion?.RewardType.GetCategories().Length > 0
                    ? uiService.GetBossRewardPath(rewardRegion.MarkedReward)
                    : null;
            };

            model.RewardSet += (_, args) => tracker.RewardTracker.SetAreaReward(rewardRegion, args.RewardType);
        }

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
            tracker.ModeTracker.PegsPegged >= pegNumber ? "pegged.png" : "peg.png", out _);

        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var model = new TrackerWindowPanelViewModel
        {
            Row = gridLocation.Row,
            Column = gridLocation.Column,
            AddShadows = _model.AddShadows,
            Images =
            [
                new TrackerWindowPanelImage { ImagePath = fileName, IsActive = true }
            ]
        };

        _pegWorldImages[pegNumber] = model;

        model.Clicked += (_, _) => tracker.ModeTracker.Peg();

        return model;
    }

    private TrackerWindowPanelViewModel? GetShakPanelViewModel(UIGridLocation gridLocation)
    {
        if (!int.TryParse(gridLocation.Identifiers.First(), out _))
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
            AddShadows = _model.AddShadows,
            Images =
            [
                new TrackerWindowPanelImage { ImagePath = fileName, IsActive = true, Width = 128, Height = 128 }
            ]
        };

        model.Clicked += (_, _) => tracker.ModeTracker.StopShaktoolMode();

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
