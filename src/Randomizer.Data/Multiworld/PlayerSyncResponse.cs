namespace Randomizer.Data.Multiworld;

public class PlayerSyncResponse : MultiworldResponse
{
    public MultiworldPlayerState PlayerState { get; init; } = null!;
}
