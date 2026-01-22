using System;
using AvaloniaControls.Models;
using ReactiveUI.SourceGenerators;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class MarkedLocationViewModel(Location location, Item? itemData, string? itemSprite) : ViewModelBase
{
    public Location Location => location;
    public string? ItemSprite { get; init; } = itemSprite;

    [Reactive, ReactiveLinkedProperties(nameof(Opacity))]
    public partial bool IsAvailable { get; set; } = location.Accessibility is Accessibility.Available or Accessibility.AvailableWithKeys;

    public bool ShowOutOfLogic { get; set; }
    public double Opacity => ShowOutOfLogic || IsAvailable ? 1.0 : 0.33;
    public string Item { get; init; }= itemData?.Name ?? "";
    public string LocationName { get; init; }= location.Metadata.Name?[0] ?? location.Name ?? "";
    public string Area { get; init; } = location.Region.Metadata.Name?[0] ?? location.Region.Name ?? "";

    public void LocationOnAccessibilityUpdated(object? sender, EventArgs e)
    {
        IsAvailable = location.Accessibility is Accessibility.Available or Accessibility.AvailableWithKeys;
    }
}
