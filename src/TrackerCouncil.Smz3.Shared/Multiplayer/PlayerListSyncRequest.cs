namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class PlayerListSyncRequest(bool sendToAllPlayers)
{
    public bool SendToAllPlayers { get; } = sendToAllPlayers;
}
