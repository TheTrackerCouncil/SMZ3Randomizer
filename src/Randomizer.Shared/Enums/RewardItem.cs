using System.ComponentModel;

namespace Randomizer.Shared
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
        /// Specifies the reward is a blue crystal.
        /// </summary>
        [Description("Crystal")]
        Crystal = 1,

        /// <summary>
        /// Specifies the reward is a red crystal, used to get the super bomb.
        /// </summary>
        [Description("Red crystal")]
        RedCrystal = 2,

        /// <summary>
        /// Specifies the reward is the green pendant.
        /// </summary>
        [Description("Green pendant")]
        GreenPendant = 3,

        /// <summary>
        /// Specifies the reward is the red pendant.
        /// </summary>
        [Description("Red pendant")]
        RedPendant = 4,

        /// <summary>
        /// Specifies the reward is the blue pendant.
        /// </summary>
        [Description("Blue pendant")]
        BluePendant = 5,

        /// <summary>
        /// Specifies the "reward" is Agahnim.
        /// </summary>
        [Description("Agahnim")]
        Agahnim = 6,
    }

}
