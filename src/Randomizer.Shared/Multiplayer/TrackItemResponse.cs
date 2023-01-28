namespace Randomizer.Shared.Multiplayer;

public class TrackItemResponse
{
    public TrackItemResponse(string playerGuid, ItemType itemType, int trackedValue)
    {
        PlayerGuid = playerGuid;
        ItemType = itemType;
        TrackedValue = trackedValue;
    }

    public string PlayerGuid { get; }
    public ItemType ItemType { get; }
    public int TrackedValue { get; }
}
