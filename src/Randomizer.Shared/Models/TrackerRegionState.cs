using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models {

    public class TrackerRegionState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        public TrackerState? TrackerState { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public RewardType? Reward { get; set; }
        public ItemType? Medallion { get; set; }
    }

}
