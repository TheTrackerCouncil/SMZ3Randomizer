namespace Randomizer.Shared.Multiplayer;

public class JoinGameRequest
{
    public JoinGameRequest(string gameGuid, string playerName, string phoneticName, string version)
    {
        GameGuid = gameGuid;
        PlayerName = playerName;
        PhoneticName = phoneticName;
        Version = version;
    }

    public string GameGuid { get; }

    public string PlayerName { get; }

    public string PhoneticName { get; }

    public string Version { get; }
}
