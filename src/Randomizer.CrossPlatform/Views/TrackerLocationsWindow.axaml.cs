using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.Shared.Enums;

namespace Randomizer.CrossPlatform.Views;

public partial class TrackerLocationsWindow : RestorableWindow
{
    private TrackerLocationsViewModel _model;
    private TrackerLocationsWindowService? _service;

    public TrackerLocationsWindow()
    {
        InitializeComponent();
        DataContext = _model = new TrackerLocationsViewModel();
    }

    public TrackerLocationsWindow(TrackerLocationsWindowService service)
    {
        _service = service;
        InitializeComponent();
        DataContext = _model = _service.GetViewModel();
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "tracker-locations-window.json");
    protected override int DefaultWidth => 450;
    protected override int DefualtHeight => 600;

    private void ClearLocationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { Tag: LocationViewModel model })
        {
            return;
        }

        _service?.ClearLocation(model);
    }

    private void ToggleShowOutOfLogicButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        _model.ShowOutOfLogic = (sender as CheckBox)?.IsChecked == true;
        _service?.UpdateModel();
    }

    private void EnumComboBox_OnValueChanged(object sender, EnumValueChangedEventArgs args)
    {
        _model.Filter = (sender as EnumComboBox)?.Value as RegionFilter? ?? RegionFilter.None;
        _service?.UpdateModel();
    }
}

