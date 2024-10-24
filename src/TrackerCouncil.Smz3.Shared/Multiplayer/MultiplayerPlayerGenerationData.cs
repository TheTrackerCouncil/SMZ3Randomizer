using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Shared.Multiplayer;

public class MultiplayerPlayerGenerationData(
    string playerGuid,
    int worldId,
    List<PlayerGenerationLocationData> locations,
    Dictionary<string, BossType> bosses,
    Dictionary<string, RewardType> rewards,
    Dictionary<string, ItemType> prerequisites,
    List<PlayerHintTile> hints)
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string PlayerGuid { get; } = playerGuid;
    public int WorldId { get; } = worldId;
    public List<PlayerGenerationLocationData> Locations { get; } = locations;
    public Dictionary<string, BossType> Bosses { get; } = bosses;
    public Dictionary<string, RewardType> Rewards { get; } = rewards;
    public Dictionary<string, ItemType> Prerequisites { get; } = prerequisites;
    public List<PlayerHintTile> Hints { get; } = hints;

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

public class PlayerGenerationLocationData(LocationId id, int itemWorldId, ItemType item)
{
    public LocationId Id { get; } = id;
    public int ItemWorldId { get; } = itemWorldId;
    public ItemType Item { get; } = item;
}
