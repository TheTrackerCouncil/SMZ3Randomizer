using Randomizer.Data.Options;

namespace Randomizer.Data.Multiplayer;

public class JoinGameRequest : MultiplayerRequest
{
    public JoinGameRequest(string gameGuid, string playerName)
    {
        GameGuid = gameGuid;
        PlayerName = playerName;
    }

    public string PlayerName { get; init; }
}
