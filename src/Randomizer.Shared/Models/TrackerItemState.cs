using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models
{

    public class TrackerItemState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public TrackerState TrackerState { get; set; }
        public ItemType? Type { get; set; }
        public string ItemName { get; set; }
        public int TrackingState { get; set; }
    }

}
