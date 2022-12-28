namespace Randomizer.Shared.Multiplayer;

public class TrackLocationResponse
{
    public TrackLocationResponse(string playerGuid, int locationId)
    {
        PlayerGuid = playerGuid;
        LocationId = locationId;
    }

    public string PlayerGuid { get; init; }
    public int LocationId { get; init; }
}
