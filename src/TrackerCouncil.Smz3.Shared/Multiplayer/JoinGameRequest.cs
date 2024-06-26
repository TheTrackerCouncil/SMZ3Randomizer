namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class JoinGameRequest(
    string gameGuid,
    string playerName,
    string phoneticName,
    string randomizerVersion,
    int multiplayerVersion)
{
    public string GameGuid { get; } = gameGuid;

    public string PlayerName { get; } = playerName;

    public string PhoneticName { get; } = phoneticName;

    public string RandomizerVersion { get; } = randomizerVersion;
    public int MultiplayerVersion { get; } = multiplayerVersion;
}
