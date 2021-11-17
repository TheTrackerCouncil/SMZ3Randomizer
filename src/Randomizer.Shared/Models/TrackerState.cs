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
        public GeneratedRom GeneratedRom { get; set; }
        public List<TrackerItemState> ItemStates { get; set; }
        public List<TrackerLocationState> LocationStates { get; set; }
        public List<TrackerRegionState> RegionStates { get; set; }
        public List<TrackerDungeonState> DungeonStates { get; set; }
        public List<TrackerMarkedLocation> MarkedLocations { get; set; }
    }

}
