using System.ComponentModel;

using Randomizer.SMZ3.Tracking.Vocabulary;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Specifies the type of pendant or crystal that is rewarded when
    /// completing a dungeon.
    /// </summary>
    public enum RewardItem
    {
        /// <summary>
        /// Specifies the reward is unknown.
        /// </summary>
        [Description("Unknown")]
        Unknown = 0,

        /// <summary>
        /// Specifies the reward is the green pendant.
        /// </summary>
        [Description("Green pendant")]
        GreenPendant,

        /// <summary>
        /// Specifies the reward is the red pendant.
        /// </summary>
        [Description("Red pendant")]
        RedPendant,

        /// <summary>
        /// Specifies the reward is the blue pendant.
        /// </summary>
        [Description("Blue pendant")]
        BluePendant,

        /// <summary>
        /// Specifies the reward is a blue or red crystal.
        /// </summary>
        [Description("Crystal")]
        Crystal,
    }

    /// <summary>
    /// Provides functionality for the <see cref="RewardItem"/> enumeration.
    /// </summary>
    public static class RewardItemExtensions
    {
        /// <summary>
        /// Returns the possible names for the reward item.
        /// </summary>
        /// <param name="reward">The reward item.</param>
        /// <returns>
        /// A new <see cref="SchrodingersString"/> that represents the possible
        /// names.
        /// </returns>
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
