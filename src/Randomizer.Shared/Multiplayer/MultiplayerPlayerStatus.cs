using System.ComponentModel;

namespace Randomizer.Shared.Multiplayer;

public enum MultiplayerPlayerStatus
{
    [Description("Waiting on Config")]
    ConfigPending,

    [Description("Ready")]
    Ready,

    [Description("Playing")]
    Playing,

    [Description("Forfeit")]
    Forfeit,

    [Description("Completed")]
    Completed
}
