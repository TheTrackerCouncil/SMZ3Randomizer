using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerLocationState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; set; }
    public LocationId LocationId { get; init; }
    public ItemType Item { get; init; }
    public ItemType? MarkedItem { get; set; }
    public LocationUsefulness? MarkedUsefulness { get; set; }
    public bool Cleared { get; set; }
    public bool Autotracked { get; set; }
    public int WorldId { get; init; }
    public int ItemWorldId { get; init; }
    [MaxLength(50)]
    public string? ItemName { get; init; }
    [MaxLength(50)]
    public string? ItemOwnerName { get; init; }
    public bool HasMarkedItem => MarkedItem != null && MarkedItem != ItemType.Nothing;
    public bool HasMarkedCorrectItem => Item.IsEquivalentTo(MarkedItem);
}
