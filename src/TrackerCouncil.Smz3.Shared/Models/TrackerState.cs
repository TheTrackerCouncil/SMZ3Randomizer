using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public DateTimeOffset StartDateTime { get; set; }
    public DateTimeOffset UpdatedDateTime { get; set; }
    public double SecondsElapsed { get; set; }
    public int PercentageCleared { get; set; }
    public int LocalWorldId { get; set; }
    public ICollection<TrackerItemState> ItemStates { get; set; } = new List<TrackerItemState>();
    public ICollection<TrackerLocationState> LocationStates { get; set; } = new List<TrackerLocationState>();
    public ICollection<TrackerRewardState> RewardStates { get; set; } = new List<TrackerRewardState>();
    public ICollection<TrackerPrerequisiteState> PrerequisiteStates { get; set; } = new List<TrackerPrerequisiteState>();
    public ICollection<TrackerTreasureState> TreasureStates { get; set; } = new List<TrackerTreasureState>();
    public ICollection<TrackerBossState> BossStates { get; set; } = new List<TrackerBossState>();
    public ICollection<TrackerHistoryEvent> History { get; set; } = new List<TrackerHistoryEvent>();
    public ICollection<TrackerHintState> Hints { get; set; } = new List<TrackerHintState>();

    [Obsolete("Use TreasureStates, RewardStates, PrerequisiteStates, and BossStates")]
    public ICollection<TrackerDungeonState> DungeonStates { get; set; } = new List<TrackerDungeonState>();

    [Obsolete("Use TreasureStates, RewardStates, PrerequisiteStates, and BossStates")]
    public ICollection<TrackerRegionState> RegionStates { get; set; } = new List<TrackerRegionState>();

    [Obsolete("Use LocationStates")]
    public ICollection<TrackerMarkedLocation> MarkedLocations { get; set; } = new List<TrackerMarkedLocation>();
}
