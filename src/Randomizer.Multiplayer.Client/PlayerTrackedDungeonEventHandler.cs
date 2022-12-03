using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerTrackedDungeonEventHandler(PlayerTrackedDungeonEventHandlerArgs args);

public class PlayerTrackedDungeonEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public TrackerDungeonState DungeonState { get; init; } = null!;
    public bool IsLocalPlayer { get; init; }
}
