using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerDungeonState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; set; }
    public int WorldId { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    public bool Cleared { get; set; }
    public bool AutoTracked { get; set; }

    [Obsolete("Use TrackerTreasureState/IHasTreasure Instead")]
    public int RemainingTreasure { get; set; }

    [Obsolete("Use TrackerTreasureState/IHasTreasure Instead")]
    public bool HasManuallyClearedTreasure { get; set; }

    [Obsolete("Use TrackerRewardState/IHasReward Instead")]
    public RewardType? Reward { get; set; }

    [Obsolete("Use TrackerRewardState/IHasReward Instead")]
    public RewardType? MarkedReward { get; set; }

    [Obsolete("Use TrackerRewardState/IHasReward Instead")]
    public bool HasReward => Reward != null && Reward != RewardType.None;

    [Obsolete("Use TrackerRewardState/IHasReward Instead")]
    public bool HasMarkedReward => MarkedReward != null && MarkedReward != RewardType.None;

    [Obsolete("Use TrackerPrerequisiteState/IHasPrerequisite Instead")]
    public ItemType? RequiredMedallion { get; set; }

    [Obsolete("Use TrackerPrerequisiteState/IHasPrerequisite Instead")]
    public ItemType? MarkedMedallion { get; set; }

    [Obsolete("Use TrackerPrerequisiteState/IHasPrerequisite Instead")]
    public bool RequiresMedallion => RequiredMedallion != null && RequiredMedallion.Value.IsInCategory(ItemCategory.Medallion);
}
