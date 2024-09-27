using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

/// <summary>
/// Specifies the reward for completing a dungeon or boss.
/// </summary>
public enum RewardType
{
    [Description("Unknown")]
    None,
    [Description("Agahnim")]
    Agahnim,
    [Description("Green Pendant")]
    PendantGreen,
    [Description("Red Pendant")]
    PendantRed,
    [Description("Blue Crystal")]
    CrystalBlue,
    [Description("Red Crystal")]
    CrystalRed,
    [Description("Blue Pendant")]
    PendantBlue,
    [Description("Metroid Boss")]
    MetroidBoss,
}
