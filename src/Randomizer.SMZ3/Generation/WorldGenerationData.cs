using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.SMZ3.Generation;

/// <summary>
/// Class for housing all of the details for generating a rom for a particular world
/// </summary>
public class WorldGenerationData
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public WorldGenerationData(World world, Dictionary<int, byte[]>? patches = null, IEnumerable<string>? hints = null)
    {
        World = world;
        Patches = patches ?? new Dictionary<int, byte[]>();
        Hints = hints ?? new List<string>();
    }

    public World World { get; }
    public Dictionary<int, byte[]> Patches { get; }
    public IEnumerable<string> Hints { get; }
    public Config Config => World.Config;
    public bool IsLocalWorld => World.IsLocalWorld;

    public MultiplayerPlayerGenerationData GetPlayerGenerationData()
    {
        var locationItems = World.Locations
            .Select(x => new PlayerGenerationLocationData(x.Id, x.Item.World.Id, x.Item.Type)).ToList();
        var dungeonData = World.Dungeons
            .Select(x => new PlayerGenerationDungeonData(x.DungeonName, x.DungeonRewardType, x.Medallion)).ToList();
        return new MultiplayerPlayerGenerationData(World.Guid, World.Id, locationItems, dungeonData, Hints.ToList());
    }


}
