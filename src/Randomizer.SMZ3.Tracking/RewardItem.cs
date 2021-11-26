using System.ComponentModel;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking
{
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
            RewardItem.Crystal => new SchrodingersString("Crystal", "Blue crystal"),
            RewardItem.RedCrystal => new SchrodingersString("Red crystal"),
            RewardItem.GreenPendant => new SchrodingersString("Green pendant", "Pendant of Courage"),
            RewardItem.RedPendant => new SchrodingersString("Red pendant", "Pendant of Wisdom"),
            RewardItem.BluePendant => new SchrodingersString("Blue pendant", "Pendant of Courage"),
            RewardItem.Agahnim => new SchrodingersString("Agahnim"),
            _ => throw new System.NotImplementedException(),
        };
    }
}
