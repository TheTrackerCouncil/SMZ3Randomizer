using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class StartGameResponse : MultiplayerResponse
{
    public List<MultiplayerPlayerState>? Players { get; init; }
}
