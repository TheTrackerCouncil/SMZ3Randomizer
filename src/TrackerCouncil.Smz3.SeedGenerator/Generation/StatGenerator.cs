using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Interfaces;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class StatGenerator(Smz3Randomizer randomizer): IStatGenerator
{
    public async Task GenerateStatsAsync(int numberOfSeeds, Config config, CancellationToken ct)
    {
        var stats = InitStats();
        var itemCounts = new ConcurrentDictionary<(int itemId, LocationId locationId), int>();
        ThreadPool.GetAvailableThreads(out _, out _);

        var finished = false;

        await Task.Run(() =>
        {
            var i = 0;
            Parallel.For(0, numberOfSeeds, (_, _) =>
            {
                try
                {
                    ct.ThrowIfCancellationRequested();
                    var seed = randomizer.GenerateSeed(config.SeedOnly(), null, ct);

                    ct.ThrowIfCancellationRequested();
                    GatherStats(stats, seed);
                    AddToMegaSpoilerLog(itemCounts, seed);
                }
                catch (Exception)
                {
                    // ignored
                }

                var seedsGenerated = Interlocked.Increment(ref i);
                StatProgressUpdated?.Invoke(this, new StatUpdateEventaArgs(seedsGenerated, numberOfSeeds));
            });

            finished = !ct.IsCancellationRequested;
        }, ct);

        if (!finished)
        {
            return;
        }

        WriteMegaSpoilerLog(itemCounts, numberOfSeeds);

        StatsCompleted?.Invoke(this, new StatsCompletedEventArgs(GetStatsString(stats, itemCounts, numberOfSeeds)));
    }

    public event EventHandler<StatUpdateEventaArgs>? StatProgressUpdated;
    public event EventHandler<StatsCompletedEventArgs>? StatsCompleted;

    private ConcurrentDictionary<string, int> InitStats()
    {
        var stats = new ConcurrentDictionary<string, int>();
        stats.TryAdd("Successfully generated", 0);
        stats.TryAdd("Shaktool betrays you", 0);
        stats.TryAdd("Zora is a scam", 0);
        stats.TryAdd("Catfish is a scamfish", 0);
        stats.TryAdd("\"I want to go on something more thrilling than Peg World.\"", 0);
        stats.TryAdd("The Morph Ball is in its original location", 0);
        stats.TryAdd("The GT Moldorm chest contains a Metroid item", 0);
        return stats;
    }

    private void GatherStats(ConcurrentDictionary<string, int> stats, SeedData seed)
    {
        var world = seed.WorldGenerationData.LocalWorld;

        stats.Increment("Successfully generated");

        if (IsScam(world.World.FindLocation(LocationId.InnerMaridiaSpringBall).Item.Type))
            stats.Increment("Shaktool betrays you");

        if (IsScam(world.World.FindLocation(LocationId.KingZora).Item.Type))
            stats.Increment("Zora is a scam");

        if (IsScam(world.World.DarkWorldNorthEast.Catfish.Item.Type))
            stats.Increment("Catfish is a scamfish");

        if (IsScam(world.World.DarkWorldNorthWest.PegWorld.Item.Type))
            stats.Increment("\"I want to go on something more thrilling than Peg World.\"");

        if (world.World.FindLocation(LocationId.BlueBrinstarMorphBallRight).Item.Type == ItemType.Morph)
            stats.Increment("The Morph Ball is in its original location");

        if (world.World.GanonsTower.MoldormChest.Item.Type.IsInCategory(ItemCategory.Metroid))
            stats.Increment("The GT Moldorm chest contains a Metroid item");
    }

    private void WriteMegaSpoilerLog(ConcurrentDictionary<(int itemId, LocationId locationId), int> itemCounts, int numberOfSeeds)
    {
        var items = Enum.GetValues<ItemType>().ToDictionary(x => (int)x);
        var locations = new World(new Config(), "", 0, "").Locations.ToList();

        var itemLocations = items.Values
            .Where(item => itemCounts.Keys.Any(x => x.itemId == (int)item))
            .ToDictionary(
                keySelector: item => item.GetDescription(),
                elementSelector: item => itemCounts.Where(x => x.Key.itemId == (int)item)
                    .OrderByDescending(x => x.Value)
                    .ThenBy(x => locations.Single(y => y.Id == x.Key.locationId).ToString())
                    .ToDictionary(
                        keySelector: x => locations.Single(y => y.Id == x.Key.locationId).ToString(),
                        elementSelector: x => x.Value)
        );

        // Area > region > location
        var locationItems = locations.Select(location => new
        {
            location.Region.Area,
            Name = location.ToString(),
            Items = itemCounts.Where(x => x.Key.locationId == location.Id)
                    .OrderByDescending(x => x.Value)
                    .ThenBy(x => x.Key.itemId)
                    .ToDictionary(
                        keySelector: x => items[x.Key.itemId].GetDescription(),
                        elementSelector: x => x.Value)
        })
            .GroupBy(x => x.Area, x => new { x.Name, x.Items })
            .ToDictionary(x => x.Key, x => x.ToList());

        var total = (double)numberOfSeeds;
        var dungeonItems = locations
            .Where(x => x.Region is IHasTreasure)
            .OrderBy(x => x.Region.Name)
            .Select(location => new
            {
                Name = location.ToString(),
                Keys = (GetLocationSmallKeyCount(location.Id)/total).ToString("0.00%"),
                BigKeys = (GetLocationBigKeyCount(location.Id)/total).ToString("0.00%"),
                MapCompass = (GetLocationMapCompassCount(location.Id)/total).ToString("0.00%"),
                Treasures = (GetLocationTreasureCount(location.Id)/total).ToString("0.00%"),
                Progression = (GetLocationProgressionCount(location.Id)/total).ToString("0.00%"),
            });

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var json = JsonSerializer.Serialize(new
        {
            ByItem = itemLocations,
            ByLocation = locationItems,
            DungeonItems = dungeonItems
        }, options);

        var path = Path.Combine(Directories.AppDataFolder, "item_generation_stats.json");
        File.WriteAllText(path, json);

        var startInfo = new ProcessStartInfo(path)
        {
            UseShellExecute = true
        };
        Process.Start(startInfo);

        int GetLocationSmallKeyCount(LocationId locationId)
        {
            return itemCounts.Where(x =>
                x.Key.locationId == locationId &&
                items[x.Key.itemId].IsInCategory(ItemCategory.SmallKey)).Sum(x => x.Value);
        }

        int GetLocationBigKeyCount(LocationId locationId)
        {
            return itemCounts.Where(x =>
                x.Key.locationId == locationId &&
                items[x.Key.itemId].IsInCategory(ItemCategory.BigKey)).Sum(x => x.Value);
        }

        int GetLocationMapCompassCount(LocationId locationId)
        {
            return itemCounts.Where(x =>
                x.Key.locationId == locationId &&
                items[x.Key.itemId].IsInAnyCategory(ItemCategory.Compass, ItemCategory.Map)).Sum(x => x.Value);
        }

        int GetLocationTreasureCount(LocationId locationId)
        {
            return itemCounts.Where(x =>
                x.Key.locationId == locationId &&
                !items[x.Key.itemId].IsInAnyCategory(ItemCategory.BigKey, ItemCategory.SmallKey, ItemCategory.Compass, ItemCategory.Map))
                .Sum(x => x.Value);
        }

        int GetLocationProgressionCount(LocationId locationId)
        {
            return itemCounts.Where(x =>
                    x.Key.locationId == locationId &&
                    !items[x.Key.itemId].IsInAnyCategory(ItemCategory.BigKey, ItemCategory.SmallKey, ItemCategory.Compass, ItemCategory.Map) &&
                    items[x.Key.itemId].IsPossibleProgression(false, false, true))
                .Sum(x => x.Value);
        }
    }

    private string GetStatsString(IDictionary<string, int> stats,
        ConcurrentDictionary<(int itemId, LocationId locationId), int> itemCounts, int total)
    {
        var message = new StringBuilder();
        message.AppendLine($"If you were to play {total} seeds:");
        foreach (var key in stats.Keys)
        {
            var number = stats[key];
            var percentage = number / (double)total;
            message.AppendLine($"- {key} {number} time(s) ({percentage:P1})");
        }
        message.AppendLine();
        message.AppendLine($"Morph ball is in {UniqueLocations(ItemType.Morph)} unique locations.");

        return message.ToString();

        int UniqueLocations(ItemType item)
        {
            return itemCounts.Keys
                .Where(x => x.itemId == (int)item)
                .Select(x => x.locationId)
                .Count();
        }
    }

    private void AddToMegaSpoilerLog(ConcurrentDictionary<(int itemId, LocationId locationId), int> itemCounts, SeedData seed)
    {
        foreach (var location in seed.WorldGenerationData.LocalWorld.World.Locations)
        {
            itemCounts.Increment(((int)location.Item.Type, location.Id));
        }
    }

    private static bool IsScam(ItemType itemType) => itemType.IsInCategory(ItemCategory.Scam);
}
