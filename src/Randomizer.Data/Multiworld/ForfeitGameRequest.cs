using Randomizer.Data.Options;

namespace Randomizer.Data.Multiworld;

public class ForfeitGameRequest : MultiworldRequest
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
