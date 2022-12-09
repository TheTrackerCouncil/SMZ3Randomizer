namespace Randomizer.Shared.Multiplayer;

public class SubmitPlayerGenerationDataRequest
{
    public SubmitPlayerGenerationDataRequest(string playerGuid, int worldId, string playerGenerationData)
    {
        PlayerGuid = playerGuid;
        PlayerGenerationData = playerGenerationData;
        WorldId = worldId;
    }

    public string PlayerGuid { get; }
    public int WorldId { get; }
    public string PlayerGenerationData { get; }
}
