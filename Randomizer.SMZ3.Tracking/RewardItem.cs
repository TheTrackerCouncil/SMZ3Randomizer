using System.ComponentModel;

namespace Randomizer.SMZ3.Tracking
{
    public enum RewardItem
    {
        [Description("Unknown")]
        Unknown = 0,

        [Description("Green pendant")]
        GreenPendant,

        [Description("Red pendant")]
        RedPendant,

        [Description("Blue pendant")]
        BluePendant,

        [Description("Blue crystal")]
        BlueCrystal,

        [Description("Red crystal")]
        RedCrystal
    }
}
