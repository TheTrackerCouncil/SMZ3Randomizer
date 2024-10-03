using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerRewardState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; set; }
    public RewardType RewardType { get; set; }
    public RewardType? MarkedReward { get; set; }
    public bool HasReceivedReward { get; set; }
    [MaxLength(50)]
    public string RegionName { get; set; } = string.Empty;
    public bool AutoTracked { get; set; }
    public int WorldId { get; set; }
}
