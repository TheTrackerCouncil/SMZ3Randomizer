using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class StartGameRequest
{
    public StartGameRequest(string validationHash)
    {
        ValidationHash = validationHash;
    }

    public string ValidationHash { get; }
}
