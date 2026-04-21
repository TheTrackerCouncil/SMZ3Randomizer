using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum GameModeType
{
    [Description("Collect crystals and defeat Metroid bosses")]
    Vanilla,

    [Description("All dungeons and defeat Metroid bosses")]
    AllDungeons,

    [Description("Spazer Hunt (auto tracking required)")]
    SpazerHunt
}
