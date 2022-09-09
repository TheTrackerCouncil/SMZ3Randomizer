using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Provides additional functionality for points of interest.
    /// </summary>
    public static class PointOfInterest
    {
        /// <summary>
        /// Determines whether the specified point of interest can be shown on a
        /// map.
        /// </summary>
        /// <param name="poi">The point of interest.</param>
        /// <returns>
        /// <c>true</c> if the point of interest can be shown on a map;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool CanShowOnMap(this IPointOfInterest poi)
            => poi.X != null && poi.Y != null;

        /// <summary>
        /// Returns the locations associated with the point of interest which
        /// are in logic with the specified items.
        /// </summary>
        /// <param name="poi">The point of interest.</param>
        /// <param name="world">
        /// The instance of the world whose locations to return.
        /// </param>
        /// <param name="progression">The available items.</param>
        /// <returns>
        /// A collection of available locations from the specified world that
        /// are associated with this point of interest.
        /// </returns>
        public static IReadOnlyCollection<Location> GetAccessibleLocations(
            this IPointOfInterest poi, World world, Progression progression)
        {
            return poi.GetLocations(world)
                .Where(x => x.IsAvailable(progression))
                .ToImmutableList();
        }

        /// <summary>
        /// Returns the locations associated with the point of interest which
        /// have been cleared.
        /// </summary>
        /// <param name="poi">The point of interest.</param>
        /// <param name="world">
        /// The instance of the world whose locations to return.
        /// </param>
        /// <returns>
        /// A collection of cleared locations from the specified world that are
        /// associated with this point of interest.
        /// </returns>
        public static IReadOnlyCollection<Location> GetClearedLocations(
            this IPointOfInterest poi, World world)
        {
            return poi.GetLocations(world)
                .Where(x => x.Cleared)
                .ToImmutableList();
        }

        /// <summary>
        /// Returns the locations associated with the point of interest which
        /// are out of logic and have not be cleared with glitches.
        /// </summary>
        /// <param name="poi">The point of interest.</param>
        /// <param name="world">
        /// The instance of the world whose locations to return.
        /// </param>
        /// <param name="progression">The available items.</param>
        /// <returns>
        /// A collection of locations which are not available and have not been
        /// cleared from the specified world that are associated with this point
        /// of interest.
        /// </returns>
        public static IReadOnlyCollection<Location> GetInaccessibleLocations(
            this IPointOfInterest poi, World world, Progression progression)
        {
            return poi.GetLocations(world)
                .Where(x => !x.Cleared && !x.IsAvailable(progression))
                .ToImmutableList();
        }
    }
}
