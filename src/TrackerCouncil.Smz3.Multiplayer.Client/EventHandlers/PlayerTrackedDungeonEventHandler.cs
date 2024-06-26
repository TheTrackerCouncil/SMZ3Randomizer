using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;

public delegate void PlayerTrackedDungeonEventHandler(PlayerTrackedDungeonEventHandlerArgs args);

public class PlayerTrackedDungeonEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public string PhoneticName { get; init; } = "";
    public TrackerDungeonState DungeonState { get; init; } = null!;
    public bool IsLocalPlayer { get; init; }
}
