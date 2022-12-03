namespace Randomizer.Shared.Multiplayer;

public class JoinGameRequest
{
    public JoinGameRequest(string gameGuid, string playerName, string phoneticName, string randomizerVersion, int multiplayerVersion)
    {
        GameGuid = gameGuid;
        PlayerName = playerName;
        PhoneticName = phoneticName;
        RandomizerVersion = randomizerVersion;
        MultiplayerVersion = multiplayerVersion;
    }

    public string GameGuid { get; }

    public string PlayerName { get; }

    public string PhoneticName { get; }

    public string RandomizerVersion { get; }
    public int MultiplayerVersion { get; }
}
