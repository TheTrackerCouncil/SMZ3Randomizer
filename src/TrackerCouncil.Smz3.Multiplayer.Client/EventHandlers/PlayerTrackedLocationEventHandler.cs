using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;

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
