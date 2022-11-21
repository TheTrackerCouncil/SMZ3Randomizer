namespace Randomizer.Shared.Multiplayer;

public class UpdateGameStatusResponse : MultiplayerResponse
{
    public UpdateGameStatusResponse(MultiplayerGameState gameState) : base(gameState)
    {
    }
}
