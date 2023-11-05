using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.CrossPlatform;
using Randomizer.Data.Options;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Generation;
using Serilog;
using Serilog.Events;

namespace Randomizer.Tools
{
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

            var treeProgression = 0;
            var treeNice = 0;
            var treeJunk = 0;

            var hintService = s_services.GetRequiredService<IGameHintService>();

            Parallel.For(0, count, new ParallelOptions() { MaxDegreeOfParallelism = 12 }, (_, _) =>
            {
                try
                {
                    var seed = s_services.GetRequiredService<Smz3Randomizer>().GenerateSeed(config);
                    var playthrough = Playthrough.Generate(seed.WorldGenerationData.Select(x => x.World).ToList(), config);
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
                            break;
                        case LocationUsefulness.NiceToHave:
                        case LocationUsefulness.Sword:
                            pedNice++;
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
                            break;
                        case LocationUsefulness.NiceToHave:
                        case LocationUsefulness.Sword:
                            treeNice++;
                            break;
                        default:
                            treeJunk++;
                            break;
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
            sb.AppendLine($"Tree Progression: {treeProgression * 1f / stats.Count * 100f}%");
            sb.AppendLine($"Tree Nice: {treeNice * 1f / stats.Count * 100f}%");
            sb.AppendLine($"Tree Junk: {treeJunk * 1f / stats.Count * 100f}%");
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

}
