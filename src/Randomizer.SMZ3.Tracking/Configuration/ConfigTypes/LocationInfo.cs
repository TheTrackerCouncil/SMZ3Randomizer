using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
{
    /// <summary>
    /// Represents extra information about a trackable location in SMZ3.
    /// </summary>
    public class LocationInfo : IPointOfInterest, IMergeable<LocationInfo>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LocationInfo() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationInfo"/> class
        /// with the specified info.
        /// </summary>
        /// <param name="id">
        /// The ID of the location. Must match an existing <see
        /// cref="Location.Id"/>.
        /// </param>
        /// <param name="name">The possible names for the location.</param>
        public LocationInfo(SchrodingersString name)
        {
            Name = name;
        }

        /// <summary>
        /// The internal SMZ3 location id for the location. Also used
        /// for matching configs for merging
        /// </summary>
        [MergeKey]
        public int LocationNumber { get; init; }

        /// <summary>
        /// Gets the possible names for the location.
        /// </summary>
        public SchrodingersString Name { get; set; }

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
        /// Gets the possible hints for the location, if any are defined.
        /// </summary>
        public SchrodingersString? Hints { get; set; }

        /// <summary>
        /// Gets the phrases to reply with when tracking a junk item at this location.
        /// </summary>
        public SchrodingersString? WhenTrackingJunk { get; set; }

        /// <summary>
        /// Gets the phrases to reply with when tracking a progression item at this location.
        /// </summary>
        public SchrodingersString? WhenTrackingProgression { get; set; }

        /// <summary>
        /// Gets the phrases to reply with when marking a junk item at this location.
        /// </summary>
        public SchrodingersString? WhenMarkingJunk { get; set; }

        /// <summary>
        /// Gets the phrases to reply with when marking a progression item at this location.
        /// </summary>
        public SchrodingersString? WhenMarkingProgression { get; set; }

        /// <summary>
        /// Gets the phrases to reply with when the location is out of logic
        /// </summary>
        public SchrodingersString? OutOfLogic { get; set; }

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
            => world.Locations.Single(x => x.Id == LocationNumber);

        /// <summary>
        /// Returns the location associated with the point of interest.
        /// </summary>
        /// <param name="world">
        /// The instance of the world whose locations to return.
        /// </param>
        /// <returns>
        /// A collection of locations containing this location.
        /// </returns>
        public IReadOnlyCollection<Location> GetLocations(World world)
            => new ReadOnlyCollection<Location>(new[] { GetLocation(world) });

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
        public bool IsAccessible(World world, Progression progression)
        {
            var location = GetLocation(world);
            return location.IsAvailable(progression);
        }

        /// <summary>
        /// Returns a string representation of the location.
        /// </summary>
        /// <returns>A string representation of this location.</returns>
        public override string? ToString() => Name[0];
    }
}
