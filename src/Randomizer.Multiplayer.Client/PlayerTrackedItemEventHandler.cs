using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerTrackedItemEventHandler(PlayerTrackedItemEventHandlerArgs args);

public class PlayerTrackedItemEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public ItemType ItemType { get; init; }
    public int TrackingValue { get; init; }
    public bool IsLocalPlayer { get; init; }
}
