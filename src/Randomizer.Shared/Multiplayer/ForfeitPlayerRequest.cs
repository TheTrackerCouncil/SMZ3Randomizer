namespace Randomizer.Shared.Multiplayer;

public class ForfeitPlayerGameRequest
{
    public ForfeitPlayerGameRequest(string playerGuid)
    {
        PlayerGuid = playerGuid;
    }

    public string PlayerGuid { get; }
}
