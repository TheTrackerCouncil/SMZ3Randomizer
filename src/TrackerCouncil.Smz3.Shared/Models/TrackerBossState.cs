using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerBossState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; init; }
    [MaxLength(50)]
    public string RegionName { get; init; } = string.Empty;
    [MaxLength(50)]
    public string BossName { get; init; } = string.Empty;
    public bool Defeated { get; set; }
    public bool AutoTracked { get; set; }
    public BossType Type { get; set; }
    public int WorldId { get; set; }
}
