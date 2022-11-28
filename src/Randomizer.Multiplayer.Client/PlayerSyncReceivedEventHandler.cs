using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerSyncReceivedEventHandler(PlayerSyncReceivedEventHandlerArgs args);

public class PlayerSyncReceivedEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public ICollection<ItemType>? ItemsToGive { get; init; }
    public bool IsLocalPlayer { get; init; }
    public bool DidForfeit { get; init; }
}
