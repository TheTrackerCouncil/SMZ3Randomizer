using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Randomizer.Shared.Multiplayer;

public class MultiplayerPlayerGenerationData
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public MultiplayerPlayerGenerationData(string playerGuid, int worldId, List<PlayerGenerationLocationData> locations,
        List<PlayerGenerationDungeonData> dungeons, List<string> hints)
    {
        PlayerGuid = playerGuid;
        WorldId = worldId;
        Locations = locations;
        Dungeons = dungeons;
        Hints = hints;
    }

    public string PlayerGuid { get; }
    public int WorldId { get; }
    public List<PlayerGenerationLocationData> Locations { get; }
    public List<PlayerGenerationDungeonData> Dungeons { get; }
    public List<string> Hints { get; }

    /// <summary>
    /// Converts the generation data into a compressed string of the json
    /// </summary>
    /// <param name="data">The data to convert</param>
    /// <returns>The compressed json string representation of the data</returns>
    public static string ToString(MultiplayerPlayerGenerationData data)
    {
        var json = JsonSerializer.Serialize(data, s_options);
        var buffer = Encoding.UTF8.GetBytes(json);
        var memoryStream = new MemoryStream();
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
        {
            gZipStream.Write(buffer, 0, buffer.Length);
        }

        memoryStream.Position = 0;

        var compressedData = new byte[memoryStream.Length];
        memoryStream.Read(compressedData, 0, compressedData.Length);

        var gZipBuffer = new byte[compressedData.Length + 4];
        Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
        Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
        return Convert.ToBase64String(gZipBuffer);
    }

    /// <summary>
    /// Takes in a compressed json string and converts it into a MultiplayerPlayerGenerationData object
    /// </summary>
    /// <param name="compressedString">The compressed json string representation of the data</param>
    /// <returns>The MultiplayerPlayerGenerationData data object, if it was able to be converted</returns>
    public static MultiplayerPlayerGenerationData? FromString(string compressedString)
    {
        var gZipBuffer = Convert.FromBase64String(compressedString);
        using var memoryStream = new MemoryStream();
        var dataLength = BitConverter.ToInt32(gZipBuffer, 0);
        memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

        var buffer = new byte[dataLength];

        memoryStream.Position = 0;
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
        {
            gZipStream.Read(buffer, 0, buffer.Length);
        }

        var json = Encoding.UTF8.GetString(buffer);
        return JsonSerializer.Deserialize<MultiplayerPlayerGenerationData>(json, s_options);
    }
}

public class PlayerGenerationLocationData
{
    public PlayerGenerationLocationData(int id, int itemWorldId, ItemType item)
    {
        Id = id;
        ItemWorldId = itemWorldId;
        Item = item;
    }

    public int Id { get; }
    public int ItemWorldId { get; }
    public ItemType Item { get; }
}

public class PlayerGenerationDungeonData
{
    public PlayerGenerationDungeonData(string name, RewardType? reward, ItemType medallion)
    {
        Name = name;
        Reward = reward;
        Medallion = medallion;
    }

    public string Name { get; } = "";
    public RewardType? Reward { get; }
    public ItemType Medallion { get; }
}
