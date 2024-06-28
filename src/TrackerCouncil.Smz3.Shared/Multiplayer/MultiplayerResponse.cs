namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class MultiplayerResponse
{
    protected MultiplayerResponse(MultiplayerGameState gameState)
    {
        GameState = gameState;
    }

    public MultiplayerGameState GameState { get; }
}
