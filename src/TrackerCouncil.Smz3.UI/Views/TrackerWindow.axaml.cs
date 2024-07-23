using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared.Models;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class TrackerWindow : RestorableWindow
{
    private readonly TrackerWindowService? _service;
    private GeneratedRom? _generatedRom;
    private readonly TrackerWindowViewModel _model;
    private CurrentTrackWindow? _currentTrackWindow;
    private Dictionary<string, Image> _layoutImages = new();
    private MainWindow? _parentWindow;

    public TrackerWindow()
    {
        InitializeComponent();
        DataContext = _model = new TrackerWindowViewModel();
        _parentWindow = MessageWindow.GlobalParentWindow as MainWindow;
    }

    public TrackerWindow(TrackerWindowService service)
    {
        _service = service;

        DataContext = _model = _service.GetViewModel(this);
        InitializeComponent();

        foreach (var layout in _model.Layouts)
        {
            var image = new Image()
            {
                Source = new Bitmap(AssetLoader.Open(new Uri("avares://SMZ3CasRandomizer/Assets/check.png"))),
                IsVisible = false
            };
            _layoutImages[layout.Name] = image;

            var layoutMenuItem = new MenuItem
            {
                Header = layout.Name,
                Tag = layout,
                Icon = image
            };
            layoutMenuItem.Click += LayoutMenuItemOnClick;
            LayoutMenu.Items.Add(layoutMenuItem);
        }

        service.LayoutSet += (sender, args) =>
        {
            Dispatcher.UIThread.Invoke(CreateLayout);
        };
        CreateLayout();
        _parentWindow = MessageWindow.GlobalParentWindow as MainWindow;
    }

    private void OpenCurrentTrackWindow()
    {
        if (_currentTrackWindow != null)
        {
            return;
        }

        _currentTrackWindow = new CurrentTrackWindow();
        _currentTrackWindow.Show(this);
        _currentTrackWindow.Closed += (_, _) => _currentTrackWindow = null;
    }

    private void CreateLayout()
    {
        MainPanel.Children.Clear();
        foreach (var layout in _model.Layouts)
        {
            _layoutImages[layout.Name].IsVisible = layout.Name == _model.LayoutName;
        }
        foreach (var image in _model.Panels)
        {
            var panel = new TrackerWindowPanel(image);
            panel.VerticalAlignment = VerticalAlignment.Top;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Margin = new Thickness((image.Column-1) * 34, (image.Row-1) * 34, 0, 0);
            MainPanel.Children.Add(panel);
        }
    }

    private void LayoutMenuItemOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: UILayout layout })
        {
            return;
        }

        // This can take a few seconds when rebuilding the full UI, so rather than have the UI freeze,
        // fire it off in a new task
        ITaskService.Run(() =>
        {
            _service?.SetLayout(layout);

            Dispatcher.UIThread.Invoke(() =>
            {
                if (Math.Abs(Width - _model.IdealWindowWidth) > 1 || Math.Abs(Height - _model.IdealWindowHeight) > 1)
                {
                    _model.ShowResizeButton = true;
                }
            });
        });
    }

    public GeneratedRom? Rom
    {
        get => _generatedRom;
        set
        {
            _generatedRom = value;
            if (value != null)
            {
                _service?.SetRom(value);
            }
        }
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "tracker-window.json");
    protected override int DefaultWidth => 350;
    protected override int DefaultHeight => 300;

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        ITaskService.Run(() =>
        {
            _service?.StartTracker();

            Dispatcher.UIThread.Invoke(() =>
            {
                if (_model.OpenTrackWindow)
                {
                    OpenCurrentTrackWindow();
                }
                _service?.OpenTrackerLocationsWindow();
                _service?.OpenTrackerMapWindow();
            }, DispatcherPriority.Background);
        });

        if (Math.Abs(Width - _model.IdealWindowWidth) > 1 || Math.Abs(Height - _model.IdealWindowHeight) > 1)
        {
            _model.ShowResizeButton = true;
        }

        _parentWindow?.Hide();
    }

    private async void LoadSavedStateMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null) return;
        await _service.LoadRomAsync();
    }

    private async void SaveStateMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null) return;
        await _service.SaveStateAsync();
    }

    private void LocationsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.OpenTrackerLocationsWindow();
    }

    private void MapMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.OpenTrackerMapWindow();
    }

    private void CurrentSongMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenCurrentTrackWindow();
    }

    private void AutoTrackerMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = new AutoTrackingHelpWindow();
        window.ShowDialog(this);
    }

    private bool _finishedShutdown;

    private async void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_service == null) return;

        if (!_finishedShutdown)
        {
            e.Cancel = true;
            _finishedShutdown = true;
            await _service.Shutdown();
        }

        _currentTrackWindow?.Close(true);

        _parentWindow?.Reload();
        _parentWindow?.Show();
    }

    private void TimeTextBlock_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);

        if (point.Properties.IsLeftButtonPressed && e.ClickCount == 2)
        {
            _service?.ResetTimer();
        }
        else if (point.Properties.IsRightButtonPressed)
        {
            _service?.ToggleTimer();
        }
    }

    private void DisableAutoTrackerMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SetConnector(SnesConnectorType.None);
    }

    private void SniMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SetConnector(SnesConnectorType.Sni);
    }

    private void Usb2SnesMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SetConnector(SnesConnectorType.Usb2Snes);
    }

    private void LuaMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SetConnector(SnesConnectorType.Lua);
    }

    private void CrowdControlMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SetConnector(SnesConnectorType.LuaCrowdControl);
    }

    private void EmoTrackerMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SetConnector(SnesConnectorType.LuaEmoTracker);
    }

    private void OpenFolderMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.OpenAutoTrackerFolder();
    }

    private void TrackerHelpMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.OpenTrackerHelpWindow();
    }

    private void SpeechRecognition_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);

        if (point.Properties.IsLeftButtonPressed && e.ClickCount == 2)
        {
            _service?.ToggleSpeechRecognition();
        }
    }

    private void ResizeWindowButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Width = _model.IdealWindowWidth;
        Height = _model.IdealWindowHeight;
        _model.ShowResizeButton = false;
    }

    private void WindowBase_OnResized(object? sender, WindowResizedEventArgs e)
    {
        if (Math.Abs(Width - _model.IdealWindowWidth) > 1 || Math.Abs(Height - _model.IdealWindowHeight) > 1)
        {
            _model.ShowResizeButton = true;
        }
        else
        {
            _model.ShowResizeButton = false;
        }
    }
}

