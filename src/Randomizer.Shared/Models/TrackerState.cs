using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;

using Randomizer.Shared.Migrations;

namespace Randomizer.Shared.Models
{

    public class TrackerState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset UpdatedDateTime { get; set; }
        public double SecondsElapsed { get; set; }
        public int PercentageCleared { get; set; }
        public ICollection<TrackerItemState> ItemStates { get; set; }
        public ICollection<TrackerLocationState> LocationStates { get; set; }
        public ICollection<TrackerRegionState> RegionStates { get; set; }
        public ICollection<TrackerDungeonState> DungeonStates { get; set; }
        public ICollection<TrackerMarkedLocation> MarkedLocations { get; set; }
        public ICollection<TrackerBossState> BossStates { get; set; }
        public ICollection<TrackerHistoryEvent> History { get; set; }
    }

}
