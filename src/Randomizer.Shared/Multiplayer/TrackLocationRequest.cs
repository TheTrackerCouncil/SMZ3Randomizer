namespace Randomizer.Shared.Multiplayer;

public class TrackLocationRequest
{
    public TrackLocationRequest(string playerGuid, int locationId)
    {
        PlayerGuid = playerGuid;
        LocationId = locationId;
    }

    public string PlayerGuid { get; }
    public int LocationId { get; }
}
