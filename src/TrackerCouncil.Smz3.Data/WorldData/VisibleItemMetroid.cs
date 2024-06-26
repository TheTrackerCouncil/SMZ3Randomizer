using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Data.WorldData;

public class VisibleItemMetroid
{
    public int Room { get; set; }
    public List<VisibleItemArea> Areas { get; set; } = new();
}
