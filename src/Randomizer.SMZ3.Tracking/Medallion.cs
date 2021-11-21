using System;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking
{
    

    /// <summary>
    /// Provides functionality for the <see cref="Medallion"/> enumeration.
    /// </summary>
    public static class MedallionExtensions
    {
        /// <summary>
        /// Converts the medallion to the equivalent <see cref="ItemType"/>.
        /// </summary>
        /// <param name="medallion">The type of medallion.</param>
        /// <returns>
        /// The <see cref="ItemType"/> that is the equivalent of <paramref
        /// name="medallion"/>.
        /// </returns>
        public static ItemType ToItemType(this Medallion medallion) => medallion switch
        {
            Medallion.Bombos => ItemType.Bombos,
            Medallion.Ether => ItemType.Ether,
            Medallion.Quake => ItemType.Quake,
            _ => ItemType.Nothing
        };
    }
}
