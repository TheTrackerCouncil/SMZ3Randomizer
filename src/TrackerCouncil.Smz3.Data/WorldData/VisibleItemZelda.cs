using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Data.WorldData;

public class VisibleItemZelda
{
    public int? Room { get; set; }
    public int? OverworldScreen { get; set; }
    public List<VisibleItemArea> Areas { get; set; } = new();
}
