namespace Randomizer.Shared.Multiplayer;

public class UpdatePlayerWorldRequest
{
    public UpdatePlayerWorldRequest(string playerGuid, MultiplayerWorldState worldState, bool propogate)
    {
        PlayerGuid = playerGuid;
        WorldState = worldState;
        Propogate = propogate;
    }

    public string PlayerGuid { get; }
    public MultiplayerWorldState WorldState { get; }
    public bool Propogate { get; }
}
