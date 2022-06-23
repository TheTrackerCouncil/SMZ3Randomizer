using System.ComponentModel;
using Randomizer.Shared;
using static Randomizer.SMZ3.Tracking.SchrodingersString;

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
            RewardItem.BluePendant => new SchrodingersString("Blue pendant", "Pendant of Power"),
            RewardItem.Agahnim => new SchrodingersString("Agahnim"),
            RewardItem.NonGreenPendant => new SchrodingersString("Red or blue pendant", "Non green pendant", "Even worse pendant", "More useless pendant", "Pendant of pointlessness", "Time wasting pendant", "Pendant of power or wisdom or whatever", new Possibility("Pendant of power or wisdom or whatever. They changed the colors after this game and it's so hard to keep track.", 0.1)),
            _ => throw new System.NotImplementedException(),
        };
    }
}
