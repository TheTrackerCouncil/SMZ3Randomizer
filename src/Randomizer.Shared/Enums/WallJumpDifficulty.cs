using System;
using System.ComponentModel;

namespace Randomizer.Shared
{
    /// <summary>
    /// Specifies the difficulty level wall jump tricks.
    /// </summary>
    /// <remarks>
    /// See
    /// https://docs.google.com/document/d/12zxeKyZT1LOMpnxzbA2dIVYEiZSFe3iXbPZrazd6BQs/edit
    /// for details.
    /// </remarks>
    public enum WallJumpDifficulty
    {
        /// <summary>
        /// No wall jumps at all.
        /// </summary>
        None = 0,

        /// <summary>
        /// Basic and straightforward wall jumps.
        /// </summary>
        [Description("Relatively straightforward Wall Jump tricks, such as the Etecoon room in Green Brinstar.")]
        Easy = 1,

        /// <summary>
        /// Wall jumps that require multiple consecutive wall jumps.
        /// </summary>
        [Description("The difficulty generally expected of you by the original SMZ3 randomizer. Wall Jump tricks requiring multiple consecutive wall jumps, such as the upper half of the Red Tower in Red Brinstar.")]
        Medium = 2,

        /// <summary>
        /// Precise wall jumps.
        /// </summary>
        [Description("Precise Wall Jumps such as Bubble Mountain.")]
        Hard = 3,

        /// <summary>
        /// Very precise or even frame perfect wall jumps.
        /// </summary>
        [Description("Very precise and sometimes frame perfect Wall Jumps.")]
        Insane = 4
    }
}
