using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum BossType
{
    None,
    [Description("Kraid")]
    Kraid,
    [Description("Phantoon")]
    Phantoon,
    [Description("Draygon")]
    Draygon,
    [Description("Ridley")]
    Ridley,
}
