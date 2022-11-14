namespace Randomizer.Data.Multiworld;

public class PlayerSyncResponse : MultiworldResponse
{
    public MultiworldPlayerState? PlayerState { get; init; }

    public bool IsValid => IsSuccessful && PlayerState != null;
}
