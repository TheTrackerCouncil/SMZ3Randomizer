namespace Randomizer.Shared.Multiplayer;

public class JoinGameRequest : MultiplayerRequest
{
    public JoinGameRequest(string gameGuid, string playerName, string version)
    {
        GameGuid = gameGuid;
        PlayerName = playerName;
        Version = version;
    }

    public string PlayerName { get; init; }

    public string Version { get; }
}
