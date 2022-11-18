using System.ComponentModel;

namespace Randomizer.Shared
{
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
    }

}
