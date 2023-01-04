using Randomizer.Shared;
using Randomizer.Shared.Models;

namespace Randomizer.Multiplayer.Client.EventHandlers;

public delegate void PlayerTrackedLocationEventHandler(PlayerTrackedLocationEventHandlerArgs args);

public class PlayerTrackedLocationEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public string PhoneticName { get; init; } = "";
    public TrackerLocationState LocationState { get; init; } = null!;
    public bool IsLocalPlayer { get; init; }
    public ItemType ItemToGive { get; init; }
}
