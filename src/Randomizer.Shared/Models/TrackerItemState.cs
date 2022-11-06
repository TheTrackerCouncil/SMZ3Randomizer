using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models
{

    public class TrackerItemState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        public TrackerState? TrackerState { get; set; }
        public ItemType? Type { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int TrackingState { get; set; }
        public int WorldId { get; set; }
    }

}
