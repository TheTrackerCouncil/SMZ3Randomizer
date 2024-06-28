using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class StartGameResponse(
    MultiplayerGameState gameState,
    List<MultiplayerPlayerState> allPlayers,
    List<string> playerGenerationData)
    : MultiplayerResponse(gameState)
{
    public List<MultiplayerPlayerState> AllPlayers { get; } = allPlayers;
    public List<string> PlayerGenerationData { get; } = playerGenerationData;
}
