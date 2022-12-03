using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerTrackedLocationEventHandler(PlayerTrackedLocationEventHandlerArgs args);

public class PlayerTrackedLocationEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public TrackerLocationState LocationState { get; init; } = null!;
    public bool IsLocalPlayer { get; init; }
    public ItemType ItemToGive { get; init; }
}
