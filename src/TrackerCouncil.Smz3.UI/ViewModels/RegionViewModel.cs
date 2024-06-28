using System.Collections.Generic;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class RegionViewModel
{
    public static string? ChestImage { get; set; }
    public string RegionName { get; set; } = "";
    public string LocationCount => Locations.Count.ToString();
    public List<LocationViewModel> Locations { get; set; } = [];
}
