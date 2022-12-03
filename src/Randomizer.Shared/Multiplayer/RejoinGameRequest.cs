namespace Randomizer.Shared.Multiplayer;

public class RejoinGameRequest
{
    public RejoinGameRequest(string gameGuid, string playerGuid, string playerKey, int multiplayerVersion)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
        MultiplayerVersion = multiplayerVersion;
    }

    public string GameGuid { get; }
    public string PlayerGuid { get; }
    public string PlayerKey { get; }
    public int MultiplayerVersion { get; }
}
