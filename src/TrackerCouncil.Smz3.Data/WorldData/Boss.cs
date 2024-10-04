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
    /// <param name="metadata">The metadata object with additional boss info</param>
    /// <param name="bossState"></param>
    public Boss(BossType type, World world, BossInfo metadata, TrackerBossState? bossState = null)
    {
        Type = type;
        World = world;
        Name = metadata.Boss;
        Metadata = metadata;
        State = bossState ?? new TrackerBossState();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="type">The type of boss</param>
    /// <param name="world">The world this boss belongs to</param>
    /// <param name="metadata">The metadata service for looking up additional boss info</param>
    /// <param name="bossState"></param>
    public Boss(BossType type, World world, IMetadataService? metadata, TrackerBossState? bossState = null)
    {
        Type = type;
        World = world;
        Name = type.GetDescription();
        Metadata = metadata?.Boss(type) ?? new BossInfo(Name);
        State = bossState ?? new TrackerBossState();
    }

    public string Name { get; set; }

    public BossType Type { get; set; }

    public TrackerBossState State { get; set; }

    public BossInfo Metadata { get; set; }

    public World World { get; set; }

    public IHasBoss? Region { get; set; }

    public bool Defeated
    {
        get => State.Defeated;
        set => State.Defeated = value;
    }

    public bool AutoTracked
    {
        get => State.AutoTracked;
        set => State.AutoTracked = value;
    }

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
