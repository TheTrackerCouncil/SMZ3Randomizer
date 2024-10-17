using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerMedallionState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; set; }
    [MaxLength(50)]
    public string RegionName { get; set; } = string.Empty;
    public ItemType RequiredMedallion { get; set; }
    public ItemType? MarkedMedallion { get; set; }
}
