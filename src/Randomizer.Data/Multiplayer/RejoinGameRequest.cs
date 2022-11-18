using Randomizer.Data.Options;

namespace Randomizer.Data.Multiplayer;

public class RejoinGameRequest : MultiplayerRequest
{
    public RejoinGameRequest(string gameGuid, string playerGuid, string playerKey)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
    }
}
