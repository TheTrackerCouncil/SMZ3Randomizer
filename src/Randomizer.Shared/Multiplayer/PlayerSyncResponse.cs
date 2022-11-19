namespace Randomizer.Shared.Multiplayer;

public class PlayerSyncResponse : MultiplayerResponse
{
    public MultiplayerPlayerState? PlayerState { get; init; }

    public bool IsValid => IsSuccessful && PlayerState != null;
}
