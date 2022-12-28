using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class PlayerListSyncRequest
{
    public PlayerListSyncRequest(bool sendToAllPlayers)
    {
        SendToAllPlayers = sendToAllPlayers;
    }

    public bool SendToAllPlayers { get; }
}
