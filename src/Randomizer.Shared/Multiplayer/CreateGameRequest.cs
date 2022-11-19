namespace Randomizer.Shared.Multiplayer;

public class CreateGameRequest : MultiplayerRequest
{
    public CreateGameRequest(string playerName, MultiplayerGameType gameType)
    {
        PlayerName = playerName;
        GameType = gameType;
    }

    public string PlayerName { get; }

    public MultiplayerGameType GameType { get; }
}
