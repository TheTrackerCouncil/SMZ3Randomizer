using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class PlayerListSyncResponse(MultiplayerGameState gameState, List<MultiplayerPlayerState> allPlayers)
    : MultiplayerResponse(gameState)
{
    public List<MultiplayerPlayerState> AllPlayers { get; } = allPlayers;
}
