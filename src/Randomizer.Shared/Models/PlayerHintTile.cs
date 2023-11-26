using System.Collections.Generic;
using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Models;

/// <summary>
/// Details about a hint tile in a player's world and what information is contained
/// in that hint tile
/// </summary>
public class PlayerHintTile
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

    public PlayerHintTile GetHintTile(string code)
    {
        var clone = MemberwiseClone() as PlayerHintTile;
        clone!.HintTileCode = code;
        return clone;
    }
}
