using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

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
