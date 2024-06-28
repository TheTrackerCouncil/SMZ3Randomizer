namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class UpdatePlayerStateRequest(MultiplayerPlayerState state, bool propagate = true)
{
    public MultiplayerPlayerState State { get; } = state;

    public bool Propagate { get; } = propagate;
}
