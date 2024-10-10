using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum BossType
{
    None,

    [Description("Kraid")]
    [BossCategory(BossCategory.Metroid, BossCategory.GoldenFour, BossCategory.HasReward)]
    Kraid,

    [Description("Phantoon")]
    [BossCategory(BossCategory.Metroid, BossCategory.GoldenFour, BossCategory.HasReward)]
    Phantoon,

    [Description("Draygon")]
    [BossCategory(BossCategory.Metroid, BossCategory.GoldenFour, BossCategory.HasReward)]
    Draygon,

    [Description("Ridley")]
    [BossCategory(BossCategory.Metroid, BossCategory.GoldenFour, BossCategory.HasReward)]
    Ridley,

    [Description("Mother Brain")]
    [BossCategory(BossCategory.Metroid)]
    MotherBrain,

    [Description("Castle Guard")]
    [BossCategory(BossCategory.Zelda)]
    CastleGuard,

    [Description("Armos Knights")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    ArmosKnights,

    [Description("Lanmolas")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    Lanmolas,

    [Description("Moldorm")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    Moldorm,

    [Description("Helmasaur King")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    HelmasaurKing,

    [Description("Arrghus")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    Arrghus,

    [Description("Blind")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    Blind,

    [Description("Mothula")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    Mothula,

    [Description("Kholdstare")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    Kholdstare,

    [Description("Vitreous")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    Vitreous,

    [Description("Trinexx")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    Trinexx,

    [Description("Agahnim")]
    [BossCategory(BossCategory.Zelda, BossCategory.HasReward)]
    Agahnim,

    [Description("Ganon")]
    [BossCategory(BossCategory.Zelda)]
    Ganon,
}
