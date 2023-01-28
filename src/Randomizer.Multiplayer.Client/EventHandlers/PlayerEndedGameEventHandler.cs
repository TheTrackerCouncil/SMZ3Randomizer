namespace Randomizer.Multiplayer.Client.EventHandlers;

public delegate void PlayerEndedGameEventHandler(PlayerEndedGameEventHandlerArgs args);

public class PlayerEndedGameEventHandlerArgs
{
    public int? PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public string PhoneticName { get; init; } = "";
    public bool IsLocalPlayer { get; init; }
    public bool DidForfeit { get; init; }
    public bool DidComplete { get; init; }
    public bool SendItemsOnComplete { get; init; }
}
