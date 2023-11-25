using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.WorldData;

public class HintTile : IMergeable<ItemData>
{
    [MergeKey]
    public required string HintTileKey { get; set; }
    public required int Room { get; set; }
    public required int TopLeftX { get; set; }
    public required int TopLeftY { get; set; }
    public SchrodingersString Name { get; set; } = new();
}
