using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Randomizer.SMZ3
{
    /// <summary>
    /// Represents a room or section in a region.
    /// </summary>
    public abstract class Room
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class that has
        /// no alternate names.
        /// </summary>
        /// <param name="region">The region the room is located in.</param>
        /// <param name="name">The name of the room.</param>
        public Room(Region region, string name)
            : this(region, name, Array.Empty<string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class with the
        /// specified alternate names.
        /// </summary>
        /// <param name="region">The region the room is located in.</param>
        /// <param name="name">The name of the room.</param>
        /// <param name="alsoKnownAs">A collection of alternate names.</param>
        public Room(Region region, string name, params string[] alsoKnownAs)
        {
            Region = region;
            Name = name;
            AlsoKnownAs = new ReadOnlyCollection<string>(alsoKnownAs);
        }

        /// <summary>
        /// Gets the region the room is located in.
        /// </summary>
        public Region Region { get; }

        /// <summary>
        /// Gets the name of the room.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a collection of alternate names for the room.
        /// </summary>
        public IReadOnlyCollection<string> AlsoKnownAs { get; }

        /// <summary>
        /// Gets the world the room is located in.
        /// </summary>
        public World World => Region.World;

        /// <summary>
        /// Gets the randomizer configuration options.
        /// </summary>
        public Config Config => Region.Config;

        /// <summary>
        /// Returns all locations in this room.
        /// </summary>
        /// <returns>A collection of locations in the room.</returns>
        public IEnumerable<Location> GetLocations()
            => GetType().GetPropertyValues<Location>(this);

        /// <summary>
        /// Returns a string that represents the room.
        /// </summary>
        /// <returns>A new string that represents this room.</returns>
        public override string ToString() => $"{Region} - {Name}";
    }
}
