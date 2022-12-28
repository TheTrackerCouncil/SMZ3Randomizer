using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models
{

    public class TrackerLocationState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        public TrackerState? TrackerState { get; set; }
        public int LocationId { get; init; }
        public ItemType Item { get; init; }
        public ItemType? MarkedItem { get; set; }
        public bool Cleared { get; set; }
        public bool Autotracked { get; set; }
        public int WorldId { get; init; }
        public int ItemWorldId { get; init; }
        public bool HasMarkedItem => MarkedItem != null && MarkedItem != ItemType.Nothing;
    }

}
