using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class JoinGameResponse(
    MultiplayerGameState gameState,
    string playerGuid,
    string playerKey,
    List<MultiplayerPlayerState> allPlayers)
    : MultiplayerResponse(gameState)
{
    public string PlayerGuid { get; } = playerGuid;
    public string PlayerKey { get; } = playerKey;
    public List<MultiplayerPlayerState> AllPlayers { get; } = allPlayers;
}
