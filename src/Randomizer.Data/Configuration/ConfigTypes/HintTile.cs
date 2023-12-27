using Newtonsoft.Json;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.WorldData;

public class HintTile : IMergeable<ItemData>
{
    [MergeKey]
    public required string HintTileKey { get; set; }

    [JsonIgnore]
    public required int Room { get; set; }

    [JsonIgnore]
    public required int TopLeftX { get; set; }

    [JsonIgnore]
    public required int TopLeftY { get; set; }

    public SchrodingersString Name { get; set; } = new();
}
