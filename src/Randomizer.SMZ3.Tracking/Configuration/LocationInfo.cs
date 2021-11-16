using System;
using System.Linq;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Represents extra information about a trackable location in SMZ3.
    /// </summary>
    public class LocationInfo : IPointOfInterest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationInfo"/> class
        /// with the specified info.
        /// </summary>
        /// <param name="id">
        /// The ID of the location. Must match an existing <see
        /// cref="Location.Id"/>.
        /// </param>
        /// <param name="name">The possible names for the location.</param>
        public LocationInfo(int id, SchrodingersString name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Gets the ID of the location. This value must match the <see
        /// cref="Location.Id"/> of a location in the game.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the possible names for the location.
        /// </summary>
        public SchrodingersString Name { get; }

        /// <summary>
        /// Gets the x-coordinate of the location on the map, if it should be
        /// displayed.
        /// </summary>
        public int? X { get; init; }

        /// <summary>
        /// Gets the y-coordinate of the location on the map, if it should be
        /// displayed.
        /// </summary>
        public int? Y { get; init; }

        /// <summary>
        /// Gets the scale of the location, if it should be displayed on a map.
        /// </summary>
        public double? Scale { get; init; }

        /// <summary>
        /// Returns the <see cref="Location"/> that matches the location info in
        /// the specified world.
        /// </summary>
        /// <param name="world">The world to find the location in.</param>
        /// <returns>
        /// A matching <see cref="Location"/> for the current location info.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// There is no matching location in <paramref name="world"/>. -or-
        /// There is more than one matching location in <paramref
        /// name="world"/>.
        /// </exception>
        public Location GetLocation(World world)
            => world.Locations.Single(x => x.Id == Id);
    }
}
