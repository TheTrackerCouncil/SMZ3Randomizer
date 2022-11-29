namespace Randomizer.Shared.Multiplayer;

public class SubmitPlayerGenerationDataRequest
{
    public SubmitPlayerGenerationDataRequest(string playerGuid, string playerGenerationData)
    {
        PlayerGuid = playerGuid;
        PlayerGenerationData = playerGenerationData;
    }

    public string PlayerGuid { get; }
    public string PlayerGenerationData { get; }
}
