namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class RejoinGameRequest(string gameGuid, string playerGuid, string playerKey, int multiplayerVersion)
{
    public string GameGuid { get; } = gameGuid;
    public string PlayerGuid { get; } = playerGuid;
    public string PlayerKey { get; } = playerKey;
    public int MultiplayerVersion { get; } = multiplayerVersion;
}
