using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class PlayerListSyncResponse : MultiplayerResponse
{
    public PlayerListSyncResponse(MultiplayerGameState gameState, List<MultiplayerPlayerState> allPlayers) : base(gameState)
    {
        AllPlayers = allPlayers;
    }

    public List<MultiplayerPlayerState> AllPlayers { get; }
}
