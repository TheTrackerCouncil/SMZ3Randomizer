using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class TrackLocationRequest(string playerGuid, LocationId locationId)
{
    public string PlayerGuid { get; } = playerGuid;
    public LocationId LocationId { get; } = locationId;
}
