using System.Collections.Generic;
using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Models;

public class HintTile
{
    public HintTileType Type { get; set; }
    public int WorldId { get; set; }
    public string LocationKey { get; set; } = "";
    public int? LocationWorldId { get; set; }
    public IEnumerable<LocationId>? Locations { get; set; }
    public LocationUsefulness? Usefulness { get; set; }
    public ItemType? MedallionType { get; set; }
    public string HintTileCode { get; set; } = "";
    public TrackerHintState? State { get; set; }

    public HintTile GetHintTile(string code)
    {
        var clone = MemberwiseClone() as HintTile;
        clone!.HintTileCode = code;
        return clone;
    }
}
