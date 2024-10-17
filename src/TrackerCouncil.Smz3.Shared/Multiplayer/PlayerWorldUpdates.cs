using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class PlayerWorldUpdates
{
    public readonly List<MultiplayerLocationState> Locations = new();
    public readonly List<MultiplayerItemState> Items = new();
    public readonly List<MultiplayerBossState> Bosses = new();

    public bool HasUpdates => Locations.Count > 0 || Items.Count > 0 || Bosses.Count > 0;
}
