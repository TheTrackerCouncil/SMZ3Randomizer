using System.ComponentModel;

namespace Randomizer.SMZ3
{
    /// <summary>
    /// Specifies the reward for completing a dungeon or boss.
    /// </summary>
    public enum Reward
    {
        [Description("None")]
        None,
        [Description("Agahnim")]
        Agahnim,
        [Description("Green Pendant")]
        PendantGreen,
        [Description("Blue/Red Pendant")]
        PendantNonGreen,
        [Description("Blue Crystal")]
        CrystalBlue,
        [Description("Red Crystal")]
        CrystalRed,
        [Description("Golden Four Boss")]
        GoldenFourBoss
    }

}
