namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class ForfeitPlayerGameResponse(MultiplayerGameState gameState, MultiplayerPlayerState playerstate)
    : MultiplayerResponse(gameState)
{
    public MultiplayerPlayerState PlayerState { get; } = playerstate;
}
