using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.SMZ3.FileData;

/// <summary>
/// Class used to house data utilized by the PatcherService
/// </summary>
public class PatcherServiceData
{
    public required World LocalWorld { get; init; }
    public required List<World> Worlds { get; init; }
    public required string SeedGuid { get; init; }
    public required int Seed { get; init; }
    public required Random Random { get; init; }
    public required IEnumerable<string> Hints { get; init; }
    public required Configs Configs { get; init; }
    public required PlandoConfig? PlandoConfig { get; init; }

    public const bool EnableMultiworld = true;
    public Config Config => LocalWorld.Config;
    public GameLinesConfig GameLines => Configs.GameLines;

    public RegionInfo? GetRegionInfo(Region region) => Configs.Regions.FirstOrDefault(x => x.Type == region.GetType());
    public ItemData? GetItemData(Item item) => Configs.Items.FirstOrDefault(x => x.InternalItemType == item.Type);
}
