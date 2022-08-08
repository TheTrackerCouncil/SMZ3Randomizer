using System;
using System.Collections.Generic;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

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
        /// Returns the locations associated with the point of interest.
        /// </summary>
        /// <param name="world">
        /// The instance of the world whose locations to return.
        /// </param>
        /// <returns>
        /// A collection of locations from the specified world that are associated
        /// with this point of interest.
        /// </returns>
        IReadOnlyCollection<Location> GetLocations(World world);

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
