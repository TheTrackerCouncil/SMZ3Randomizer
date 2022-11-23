namespace Randomizer.Shared.Multiplayer;

public class CreateGameRequest : MultiplayerRequest
{
    public CreateGameRequest(string playerName, MultiplayerGameType gameType, string version)
    {
        PlayerName = playerName;
        GameType = gameType;
        Version = version;
    }

    public string PlayerName { get; }

    public MultiplayerGameType GameType { get; }

    public string Version { get; }
}
