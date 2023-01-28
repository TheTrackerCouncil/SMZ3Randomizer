using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Models {

    public class TrackerDungeonState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }

        public TrackerState? TrackerState { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Cleared { get; set; }
        public bool AutoTracked { get; set; }
        public int RemainingTreasure { get; set; }
        public RewardType? Reward { get; set; }
        public ItemType? RequiredMedallion { get; set; }
        public RewardType? MarkedReward { get; set; }
        public ItemType? MarkedMedallion { get; set; }
        public bool HasManuallyClearedTreasure { get; set; }
        public int WorldId { get; set; }
        public bool HasReward => Reward != null && Reward != RewardType.None;
        public bool HasMarkedReward => MarkedReward != null && MarkedReward != RewardType.None;
        public bool RequiresMedallion => RequiredMedallion != null && RequiredMedallion.Value.IsInCategory(ItemCategory.Medallion);
    }

}
