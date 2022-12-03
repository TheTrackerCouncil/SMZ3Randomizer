namespace Randomizer.Shared.Multiplayer;

public class CompleteGameResponse : MultiplayerResponse
{
    public CompleteGameResponse(MultiplayerGameState gameState, MultiplayerPlayerState playerstate) : base(gameState)
    {
        PlayerState = playerstate;
    }

    public MultiplayerPlayerState PlayerState { get; }
}
