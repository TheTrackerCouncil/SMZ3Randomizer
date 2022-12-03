namespace Randomizer.Shared.Multiplayer;

public class JoinGameRequest
{
    public JoinGameRequest(string gameGuid, string playerName, string version)
    {
        GameGuid = gameGuid;
        PlayerName = playerName;
        Version = version;
    }

    public string GameGuid { get; }

    public string PlayerName { get; init; }

    public string Version { get; }
}
