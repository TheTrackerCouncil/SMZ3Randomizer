namespace Randomizer.Shared.Multiplayer;

public class TrackItemRequest
{
    public TrackItemRequest(string playerGuid, ItemType itemType)
    {
        PlayerGuid = playerGuid;
        ItemType = itemType;
    }

    public string PlayerGuid { get; }
    public ItemType ItemType { get; }
}
