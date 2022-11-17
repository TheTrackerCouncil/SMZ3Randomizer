using Randomizer.Data.Options;

namespace Randomizer.Data.Multiworld;

public class RejoinGameRequest : MultiworldRequest
{
    public RejoinGameRequest(string gameGuid, string playerGuid, string playerKey)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
    }
}
