namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class RequestPlayerUpdateRequest(string? playerGuid)
{
    public string? PlayerGuid { get; } = playerGuid;
}
