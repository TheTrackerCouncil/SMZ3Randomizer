namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class UpdatePlayerWorldRequest(string playerGuid, MultiplayerWorldState worldState, bool propogate)
{
    public string PlayerGuid { get; } = playerGuid;
    public MultiplayerWorldState WorldState { get; } = worldState;
    public bool Propogate { get; } = propogate;
}
