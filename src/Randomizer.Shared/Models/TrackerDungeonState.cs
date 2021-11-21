using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models {

    public class TrackerDungeonState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public TrackerState TrackerState { get; set; }
        public string Name { get; set; }
        public bool Cleared { get; set; }
        public int RemainingTreasure { get; set; }
        public RewardItem Reward { get; set; }
        public Medallion RequiredMedallion { get; set; }
    }

}
