﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Models {

    public class TrackerHistoryEvent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public TrackerState TrackerState { get; set; }
        public HistoryEventType Type { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public string ObjectName { get; set; }
        public bool IsImportant { get; set; }
        public double Time { get; set; }
    }

}
