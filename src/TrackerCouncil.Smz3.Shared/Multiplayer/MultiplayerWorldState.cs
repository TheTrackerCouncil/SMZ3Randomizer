using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class MultiplayerWorldState(
    Dictionary<LocationId, bool> locations,
    Dictionary<ItemType, int> items,
    Dictionary<BossType, bool> bosses,
    Dictionary<string, bool> dungeons)
{
    public Dictionary<LocationId, bool> Locations { get; set; } = locations;
    public Dictionary<ItemType, int> Items { get; set; } = items;
    public Dictionary<BossType, bool> Bosses { get; set; } = bosses;
    public Dictionary<string, bool> Dungeons { get; set; } = dungeons;
}
