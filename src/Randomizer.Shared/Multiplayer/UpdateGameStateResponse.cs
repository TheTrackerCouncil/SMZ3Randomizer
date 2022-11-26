namespace Randomizer.Shared.Multiplayer;

public class UpdateGameStateResponse : MultiplayerResponse
{
    public UpdateGameStateResponse(MultiplayerGameState gameState) : base(gameState)
    {
    }
}
