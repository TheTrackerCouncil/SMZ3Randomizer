using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class TrackLocationResponse(string playerGuid, LocationId locationId)
{
    public string PlayerGuid { get; init; } = playerGuid;
    public LocationId LocationId { get; init; } = locationId;
}
