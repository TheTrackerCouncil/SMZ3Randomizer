namespace Randomizer.Shared.Multiplayer;

public class MultiplayerResponse
{
    protected MultiplayerResponse(MultiplayerGameState gameState)
    {
        GameState = gameState;
    }

    public MultiplayerGameState GameState { get; }
}
