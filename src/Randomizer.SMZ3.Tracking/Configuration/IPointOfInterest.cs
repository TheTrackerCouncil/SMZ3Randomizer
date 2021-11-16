using System;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Defines a potential point of interest on the map.
    /// </summary>
    public interface IPointOfInterest
    {
        /// <summary>
        /// Gets the possible names for the point of interest.
        /// </summary>
        SchrodingersString Name { get; }

        /// <summary>
        /// Gets the x-coordinate of the POI on the map, if it should be
        /// displayed.
        /// </summary>
        int? X { get; }

        /// <summary>
        /// Gets the y-coordinate of the POI on the map, if it should be
        /// displayed.
        /// </summary>
        int? Y { get; }

        /// <summary>
        /// Determines whether the point of interest is accessible with the
        /// specified set of items.
        /// </summary>
        /// <param name="world">
        /// The instance of the world that contains the point of interest.
        /// </param>
        /// <param name="progression">The available items.</param>
        /// <returns>
        /// <c>true</c> if the point of interest is accessible; otherwise,
        /// <c>false</c>.
        /// </returns>
        bool IsAccessible(World world, Progression progression);
    }
}
