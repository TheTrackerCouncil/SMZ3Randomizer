namespace Randomizer.Shared.Multiplayer;

public class TrackItemResponse
{
    public TrackItemResponse(string playerGuid, ItemType itemType)
    {
        PlayerGuid = playerGuid;
        ItemType = itemType;
    }

    public string PlayerGuid { get; init; }
    public ItemType ItemType { get; init; }
}
