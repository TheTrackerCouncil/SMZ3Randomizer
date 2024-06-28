using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Shared.Models;

public class TrackerHintState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }
    public TrackerState? TrackerState { get; set; }
    public HintTileType Type { get; set; }
    public int WorldId { get; set; }
    public string LocationKey { get; set; } = string.Empty;
    public int? LocationWorldId { get; set; }
    public string? LocationString { get; set; }
    public LocationUsefulness? Usefulness { get; set; }
    public ItemType? MedallionType { get; set; }
    public string HintTileCode { get; set; } = string.Empty;
    public HintState HintState { get; set; }

}
