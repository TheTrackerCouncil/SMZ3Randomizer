using System.ComponentModel;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using static Randomizer.SMZ3.Tracking.Configuration.ConfigTypes.SchrodingersString;

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
            RewardItem.RedCrystal => new SchrodingersString(
                "Red crystal",
                new Possibility("Blood Red Crystal", 0.1),
                new Possibility("Bomb Purchasing Crystal", 0.1),
                new Possibility("Hyper realistic blood Red Crystal", 0.05),
                new Possibility("One of those crystals you hope you get late in the game so you don't have to check pyramid fairy", 0.05)
            ),
            RewardItem.GreenPendant => new SchrodingersString
            (
                "Green pendant",
                "Pendant of Courage",
                new Possibility("Slightly better pendant", 0.1),
                new Possibility("Less useless pendant", 0.1),
                new Possibility("That pendant that you turn into Sahasrahla as a last resort to hope you get something good", 0.05)
            ),
            RewardItem.RedPendant => new SchrodingersString
            (
                "Red pendant",
                "Pendant of Wisdom",
                new Possibility("Even worse pendant", 0.1),
                new Possibility("More useless pendant", 0.1),
                new Possibility("Time wasting pendant", 0.1),
                new Possibility("Pendant of pointlessness", 0.1),
                new Possibility("Pendant of Wisdom. I think. I get confused as they changed the colors later in this series", 0.05),
                new Possibility("Pendant of Power. Wait. I meant Wisdom. I get confused as they changed the colors later in this series", 0.05)
            ),
            RewardItem.BluePendant => new SchrodingersString
            (
                "Blue pendant",
                "Pendant of Power",
                new Possibility("Even worse pendant", 0.1),
                new Possibility("More useless pendant", 0.1),
                new Possibility("Time wasting pendant", 0.1),
                new Possibility("Pendant of pointlessness", 0.1),
                new Possibility("Pendant of Power. I think. I get confused as they changed the colors later in this series", 0.05),
                new Possibility("Pendant of Wisdom. Wait. I meant Power. I get confused as they changed the colors later in this series", 0.05)
            ),
            RewardItem.Agahnim => new SchrodingersString("Agahnim"),
            RewardItem.NonGreenPendant => new SchrodingersString
            (
                "Red or blue pendant",
                "Non green pendant",
                "Even worse pendant",
                "More useless pendant",
                "Pendant of pointlessness",
                "Time wasting pendant",
                "Pendant of power or wisdom or whatever",
                new Possibility("Pendant of power or wisdom or whatever. They changed the colors after this game and it's so hard to keep track.", 0.1)
            ),
            _ => throw new System.NotImplementedException(),
        };
    }
}
