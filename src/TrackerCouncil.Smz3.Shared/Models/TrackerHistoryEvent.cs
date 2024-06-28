using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerHistoryEvent
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; set; }
    public HistoryEventType Type { get; set; }
    public LocationId? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public bool IsImportant { get; set; }
    public bool IsUndone { get; set; }
    public double Time { get; set; }
}
