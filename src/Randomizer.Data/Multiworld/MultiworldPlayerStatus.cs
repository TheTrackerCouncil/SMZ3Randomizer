using System.ComponentModel;

namespace Randomizer.Data.Multiworld;

public enum MultiworldPlayerStatus
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
    Completed,

    [Description("Disconnected")]
    Disconnected,
}
