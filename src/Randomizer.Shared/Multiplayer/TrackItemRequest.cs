namespace Randomizer.Shared.Multiplayer;

public class TrackItemRequest
{
    public TrackItemRequest(string playerGuid, ItemType itemType, int trackedValue)
    {
        PlayerGuid = playerGuid;
        ItemType = itemType;
        TrackedValue = trackedValue;
    }

    public string PlayerGuid { get; }
    public ItemType ItemType { get; }

    public int TrackedValue { get; }
}
