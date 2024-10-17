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
        var mockLocationsNames = new List<string>();
        for (var i = 0; i < 20; i++)
        {
            mockLocationsNames.Add($"Location {i}");
        }

        DataContext = _model = new TrackerLocationsViewModel()
        {
            Regions =
            [
                new RegionViewModel("Test Region", mockLocationsNames)
            ]
        };
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
        var newValue = (sender as CheckBox)?.IsChecked == true;
        OutOfLogicChanged?.Invoke(this, new OutOfLogicChangedEventArgs() { ShowOutOfLogic = newValue });
        _service?.UpdateShowOutOfLogic(newValue);
    }

    private void FilterComboBox_OnValueChanged(object sender, EnumValueChangedEventArgs args)
    {
        _service?.UpdateFilter((sender as EnumComboBox)?.Value as RegionFilter? ?? RegionFilter.None);
    }
}

