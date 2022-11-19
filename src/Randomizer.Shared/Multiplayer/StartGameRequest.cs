using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class StartGameRequest : MultiplayerRequest
{
    public StartGameRequest(List<MultiplayerPlayerState> playerStates)
    {
        PlayerStates = playerStates;
    }

    public List<MultiplayerPlayerState> PlayerStates { get; }

}
