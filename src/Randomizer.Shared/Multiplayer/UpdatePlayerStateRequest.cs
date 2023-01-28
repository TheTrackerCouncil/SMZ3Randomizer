namespace Randomizer.Shared.Multiplayer;

public class UpdatePlayerStateRequest
{
    public UpdatePlayerStateRequest(MultiplayerPlayerState state, bool propagate = true)
    {
        State = state;
        Propagate = propagate;
    }

    public MultiplayerPlayerState State { get; }

    public bool Propagate { get; }

}
