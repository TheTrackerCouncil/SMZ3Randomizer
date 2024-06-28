namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class PlayerSyncResponse(MultiplayerGameState gameState, MultiplayerPlayerState playerState)
    : MultiplayerResponse(gameState)
{
    public MultiplayerPlayerState PlayerState { get; } = playerState;
}
