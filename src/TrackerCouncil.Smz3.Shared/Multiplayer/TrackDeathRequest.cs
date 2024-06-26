namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class TrackDeathRequest(string playerGuid)
{
    public string PlayerGuid { get; } = playerGuid;
}
