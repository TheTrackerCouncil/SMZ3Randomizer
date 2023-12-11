using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.Data.WorldData;

public class VisibleItemArea
{
    public int TopLeftX { get; set; }
    public int TopLeftY { get; set; }
    public int BottomRightX { get; set; }
    public int BottomRightY { get; set; }
    public List<LocationId> Locations { get; set; } = new();
}
