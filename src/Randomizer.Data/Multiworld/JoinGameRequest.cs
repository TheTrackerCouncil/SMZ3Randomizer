using Randomizer.Data.Options;

namespace Randomizer.Data.Multiworld;

public class JoinGameRequest : MultiworldRequest
{
    public JoinGameRequest(string gameGuid, string playerName)
    {
        GameGuid = gameGuid;
        PlayerName = playerName;
    }

    public string PlayerName { get; init; }
}
