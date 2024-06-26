﻿using System;
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
    public ICollection<TrackerRegionState> RegionStates { get; set; } = new List<TrackerRegionState>();
    public ICollection<TrackerDungeonState> DungeonStates { get; set; } = new List<TrackerDungeonState>();
    public ICollection<TrackerMarkedLocation> MarkedLocations { get; set; } = new List<TrackerMarkedLocation>();
    public ICollection<TrackerBossState> BossStates { get; set; } = new List<TrackerBossState>();
    public ICollection<TrackerHistoryEvent> History { get; set; } = new List<TrackerHistoryEvent>();
    public ICollection<TrackerHintState> Hints { get; set; } = new List<TrackerHintState>();
}
