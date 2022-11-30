namespace Randomizer.Shared.Multiplayer;

public class RequestPlayerUpdateRequest
{
    public RequestPlayerUpdateRequest(string? playerGuid)
    {
        PlayerGuid = playerGuid;
    }

    public string? PlayerGuid { get; }
}
