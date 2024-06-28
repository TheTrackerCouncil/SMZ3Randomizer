using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class TrackItemResponse(string playerGuid, ItemType itemType, int trackedValue)
{
    public string PlayerGuid { get; } = playerGuid;
    public ItemType ItemType { get; } = itemType;
    public int TrackedValue { get; } = trackedValue;
}
