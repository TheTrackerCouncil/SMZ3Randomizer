using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;

public delegate void PlayerTrackedBossEventHandler(PlayerTrackedBossEventHandlerArgs args);

public class PlayerTrackedBossEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public string PhoneticName { get; init; } = "";
    public TrackerBossState BossState { get; init; } = null!;
    public bool IsLocalPlayer { get; init; }
}
