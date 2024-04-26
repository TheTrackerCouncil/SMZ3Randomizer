using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared.Models;
using SnesConnectorLibrary;

namespace Randomizer.CrossPlatform.Views;

public partial class TrackerWindow : RestorableWindow
{
    private readonly TrackerWindowService? _service;
    private GeneratedRom? _generatedRom;
    private readonly TrackerWindowViewModel _model;
    private CurrentTrackWindow? _currentTrackWindow;
    private TrackerMapWindow? _trackerMapWindow;

    public TrackerWindow()
    {
        InitializeComponent();
        DataContext = _model = new TrackerWindowViewModel();
    }

    public TrackerWindow(TrackerWindowService service)
    {
        _service = service;
        DataContext = _model = _service.GetViewModel(this);
        InitializeComponent();

        foreach (var layout in _model.Layouts)
        {
            var layoutMenuItem = new MenuItem
            {
                Header = layout.Name,
                Tag = layout
            };
            layoutMenuItem.Click += LayoutMenuItemOnClick;
            LayoutMenu.Items.Add(layoutMenuItem);
        }

        CreateLayout();
    }

    private void OpenCurrentTrackWindow()
    {
        if (_currentTrackWindow != null)
        {
            return;
        }

        _currentTrackWindow = new CurrentTrackWindow();
        _currentTrackWindow.Show(Owner as Window ?? this);
        _currentTrackWindow.Closed += (_, _) => _currentTrackWindow = null;
    }

    private void OpenTrackerMapWindow()
    {
        if (_trackerMapWindow != null)
        {
            return;
        }

        _trackerMapWindow = new TrackerMapWindow();
        _trackerMapWindow.Show(Owner as Window ?? this);
        _trackerMapWindow.Closed += (_, _) => _trackerMapWindow = null;
    }

    private void CreateLayout()
    {
        MainGrid.Children.Clear();
        foreach (var image in _model.Panels)
        {
            var panel = new TrackerWindowPanel(image);
            Grid.SetColumn(panel, image.Column);
            Grid.SetRow(panel, image.Row);
            MainGrid.Children.Add(panel);
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
            Dispatcher.UIThread.Invoke(CreateLayout);
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
    protected override int DefaultWidth => 800;
    protected override int DefualtHeight => 600;

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _service?.StartTracker();

        if (_model.OpenTrackWindow)
        {
            OpenCurrentTrackWindow();
        }

        _service?.OpenTrackerMapWindow();
        _service?.OpenTrackerLocationsWindow();
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

    private void HelpMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void AutoTrackerMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
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
}

