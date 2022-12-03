using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerSyncReceivedEventHandler(PlayerSyncReceivedEventHandlerArgs args);

public class PlayerSyncReceivedEventHandlerArgs
{
    public int? PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public ICollection<TrackerLocationState> UpdatedLocationStates { get; init; } = null!;
    public ICollection<(TrackerItemState State, int TrackingValue)> UpdatedItemStates { get; init; } = null!;
    public ICollection<TrackerBossState> UpdatedBossStates { get; init; } = null!;
    public ICollection<TrackerDungeonState> UpdatedDungeonStates { get; init; } = null!;
    public ICollection<ItemType>? ItemsToGive { get; init; }
    public bool IsLocalPlayer { get; init; }
    public bool DidForfeit { get; init; }
    public bool DidComplete { get; init; }
}
