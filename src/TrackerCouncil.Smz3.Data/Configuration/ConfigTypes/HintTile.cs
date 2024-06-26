using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

public class HintTile : IMergeable<HintTile>
{
    [MergeKey]
    public required string HintTileKey { get; set; }

    [JsonIgnore, YamlIgnore]
    public required int Room { get; set; }

    [JsonIgnore, YamlIgnore]
    public required int TopLeftX { get; set; }

    [JsonIgnore, YamlIgnore]
    public required int TopLeftY { get; set; }

    /// <summary>
    /// Possible names for the hint tile
    /// </summary>
    public SchrodingersString? Name { get; set; }
}
