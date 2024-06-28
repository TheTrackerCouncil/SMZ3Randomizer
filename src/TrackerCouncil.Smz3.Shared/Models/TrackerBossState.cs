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
    public string BossName { get; init; } = string.Empty;
    public bool Defeated { get; set; }
    public bool AutoTracked { get; set; }
    public BossType Type { get; init; }
    public int WorldId { get; set; }
}
