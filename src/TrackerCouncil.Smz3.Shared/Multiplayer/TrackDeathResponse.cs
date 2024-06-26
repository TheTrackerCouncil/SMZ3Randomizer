namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class TrackDeathResponse(string playerguid)
{
    public string PlayerGuid { get; init; } = playerguid;
}
