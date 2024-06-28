namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class SubmitPlayerGenerationDataRequest(string playerGuid, int worldId, string playerGenerationData)
{
    public string PlayerGuid { get; } = playerGuid;
    public int WorldId { get; } = worldId;
    public string PlayerGenerationData { get; } = playerGenerationData;
}
