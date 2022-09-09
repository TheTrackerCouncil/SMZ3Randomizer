using Randomizer.Shared;

namespace Randomizer.Data.WorldData.Regions
{
    /// <summary>
    /// Defines a region that requires a medallion (Bombos, Ether, Quake) to be
    /// accessible.
    /// </summary>
    public interface INeedsMedallion
    {
        /// <summary>
        /// Gets or sets the type of medallion required to access the region.
        /// </summary>
        ItemType Medallion { get; set; }
    }
}
