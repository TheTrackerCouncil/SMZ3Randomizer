using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Logic;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;

namespace Randomizer.Data.WorldData
{
    /// <summary>
    /// Represents a room or section in a region.
    /// </summary>
    public abstract class Room : IHasLocations
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class that has
        /// no alternate names.
        /// </summary>
        /// <param name="region">The region the room is located in.</param>
        /// <param name="name">The name of the room.</param>
        /// <param name="metadata"></param>
        public Room(Region region, string name, IMetadataService? metadata)
            : this(region, name, metadata, Array.Empty<string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class with the
        /// specified alternate names.
        /// </summary>
        /// <param name="region">The region the room is located in.</param>
        /// <param name="name">The name of the room.</param>
        /// <param name="metadata"></param>
        /// <param name="alsoKnownAs">A collection of alternate names.</param>
        public Room(Region region, string name, IMetadataService? metadata, params string[] alsoKnownAs)
        {
            Region = region;
            Name = name;
            AlsoKnownAs = new ReadOnlyCollection<string>(alsoKnownAs);
            Metadata = metadata?.Room(this) ?? new RoomInfo(name);
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
        /// The Logic to be used to determine if certain actions can be done
        /// </summary>
        public ILogic Logic => World.Logic;

        /// <summary>
        /// Gets the randomizer configuration options.
        /// </summary>
        public Config Config => Region.Config;

        /// <summary>
        /// Additional information about the room
        /// </summary>
        public RoomInfo Metadata { get; set; }

        /// <summary>
        /// Gets all locations in the room.
        /// </summary>
        public IEnumerable<Location> Locations => GetLocations();

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
