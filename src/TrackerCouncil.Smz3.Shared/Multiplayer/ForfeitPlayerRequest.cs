namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class ForfeitPlayerGameRequest(string playerGuid)
{
    public string PlayerGuid { get; } = playerGuid;
}
