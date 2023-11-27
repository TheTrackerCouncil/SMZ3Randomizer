using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.Data.WorldData;

public class VisibleItemZelda
{
    public int? Room { get; set; }
    public int? OverworldScreen { get; set; }
    public int TopLeftX { get; set; }
    public int TopLeftY { get; set; }
    public int BottomRightX { get; set; }
    public int BottomRightY { get; set; }
    public List<LocationId> Locations { get; set; } = new();
}
