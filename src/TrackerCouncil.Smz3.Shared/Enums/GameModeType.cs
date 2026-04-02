using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum GameModeType
{
    [Description("Vanilla")]
    Vanilla,

    // TODO: Revert to old name
    [Description("April Fools Event")]
    SpazerHunt
}
