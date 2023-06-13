namespace Randomizer.Shared.Multiplayer;

public class TrackLocationResponse
{
    public TrackLocationResponse(string playerGuid, LocationId locationId)
    {
        PlayerGuid = playerGuid;
        LocationId = locationId;
    }

    public string PlayerGuid { get; init; }
    public LocationId LocationId { get; init; }
}
