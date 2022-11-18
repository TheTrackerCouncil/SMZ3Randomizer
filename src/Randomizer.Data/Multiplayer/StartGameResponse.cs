using System.Collections.Generic;
using Randomizer.Data.WorldData;

namespace Randomizer.Data.Multiplayer;

public class StartGameResponse : MultiplayerResponse
{
    public List<MultiplayerPlayerState>? Players { get; init; }
}
