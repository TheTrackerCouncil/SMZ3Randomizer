using Randomizer.Data.Options;

namespace Randomizer.Data.Multiplayer;

public class ForfeitGameRequest : MultiplayerRequest
{
    public ForfeitGameRequest(string gameGuid, string playerGuid, string playerKey, string forfeitPlayerGuid)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
        ForfeitPlayerGuid = forfeitPlayerGuid;
    }

    public string ForfeitPlayerGuid { get; }
}
