using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerTreasureState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; set; }
    [MaxLength(50)]
    public string RegionName { get; set; } = string.Empty;
    public bool Cleared { get; set; }
    public bool AutoTracked { get; set; }
    public int RemainingTreasure { get; set; }
    public int TotalTreasure { get; set; }
    public bool HasManuallyClearedTreasure { get; set; }
    public int WorldId { get; set; }
}
