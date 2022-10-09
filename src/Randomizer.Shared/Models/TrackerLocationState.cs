using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models
{

    public class TrackerLocationState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public TrackerState TrackerState { get; set; }
        public int LocationId { get; set; }
        public ItemType? Item { get; set; }
        public ItemType? MarkedItem { get; set; }
        public bool Cleared { get; set; }
        public bool HasMarkedItem => MarkedItem != null && MarkedItem != ItemType.Nothing;
    }

}
