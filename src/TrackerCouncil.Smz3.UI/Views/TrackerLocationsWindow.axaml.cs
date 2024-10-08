using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public class OutOfLogicChangedEventArgs : EventArgs
{
    public required bool ShowOutOfLogic { get; init; }
}

public partial class TrackerLocationsWindow : RestorableWindow
{
    private TrackerLocationsViewModel _model;
    private TrackerLocationsWindowService? _service;

    public TrackerLocationsWindow()
    {
        InitializeComponent();
        DataContext = _model = new TrackerLocationsViewModel()
        {
            Regions =
            [
                new RegionViewModel()
                {
                    RegionName = "Test Region",
                    Locations = CreateMockLocations()
                }
            ]
        };
        return;

        List<LocationViewModel> CreateMockLocations()
        {
            var toReturn = new List<LocationViewModel>();

            for (var i = 0; i < 20; i++)
            {
                toReturn.Add(new LocationViewModel(null, true, true) { Name = $"Location {i}"});
            }

            return toReturn;
        }
    }

    public TrackerLocationsWindow(TrackerLocationsWindowService service)
    {
        _service = service;
        InitializeComponent();
        DataContext = _model = _service.GetViewModel();
    }

    protected override string RestoreFilePath => Path.Combine(Directories.AppDataFolder, "Windows", "tracker-locations-window.json");
    protected override int DefaultWidth => 450;
    protected override int DefaultHeight => 600;

    public event EventHandler<OutOfLogicChangedEventArgs>? OutOfLogicChanged;

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
        OutOfLogicChanged?.Invoke(this, new OutOfLogicChangedEventArgs() { ShowOutOfLogic = _model.ShowOutOfLogic });
    }

    private void EnumComboBox_OnValueChanged(object sender, EnumValueChangedEventArgs args)
    {
        _model.Filter = (sender as EnumComboBox)?.Value as RegionFilter? ?? RegionFilter.None;
        _service?.UpdateModel();
    }
}

