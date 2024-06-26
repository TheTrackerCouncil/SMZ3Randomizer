using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using TrackerCouncil.Data.Configuration;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Represents a dungeon in A Link to the Past.
/// </summary>
public class DungeonInfo : IMergeable<DungeonInfo>
{
    public DungeonInfo() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DungeonInfo"/> class.
    /// </summary>
    /// <param name="name">The name of the dungeon.</param>
    /// <param name="boss">The name of the boss.</param>
    public DungeonInfo(SchrodingersString name,SchrodingersString boss)
    {
        Name = name;
        Boss = boss;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DungeonInfo"/> class.
    /// </summary>
    /// <param name="name">The name of the dungeon.</param>
    /// <param name="boss">The name of the boss.</param>
    public DungeonInfo(string name, string boss)
    {
        Dungeon = name;
        Name = new (name);
        Boss = new (boss);
    }

    /// <summary>
    /// The identifier for merging configs
    /// </summary>
    [MergeKey]
    public string Dungeon { get; init; } = "";

    /// <summary>
    /// Gets the possible names of the dungeon.
    /// </summary>
    public SchrodingersString? Name { get; set; }

    /// <summary>
    /// Gets the possible names of the dungeon boss.
    /// </summary>
    public SchrodingersString? Boss { get; set; }

    /// <summary>
    /// The name of the type of region that represents this dungeon
    /// </summary>
    [JsonIgnore, YamlIgnore]
    public Type? Type { get; init; }

    /// <summary>
    /// Returns a string representation of the dungeon.
    /// </summary>
    /// <returns>A string representing the dungeon.</returns>
    public override string ToString() => Dungeon;

    /// <summary>
    /// Determines whether the specified region represents this dungeon.
    /// </summary>
    /// <param name="region">The region to check.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="region"/> matches the dungeon;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Is(Region region)
        => Type == region.GetType();

    /// <summary>
    /// Determines whether the specified area either represents this dungeon
    /// or is located in this dungeon.
    /// </summary>
    /// <param name="area">The area to check.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="area"/> is or is contained in this
    /// dungeon; otherwise, <c>false</c>.
    /// </returns>
    public bool Is(IHasLocations area)
    {
        if (area is Region region)
            return Is(region);
        else if (area is Room room)
            return Is(room.Region);
        else
            return false;
    }

    /// <summary>
    /// Returns the region that represents this dungeon in the specified
    /// <see cref="World"/>.
    /// </summary>
    /// <param name="world">The world those regions to find.</param>
    /// <returns>
    /// The <see cref="Region"/> in <paramref name="world"/> that matches
    /// this dungeon.
    /// </returns>
    public Region GetRegion(World world)
        => world.Regions.Single(Is);

    /// <summary>
    /// Determines whether the dungeon is accessible with the specified set
    /// of items.
    /// </summary>
    /// <param name="world">
    /// The instance of the world that contains the dungeon.
    /// </param>
    /// <param name="progression">The available items.</param>
    /// <returns>
    /// <c>true</c> if the dungeon is accessible; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAccessible(World world, Progression progression)
    {
        var region = GetRegion(world);
        return region.CanEnter(progression, true);
    }

    /// <summary>
    /// Returns the locations associated with the dungeon.
    /// </summary>
    /// <param name="world">
    /// The instance of the world whose locations to return.
    /// </param>
    /// <returns>
    /// A collection of locations in the dungeon from the specified world.
    /// </returns>
    public IReadOnlyCollection<Location> GetLocations(World world)
    {
        var region = GetRegion(world);
        return region.Locations.Where(x => x.Type != LocationType.NotInDungeon).ToImmutableList();
    }
}
