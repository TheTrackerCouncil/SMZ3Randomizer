namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class CompletePlayerGameResponse(MultiplayerGameState gameState, MultiplayerPlayerState playerstate)
    : MultiplayerResponse(gameState)
{
    public MultiplayerPlayerState PlayerState { get; } = playerstate;
}
