namespace Randomizer.Shared.Multiplayer;

public class TrackLocationRequest
{
    public TrackLocationRequest(string playerGuid, LocationId locationId)
    {
        PlayerGuid = playerGuid;
        LocationId = locationId;
    }

    public string PlayerGuid { get; }
    public LocationId LocationId { get; }
}
