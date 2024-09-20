using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.Tracking;

public class ViewedObject
{
    public PlayerHintTile? HintTile { get; init; }
    public List<Location>? ViewedLocations { get; init; }
}
