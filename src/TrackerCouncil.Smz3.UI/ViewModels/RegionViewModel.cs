using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaControls.Models;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class RegionViewModel : ViewModelBase
{
    public RegionViewModel(string regionName, List<string> locationNames)
    {
        RegionName = regionName;
        Locations = locationNames.Select(x => new LocationViewModel(x, regionName)).ToList();
    }

    public RegionViewModel(Region region)
    {
        Region = region;
        RegionName = region.Name;
        AllLocations = region.Locations.ToDictionary(x => x.Id, x => new LocationViewModel(x));

        var locations = new List<LocationViewModel>();

        foreach (var location in AllLocations.Values.Where(x => x.Location != null))
        {
            locations.Add(location);
            location.Location!.AccessibilityUpdated += LocationOnAccessibilityUpdated;
        }

        Locations = locations;
        UpdateLocationCount();
    }

    private void LocationOnAccessibilityUpdated(object? sender, EventArgs e)
    {
        if (sender is not Location location) return;
        var model = AllLocations[location.Id];
        model.InLogic = location.Accessibility == Accessibility.Available;
        model.InLogicWithKeys = location.Accessibility == Accessibility.AvailableWithKeys;
        model.Cleared = location.Cleared;
        SortLocations();
        UpdateLocationCount();

        if (location.Accessibility == Accessibility.Cleared)
        {
            RegionUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    public Region? Region { get; set; }
    public static string? ChestImage { get; set; }
    public string RegionName { get; set; }
    [Reactive] public string LocationCount { get; set; } = "";
    public int InLogicLocationCount => Locations.Count(x => x.InLogic || x.InLogicWithKeys);
    public int SortOrder { get; set; }
    public bool ShowOutOfLogic { get; set; }
    public bool IsVisible => VisibleLocations > 0 && MatchesFilter;
    [Reactive] public List<LocationViewModel> Locations { get; set; }
    public Dictionary<LocationId, LocationViewModel> AllLocations { get; set; } = [];

    [Reactive]
    [ReactiveLinkedProperties(nameof(IsVisible))]
    public bool MatchesFilter { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(IsVisible))]
    public int VisibleLocations { get; set; }

    public event EventHandler? RegionUpdated;

    public void UpdateLocationCount()
    {
        VisibleLocations = Locations.Count(x => x.IsVisible);
        LocationCount = VisibleLocations.ToString();
    }

    public void SortLocations()
    {
        Locations = AllLocations.Values.OrderBy(x => !x.InLogic).ToList();
    }
}
