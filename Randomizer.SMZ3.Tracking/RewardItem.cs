using System.ComponentModel;

using Randomizer.SMZ3.Tracking.Vocabulary;

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

        [Description("Crystal")]
        Crystal,
    }

    public static class RewardItemExtensions
    {
        public static SchrodingersString GetName(this RewardItem reward) => reward switch
        {
            RewardItem.Unknown => new SchrodingersString("Unknown"),
            RewardItem.GreenPendant => new SchrodingersString("Green pendant", "Pendant of Courage"),
            RewardItem.RedPendant => new SchrodingersString("Red pendant", "Pendant of Wisdom"),
            RewardItem.BluePendant => new SchrodingersString("Blue pendant", "Pendant of Courage"),
            RewardItem.Crystal => new SchrodingersString("Crystal"),
            _ => throw new System.NotImplementedException(),
        };
    }
}
