using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerMarkedLocation
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; set; }
    public int LocationId { get; set; }
    public string ItemName { get; set; } = string.Empty;
}
