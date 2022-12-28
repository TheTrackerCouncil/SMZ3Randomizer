using System.Collections.Generic;

namespace Randomizer.Shared.Multiplayer;

public class StartGameResponse : MultiplayerResponse
{
    public StartGameResponse(MultiplayerGameState gameState, List<MultiplayerPlayerState> allPlayers,
        List<string> playerGenerationData) : base(gameState)
    {
        AllPlayers = allPlayers;
        PlayerGenerationData = playerGenerationData;
    }

    public List<MultiplayerPlayerState> AllPlayers { get; }
    public List<string> PlayerGenerationData { get; }
}
