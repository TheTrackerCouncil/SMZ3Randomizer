using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Enums;

namespace Randomizer.Data.WorldData.Regions
{
    /// <summary>
    /// Defines a region that has a boss which is required to beat the game
    /// </summary>
    public interface IHasBoss
    {
        /// <summary>
        /// Gets or sets the SM golden boss for the region
        /// </summary>
        Boss Boss { get; set; }

        /// <summary>
        /// The boss type
        /// </summary>
        BossType BossType => Boss?.Type ?? BossType.None;

        /// <summary>
        /// Determines whether the boss for the region can be defeated.
        /// </summary>
        /// <param name="items">The items currently available.</param>
        /// <returns>
        /// <see langword="true"/> if the boss can be beaten; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool CanBeatBoss(Progression items);
    }
}
