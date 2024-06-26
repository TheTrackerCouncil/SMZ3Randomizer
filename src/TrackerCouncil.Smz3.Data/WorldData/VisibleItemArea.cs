using System.Collections.Generic;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.WorldData;

public class VisibleItemArea
{
    public int TopLeftX { get; set; }
    public int TopLeftY { get; set; }
    public int BottomRightX { get; set; }
    public int BottomRightY { get; set; }
    public List<LocationId> Locations { get; set; } = new();
}
