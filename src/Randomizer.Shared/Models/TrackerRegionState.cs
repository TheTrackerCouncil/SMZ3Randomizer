using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models {

    public class TrackerRegionState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public TrackerState TrackerState { get; set; }
        public string TypeName { get; set; }
        public ItemType? Reward { get; set; }
        public ItemType? Medallion { get; set; }
    }

}
