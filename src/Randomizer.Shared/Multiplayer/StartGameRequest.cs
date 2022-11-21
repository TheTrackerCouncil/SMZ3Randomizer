using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class StartGameRequest : MultiplayerRequest
{
    public StartGameRequest(string gameGuid, string playerGuid, string playerKey, string seed, string validationHash)
    {
        GameGuid = gameGuid;
        PlayerGuid = playerGuid;
        PlayerKey = playerKey;
        Seed = seed;
        ValidationHash = validationHash;
    }

    public string Seed { get; }
    public string ValidationHash { get; }
}
