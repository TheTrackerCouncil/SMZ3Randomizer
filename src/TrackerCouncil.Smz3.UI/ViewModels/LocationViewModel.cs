using AvaloniaControls.Models;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class LocationViewModel : ViewModelBase
{
    public LocationViewModel(string name, string area)
    {
        Name = name;
        Area = area;
    }

    public LocationViewModel(Location location)
    {
        Name = location.Metadata.Name?[0] ?? location.Name;
        Area = location.Region.Metadata.Name?[0] ?? location.Region.Name;
        InLogic = location.Accessibility == Accessibility.Available;
        InLogicWithKeys = location.Accessibility == Accessibility.AvailableWithKeys;
        Location = location;
    }

    public static string? KeyImage { get; set; }
    public string Name { get; init; }
    public string Area { get; init; }
    public double Opacity => InLogic || InLogicWithKeys ? 1.0 : 0.33;
    public bool ShowKeyIcon => InLogicWithKeys && !InLogic;

    [ReactiveLinkedProperties(nameof(Opacity), nameof(IsVisible))]
    public bool ShowOutOfLogic { get; set; }

    [ReactiveLinkedProperties(nameof(Opacity), nameof(ShowKeyIcon))]
    public bool InLogic { get; set; }

    [ReactiveLinkedProperties(nameof(Opacity), nameof(ShowKeyIcon), nameof(IsVisible))]
    public bool InLogicWithKeys { get; set; }

    public bool Cleared { get; set; }

    public bool IsVisible => !Cleared && (InLogic || InLogicWithKeys || ShowOutOfLogic);



    public Location? Location { get; }
}
