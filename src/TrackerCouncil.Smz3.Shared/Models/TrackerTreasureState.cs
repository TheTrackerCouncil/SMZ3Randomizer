using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerTreasureState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; init; }
    [MaxLength(50)]
    public string RegionName { get; init; } = string.Empty;
    public int RemainingTreasure { get; set; }
    public int TotalTreasure { get; init; }
    public bool HasManuallyClearedTreasure { get; set; }
    public int WorldId { get; init; }
}
