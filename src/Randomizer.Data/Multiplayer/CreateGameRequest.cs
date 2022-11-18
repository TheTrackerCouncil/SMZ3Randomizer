namespace Randomizer.Data.Multiplayer;

public class CreateGameRequest : MultiplayerRequest
{
    public CreateGameRequest(string playerName)
    {
        PlayerName = playerName;
    }

    public string PlayerName { get; }
}
