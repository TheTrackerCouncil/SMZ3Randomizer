namespace Randomizer.Shared.Multiplayer;

public class ForfeitGameRequest
{
    public ForfeitGameRequest(string playerGuid)
    {
        PlayerGuid = playerGuid;
    }

    public string PlayerGuid { get; }
}
