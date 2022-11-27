using Randomizer.Shared.Enums;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

public delegate void PlayerTrackedDungeonEventHandler(PlayerTrackedDungeonEventHandlerArgs args);

public class PlayerTrackedDungeonEventHandlerArgs
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = "";
    public string DungeonName { get; init; } = "";
    public bool IsLocalPlayer { get; init; }
}
