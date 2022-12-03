namespace Randomizer.Shared.Multiplayer;

public class ForfeitGameResponse : MultiplayerResponse
{
    public ForfeitGameResponse(MultiplayerGameState gameState, MultiplayerPlayerState playerstate) : base(gameState)
    {
        PlayerState = playerstate;
    }

    public MultiplayerPlayerState PlayerState { get; }
}
