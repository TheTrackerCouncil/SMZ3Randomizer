using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class StartGameRequest
{
    public StartGameRequest(string seed, string validationHash)
    {
        Seed = seed;
        ValidationHash = validationHash;
    }

    public string Seed { get; }
    public string ValidationHash { get; }
}
