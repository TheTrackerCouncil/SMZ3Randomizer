namespace Randomizer.Shared.Multiplayer;

public class CompletePlayerGameResponse : MultiplayerResponse
{
    public CompletePlayerGameResponse(MultiplayerGameState gameState, MultiplayerPlayerState playerstate) : base(gameState)
    {
        PlayerState = playerstate;
    }

    public MultiplayerPlayerState PlayerState { get; }
}
