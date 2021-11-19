using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models {

    public class TrackerState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public double SecondsElapsed { get; set; }
        public ICollection<TrackerItemState> ItemStates { get; set; }
        public ICollection<TrackerLocationState> LocationStates { get; set; }
        public ICollection<TrackerRegionState> RegionStates { get; set; }
        public ICollection<TrackerDungeonState> DungeonStates { get; set; }
        public ICollection<TrackerMarkedLocation> MarkedLocations { get; set; }
    }

}
