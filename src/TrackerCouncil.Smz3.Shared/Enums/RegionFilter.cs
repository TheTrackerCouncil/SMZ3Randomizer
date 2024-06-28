using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum RegionFilter
{
    [Description("Both games")]
    None = 0,

    [Description("Zelda only")]
    ZeldaOnly = 1,

    [Description("Metroid only")]
    MetroidOnly = 2
}
