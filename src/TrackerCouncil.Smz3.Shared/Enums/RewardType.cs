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
    [RewardCategory(RewardCategory.Zelda, RewardCategory.NonRandomized)]
    Agahnim,

    [Description("Green Pendant")]
    [RewardCategory(RewardCategory.Zelda, RewardCategory.Pendant)]
    PendantGreen,

    [Description("Red Pendant")]
    [RewardCategory(RewardCategory.Zelda, RewardCategory.Pendant)]
    PendantRed,

    [Description("Blue Crystal")]
    [RewardCategory(RewardCategory.Zelda, RewardCategory.Crystal)]
    CrystalBlue,

    [Description("Red Crystal")]
    [RewardCategory(RewardCategory.Zelda, RewardCategory.Crystal)]
    CrystalRed,

    [Description("Blue Pendant")]
    [RewardCategory(RewardCategory.Zelda, RewardCategory.Pendant)]
    PendantBlue,

    [Description("Metroid Boss Token")]
    [RewardCategory(RewardCategory.Metroid)]
    MetroidBoss,

    [Description("Kraid Boss Token")]
    [RewardCategory(RewardCategory.Metroid)]
    KraidToken,

    [Description("Phantoon Boss Token")]
    [RewardCategory(RewardCategory.Metroid)]
    PhantoonToken,

    [Description("Draygon Boss Token")]
    [RewardCategory(RewardCategory.Metroid)]
    DraygonToken,

    [Description("Ridley Boss Token")]
    [RewardCategory(RewardCategory.Metroid)]
    RidleyToken,
}
