namespace TrackerCouncil.Smz3.Multiplayer.Client.EventHandlers;

public delegate void PlayerTrackedDeathEventHandler(PlayerTrackedDeathEventHandlerArgs args);

public class PlayerTrackedDeathEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public string PhoneticName { get; init; } = "";
    public bool IsLocalPlayer { get; init; }
    public bool DeathLinkEnabled { get; init; }
}
