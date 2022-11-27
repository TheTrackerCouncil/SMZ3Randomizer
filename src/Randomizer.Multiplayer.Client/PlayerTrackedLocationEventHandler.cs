using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerTrackedLocationEventHandler(PlayerTrackedLocationEventHandlerArgs args);

public class PlayerTrackedLocationEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public int LocationId { get; init; }
    public bool IsLocalPlayer { get; init; }
    public ItemType ItemToGive { get; init; }
}
