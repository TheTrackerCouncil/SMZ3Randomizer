using Randomizer.Shared.Models;

namespace Randomizer.Multiplayer.Client.EventHandlers;

public delegate void PlayerTrackedItemEventHandler(PlayerTrackedItemEventHandlerArgs args);

public class PlayerTrackedItemEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public string PhoneticName { get; init; } = "";
    public TrackerItemState ItemState { get; init; } = null!;
    public int TrackingValue { get; init; }
    public bool IsLocalPlayer { get; init; }
}
