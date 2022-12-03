using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerTrackedBossEventHandler(PlayerTrackedBossEventHandlerArgs args);

public class PlayerTrackedBossEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public TrackerBossState BossState { get; init; } = null!;
    public bool IsLocalPlayer { get; init; }
}
