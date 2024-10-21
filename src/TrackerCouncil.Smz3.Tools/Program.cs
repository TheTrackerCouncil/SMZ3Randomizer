using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.Generation;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.UI;

namespace TrackerCouncil.Smz3.Tools;

public static class Program
{
    private static ServiceProvider s_services = null!;
    private static ILogger<Smz3Randomizer> s_logger = null!;

    static Program()
    {
    }

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(LogEventLevel.Information)
            //.WriteTo.Debug()
            .CreateLogger();

        s_services = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.AddSerilog(dispose: true);
            })
            .ConfigureServices()
            .BuildServiceProvider();

        s_logger = s_services.GetRequiredService<ILogger<Smz3Randomizer>>();

        s_logger.LogInformation("{Response}", GenerateStats(new Config()
        {
            LogicConfig =
            {
                PreventScrewAttackSoftLock = true,
                PreventFivePowerBombSeed = true,
                LeftSandPitRequiresSpringBall = true,
                LaunchPadRequiresIceBeam = true,
                WaterwayNeedsGravitySuit = true,
                EasyBlueBrinstarTop = true,
                KholdstareNeedsCaneOfSomaria = false,
                ZoraNeedsRupeeItems = true,
                QuarterMagic = false,
                FireRodDarkRooms = false,
                InfiniteBombJump = false,
                ParlorSpeedBooster = false,
                MoatSpeedBooster = false,
                MockBall = false,
                SwordOnlyDarkRooms = false,
                LightWorldSouthFakeFlippers = false
            },
        }));
    }

    private static string GenerateStats(Config config, int count = 10000)
    {
        var start = DateTime.Now;
        var stats = new ConcurrentBag<StatsDetails>();

        var pedProgression = 0;
        var pedNice = 0;
        var pedJunk = 0;
        var pedItems = new ConcurrentBag<ItemType>();

        var treeProgression = 0;
        var treeNice = 0;
        var treeJunk = 0;
        var treeItems = new ConcurrentBag<ItemType>();

        var sahaProgression = 0;
        var sahaNice = 0;
        var sahaJunk = 0;
        var sahaItems = new ConcurrentBag<ItemType>();

        var anyProgression = 0;
        var anyNice = 0;
        var anyJunk = 0;

        var hintService = s_services.GetRequiredService<IGameHintService>();
        var playthroughService = s_services.GetRequiredService<PlaythroughService>();

        Parallel.For(0, count, new ParallelOptions() { MaxDegreeOfParallelism = 12 }, (_, _) =>
        {
            try
            {
                bool anyHaveMandatory = false;
                bool anyHaveNice = false;

                var seed = s_services.GetRequiredService<Smz3Randomizer>().GenerateSeed(config);
                var playthrough = playthroughService.Generate(seed.WorldGenerationData.Select(x => x.World).ToList(), config);
                stats.Add(new StatsDetails
                {
                    NumSpheres = playthrough.Spheres.Count,
                    ItemLocations = seed.WorldGenerationData.LocalWorld.World.Locations.Select(x => ((int)x.Id, x.Item.Type)).ToList()
                });

                var location =
                    seed.WorldGenerationData.LocalWorld.World.FindLocation(LocationId.MasterSwordPedestal);
                var usefulness = hintService.GetLocationUsefulness(location,
                    seed.WorldGenerationData.Worlds.ToList(), playthrough);
                switch (usefulness)
                {
                    case LocationUsefulness.Mandatory:
                        pedProgression++;
                        pedItems.Add(location.Item.Type);
                        anyHaveMandatory = true;
                        break;
                    case LocationUsefulness.NiceToHave:
                    case LocationUsefulness.Sword:
                        pedNice++;
                        anyHaveNice = true;
                        break;
                    default:
                        pedJunk++;
                        break;
                }

                location =
                    seed.WorldGenerationData.LocalWorld.World.FindLocation(LocationId.LumberjackTree);
                usefulness = hintService.GetLocationUsefulness(location,
                    seed.WorldGenerationData.Worlds.ToList(), playthrough);
                switch (usefulness)
                {
                    case LocationUsefulness.Mandatory:
                        treeProgression++;
                        treeItems.Add(location.Item.Type);
                        anyHaveMandatory = true;
                        break;
                    case LocationUsefulness.NiceToHave:
                    case LocationUsefulness.Sword:
                        treeNice++;
                        anyHaveNice = true;
                        break;
                    default:
                        treeJunk++;
                        break;
                }

                location =
                    seed.WorldGenerationData.LocalWorld.World.FindLocation(LocationId.Sahasrahla);
                usefulness = hintService.GetLocationUsefulness(location,
                    seed.WorldGenerationData.Worlds.ToList(), playthrough);
                switch (usefulness)
                {
                    case LocationUsefulness.Mandatory:
                        sahaProgression++;
                        sahaItems.Add(location.Item.Type);
                        anyHaveMandatory = true;
                        break;
                    case LocationUsefulness.NiceToHave:
                    case LocationUsefulness.Sword:
                        sahaNice++;
                        anyHaveNice = true;
                        break;
                    default:
                        sahaJunk++;
                        break;
                }

                if (anyHaveMandatory)
                {
                    anyProgression++;
                }
                else if (anyHaveNice)
                {
                    anyNice++;
                }
                else
                {
                    anyJunk++;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error generating seed");
            }
        });

        var itemLocationCounts = new Dictionary<(int, ItemType), (int, ItemType, int)>();
        foreach (var itemLocation in stats.SelectMany(x => x.ItemLocations))
        {
            if (!itemLocation.Item2.IsInAnyCategory(ItemCategory.Junk, ItemCategory.Scam, ItemCategory.NonRandomized, ItemCategory.Compass,
                    ItemCategory.SmallKey, ItemCategory.BigKey, ItemCategory.Map, ItemCategory.Keycard) && itemLocation.Item2 != ItemType.Nothing
                && !(itemLocation.Item2 is ItemType.ProgressiveGlove or ItemType.ProgressiveShield or ItemType.ProgressiveSword or ItemType.ProgressiveTunic))
            {
                if (!itemLocationCounts.ContainsKey(itemLocation))
                    itemLocationCounts.Add(itemLocation, (itemLocation.Item1, itemLocation.Item2, 1));
                else
                    itemLocationCounts[itemLocation] = (itemLocation.Item1, itemLocation.Item2, itemLocationCounts[itemLocation].Item3 + 1);
            }
        }
        var itemLocationCountsList = itemLocationCounts.Values.OrderBy(x => x.Item2).ToList();

        var leastCommon = ItemType.Nothing;
        var leastCount = int.MaxValue;
        var mostCommon = ItemType.Nothing;
        var mostCount = int.MinValue;

        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            if (!itemType.IsInAnyCategory(ItemCategory.Junk, ItemCategory.Scam, ItemCategory.NonRandomized, ItemCategory.Compass,
                    ItemCategory.SmallKey, ItemCategory.BigKey, ItemCategory.Map, ItemCategory.Keycard) && itemType != ItemType.Nothing
                && !(itemType is ItemType.ProgressiveGlove or ItemType.ProgressiveShield or ItemType.ProgressiveSword or ItemType.ProgressiveTunic))
            {
                var uniqueLocations = UniqueLocations(itemLocationCountsList, itemType);
                if (uniqueLocations < leastCount)
                {
                    leastCommon = itemType;
                    leastCount = uniqueLocations;
                }
                if (uniqueLocations > mostCount)
                {
                    mostCommon = itemType;
                    mostCount = uniqueLocations;
                }
            }
        }

        var end = DateTime.Now;
        var ts = (end - start);

        var sb = new StringBuilder();
        sb.AppendLine("");
        sb.AppendLine("Keysanity: " + config.KeysanityMode + " | Item placement: " + config.ItemPlacementRule);
        sb.AppendLine("----------------------------------------------------------------------------------------");
        sb.AppendLine($"Num succeeded: {stats.Count} ({stats.Count * 1f / count * 100f}%)");
        sb.AppendLine("Average spheres: " + stats.Sum(x => x.NumSpheres) * 1f / stats.Count() * 1f);
        sb.AppendLine("Max spheres: " + stats.Max(x => x.NumSpheres));
        sb.AppendLine("Min spheres: " + stats.Min(x => x.NumSpheres));
        sb.AppendLine("Item with most unique locations: " + mostCommon + " with " + mostCount + " unique locations");
        sb.AppendLine("Item with least unique locations: " + leastCommon + " with " + leastCount + " unique locations");
        sb.AppendLine($"Ped Progression: {pedProgression * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Ped Nice: {pedNice * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Ped Junk: {pedJunk * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Ped items: {string.Join(',', pedItems.Distinct())}");
        sb.AppendLine($"Tree Progression: {treeProgression * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Tree Nice: {treeNice * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Tree Junk: {treeJunk * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Tree items: {string.Join(',', treeItems.Distinct())}");
        sb.AppendLine($"Saha Progression: {sahaProgression * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Saha Nice: {sahaNice * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Saha Junk: {sahaJunk * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Saha items: {string.Join(',', sahaItems.Distinct())}");
        sb.AppendLine($"Ped, Tree, or Saha Progression: {anyProgression * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Ped, Tree, or Saha Nice: {anyNice * 1f / stats.Count * 100f}%");
        sb.AppendLine($"Ped, Tree, or Saha Junk: {anyJunk * 1f / stats.Count * 100f}%");
        sb.AppendLine("Run time: " + ts.TotalSeconds + "s");
        sb.AppendLine();
        return sb.ToString();
    }

    private class StatsDetails
    {
        public int NumSpheres { get; init; }
        public List<(int, ItemType)> ItemLocations { get; init; } = new();
    }

    private static int UniqueLocations(List<(int, ItemType, int)> itemLocationCountsList, ItemType item)
    {
        return itemLocationCountsList
            .Count(x => x.Item2 == item);
    }

}
