using System.ComponentModel;

namespace Randomizer.Shared
{
    /// <summary>
    /// Specifies the types of medallions used to open dungeons.
    /// </summary>
    public enum Medallion
    {
        /// <summary>
        /// No medallion is required.
        /// </summary>
        None = 0,

        /// <summary>
        /// Bombos is required.
        /// </summary>
        Bombos,

        /// <summary>
        /// Ether is required.
        /// </summary>
        Ether,

        /// <summary>
        /// Quake is required.
        /// </summary>
        Quake
    }

}
