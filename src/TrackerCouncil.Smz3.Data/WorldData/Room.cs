using System.Collections.Generic;
using System.Collections.ObjectModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.Data.WorldData;

/// <summary>
/// Represents a room or section in a region.
/// </summary>
public abstract class Room : IHasLocations
{
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
        Locations = new List<Location>();
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
    public IEnumerable<Location> Locations { get; protected set; }

    /// <summary>
    /// Returns a string that represents the room.
    /// </summary>
    /// <returns>A new string that represents this room.</returns>
    public override string ToString() => $"{Region} - {Name}";

    public string RandomName => $"{Region.RandomName} - {Metadata.Name?.ToString() ?? Name}";
}
