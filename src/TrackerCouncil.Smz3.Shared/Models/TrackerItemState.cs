using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerItemState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; init; }
    public ItemType? Type { get; init; }
    [MaxLength(50)]
    public string ItemName { get; init; } = string.Empty;
    public int TrackingState { get; set; }
    public int WorldId { get; init; }
    [MaxLength(50)]
    public string? PlayerName { get; init; }
}
