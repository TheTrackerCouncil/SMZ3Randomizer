using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;

public delegate void PlayerSyncReceivedEventHandler(PlayerSyncReceivedEventHandlerArgs args);

public class PlayerSyncReceivedEventHandlerArgs
{
    public int? PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public string PhoneticName { get; init; } = "";
    public ICollection<TrackerLocationState> UpdatedLocationStates { get; init; } = null!;
    public ICollection<(TrackerItemState State, int TrackingValue)> UpdatedItemStates { get; init; } = null!;
    public ICollection<TrackerBossState> UpdatedBossStates { get; init; } = null!;
    public ICollection<ItemType>? ItemsToGive { get; init; }
    public bool IsLocalPlayer { get; init; }
    public bool DidForfeit { get; init; }
    public bool DidComplete { get; init; }
}
