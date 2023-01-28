namespace Randomizer.Shared.Multiplayer;

public class ForfeitPlayerGameResponse : MultiplayerResponse
{
    public ForfeitPlayerGameResponse(MultiplayerGameState gameState, MultiplayerPlayerState playerstate) : base(gameState)
    {
        PlayerState = playerstate;
    }

    public MultiplayerPlayerState PlayerState { get; }
}
