namespace Randomizer.Shared.Multiplayer;

public class CreateGameRequest
{
    public CreateGameRequest(string playerName, string phoneticName, MultiplayerGameType gameType, string version)
    {
        PlayerName = playerName;
        PhoneticName = phoneticName;
        GameType = gameType;
        Version = version;
    }

    public string PlayerName { get; }

    public string PhoneticName { get; }

    public MultiplayerGameType GameType { get; }

    public string Version { get; }
}
