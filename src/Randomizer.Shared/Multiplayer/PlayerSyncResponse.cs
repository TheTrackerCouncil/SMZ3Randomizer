namespace Randomizer.Shared.Multiplayer;

public class PlayerSyncResponse : MultiplayerResponse
{
    public PlayerSyncResponse(MultiplayerGameState gameState, MultiplayerPlayerState playerState) : base(gameState)
    {
        PlayerState = playerState;
    }

    public MultiplayerPlayerState PlayerState { get; }


}
