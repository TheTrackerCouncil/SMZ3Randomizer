using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum RomGenerator
{
    [Description("SMZ3 Cas'")]
    Cas,

    [Description("Original SMZ3 Site")]
    Mainline,

    [Description("Archipelago")]
    Archipelago,
}
