namespace Randomizer.Shared.Multiplayer;

public class RejoinGameRequest
{
    public RejoinGameRequest(string gameGuid, string playerGuid, string playerKey)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
    }

    public string GameGuid { get; }
    public string PlayerGuid { get; }
    public string PlayerKey { get; }
}
