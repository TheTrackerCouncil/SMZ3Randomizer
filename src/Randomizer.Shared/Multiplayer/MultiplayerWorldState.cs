using System.Collections.Generic;
using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Multiplayer;

public class MultiplayerWorldState
{
    public MultiplayerWorldState(Dictionary<int, bool> locations, Dictionary<ItemType, int> items,
        Dictionary<BossType, bool> bosses, Dictionary<string, bool> dungeons)
    {
        Locations = locations;
        Items = items;
        Bosses = bosses;
        Dungeons = dungeons;
    }

    public Dictionary<int, bool> Locations { get; set; }
    public Dictionary<ItemType, int> Items { get; set; }
    public Dictionary<BossType, bool> Bosses { get; set; }
    public Dictionary<string, bool> Dungeons { get; set; }
}
