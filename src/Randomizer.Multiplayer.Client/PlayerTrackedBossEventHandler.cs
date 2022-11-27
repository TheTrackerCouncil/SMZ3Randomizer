using Randomizer.Shared.Enums;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerTrackedBossEventHandler(PlayerTrackedBossEventHandlerArgs args);

public class PlayerTrackedBossEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public BossType BossType { get; init; }
    public bool IsLocalPlayer { get; init; }
}
