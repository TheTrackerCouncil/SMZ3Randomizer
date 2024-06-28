using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData;

/// <summary>
/// Class to represent a Super Metroid boss that can be defeated
/// </summary>
public class Boss
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="type">The type of boss</param>
    /// <param name="world">The world this boss belongs to</param>
    /// <param name="name">The name of the boss</param>
    /// <param name="metadata">The metadata object with additional boss info</param>
    /// <param name="state">The tracking state of the boss</param>
    public Boss(BossType type, World world, string name, BossInfo metadata, TrackerBossState state)
    {
        Type = type;
        World = world;
        Name = name;
        Metadata = metadata;
        State = state;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="type">The type of boss</param>
    /// <param name="world">The world this boss belongs to</param>
    /// <param name="region">The region where this boss is locatedd</param>
    /// <param name="metadata">The metadata service for looking up additional boss info</param>
    /// <param name="trackerState">The tracking state for this run</param>
    public Boss(BossType type, World world, IHasBoss region, IMetadataService? metadata, TrackerState? trackerState)
    {
        Type = type;
        World = world;
        Region = region;
        Name = type.GetDescription();
        Metadata = metadata?.Boss(type) ?? new BossInfo(Name);
        State = trackerState?.BossStates.First(x => x.WorldId == world.Id && x.BossName == Name) ?? new TrackerBossState();
    }

    public string Name { get; set; }

    public BossType Type { get; set; }

    public TrackerBossState State { get; set; }

    public BossInfo Metadata { get; set; }

    public World World { get; set; }

    public IHasBoss? Region { get; set; }

    /// <summary>
    /// Determines if an item matches the type or name
    /// </summary>
    /// <param name="type">The type to compare against</param>
    /// <param name="name">The name to compare against if the item type is set to Nothing</param>
    /// <see langword="true"/> if the item matches the given type or name
    /// name="type"/> or <paramref name="type"/>; otherwise, <see
    /// langword="false"/>.
    public bool Is(BossType type, string name)
        => (Type != BossType.None && Type == type) || (Type == BossType.None && Name == name);
}
