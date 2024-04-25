using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.ViewModels;

namespace Randomizer.CrossPlatform.Views;

public partial class TrackerMapWindow : RestorableWindow
{
    private TrackerMapWindowViewModel _model;
    private readonly TrackerMapWindowService? _service;

    public TrackerMapWindow()
    {
        InitializeComponent();
        DataContext = _model = new TrackerMapWindowViewModel();
    }

    public TrackerMapWindow(TrackerMapWindowService service)
    {
        _service = service;
        InitializeComponent();
        DataContext = _model = _service.GetViewModel(this);
    }

    private void MapCanvas_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _service?.UpdateSize(e.NewSize);
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        LocationsItemsControl.ItemsSource = null;
        _service?.UpdateMap();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _service?.UpdateSize(MainGrid.Bounds.Size);
        _service?.UpdateLocations();
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "tracker-map-window.json");
    protected override int DefaultWidth => 1024;
    protected override int DefualtHeight => 768;

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);

        if (!point.Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (sender is not Grid { Tag: TrackerMapLocationViewModel model })
        {
            return;
        }

        _service?.Clear(model);
    }

    private void ContextMenuInputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not ContextMenu { SelectedItem: TrackerMapSubLocationViewModel model })
        {
            return;
        }

        _service?.Clear(model);
    }
}

