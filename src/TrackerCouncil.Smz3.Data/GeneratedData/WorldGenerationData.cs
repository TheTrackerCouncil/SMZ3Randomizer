using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Multiplayer;

namespace TrackerCouncil.Smz3.Data.GeneratedData;

/// <summary>
/// Class for housing all of the details for generating a rom for a particular world
/// </summary>
public class WorldGenerationData
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public WorldGenerationData(World world, Dictionary<int, byte[]>? patches = null)
    {
        World = world;
        Patches = patches ?? new Dictionary<int, byte[]>();
    }

    public World World { get; }
    public Dictionary<int, byte[]> Patches { get; }
    public Config Config => World.Config;
    public bool IsLocalWorld => World.IsLocalWorld;

    public MultiplayerPlayerGenerationData GetPlayerGenerationData()
    {
        var locationItems = World.Locations
            .Select(x => new PlayerGenerationLocationData(x.Id, x.Item.World.Id, x.Item.Type)).ToList();
        var dungeonData = World.TreasureRegions
            .Select(x =>
            {
                var rewardRegion = x as IHasReward;
                var requirementRegion = x as IHasPrerequisite;
                return new PlayerGenerationDungeonData(x.Name, rewardRegion?.RewardType, requirementRegion?.RequiredItem ?? ItemType.Nothing);
            }).ToList();
        return new MultiplayerPlayerGenerationData(World.Guid, World.Id, locationItems, dungeonData, World.HintTiles.ToList());
    }


}
