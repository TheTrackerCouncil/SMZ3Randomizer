using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHyphenator;
using Randomizer.Shared;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Infrastructure;

namespace Randomizer.Tools
{
    public static class Program
    {
        private const char SoftHyphen = (char)0xAD;
        private static readonly string SoftHyphens = new(SoftHyphen, 1);
        private static readonly Hyphenator s_hyphenator;

        static Program()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            s_hyphenator = new Hyphenator(HyphenatePatternsLanguage.EnglishUs,
                hyphenateSymbol: SoftHyphens,
                hyphenateLastWord: true);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static void Main(string[] args)
        {
            var logicConfig = new LogicConfig()
            {
                PreventScrewAttackSoftLock = true,
                PreventFivePowerBombSeed = true,
                LeftSandPitRequiresSpringBall = true,
                LaunchPadRequiresIceBeam = true,
                EasyEastCrateriaSkyItem = true,
                WaterwayNeedsGravitySuit = true,
            };
            var sb = new StringBuilder();
            foreach (KeysanityMode keysanityMode in Enum.GetValues(typeof(KeysanityMode)))
            {
                foreach (ItemPlacementRule itemPlacementRule in Enum.GetValues(typeof(ItemPlacementRule)))
                {
                    var output = GenerateStats(new Config() { KeysanityMode = keysanityMode, ItemPlacementRule = itemPlacementRule , LogicConfig = logicConfig }, 1000);
                    sb.AppendLine(output);
                    Console.WriteLine(output);
                }
            }

            Console.WriteLine("Complete!");
            //var rootCommand = new RootCommand("SMZ3 Randomizer command-line tools");

            /*
             * ConvertConfigs();
            TestConfigs();
            TestMerge();*/

            /*var formatText = new Command("format", "Formats text entries for ALttP.")
            {
                new Argument<FileInfo>("input",
                    "The file containing the entries to format. Every entry should be on a separate line.")
                    .ExistingOnly(),
            };
            formatText.Handler = CommandHandler.Create<FileInfo>(FormatText);
            rootCommand.AddCommand(formatText);

            //var generateLocationConfig = new Command("generate-locations", "Generates locations.json");
            //generateLocationConfig.Handler = CommandHandler.Create(GenerateLocationConfig);
            //rootCommand.AddCommand(generateLocationConfig);

            rootCommand.Invoke(args);*/
        }

        //public static void GenerateLocationConfig()
        //{
        //    var configProvider = new TrackerConfigProvider(null);
        //    var mapConfig = configProvider.GetMapConfig();
        //    var world = new World(new Config(), "", 0, "");

        //    var locations = world.Locations
        //        .OrderBy(l => l.Id)
        //        .Select(l =>
        //        {
        //            var mapLocation = mapConfig.Regions
        //                .SelectMany(x => x.Rooms)
        //                .SingleOrDefault(x => x.Name == l.Name);

        //            return new LocationInfo(
        //                id: l.Id,
        //                name: Tracker.GetUniqueNames(l, world))
        //            {
        //                X = mapLocation?.X,
        //                Y = mapLocation?.Y
        //            };
        //        })
        //        .ToImmutableList();

        //    var rooms = world.Rooms
        //        .OrderBy(r => r.GetType().FullName)
        //        .Select(r =>
        //        {
        //            var mapLocation = mapConfig.Regions
        //                .SelectMany(x => x.Rooms)
        //                .SingleOrDefault(x => x.Name == r.Name);

        //            return new RoomInfo(typeName: r.GetType().FullName,
        //                name: new SchrodingersString(new[] { r.Name }.Concat(r.AlsoKnownAs)))
        //            {
        //                X = mapLocation?.X,
        //                Y = mapLocation?.Y
        //            };
        //        })
        //        .ToImmutableList();

        //    var regions = world.Regions
        //        .OrderBy(r => r.GetType().FullName)
        //        .Select(r =>
        //        {
        //            return new RegionInfo(typeName: r.GetType().FullName,
        //                name: new SchrodingersString(new[] { r.Name }.Concat(r.AlsoKnownAs)));
        //        })
        //        .ToImmutableList();

        //    var config = new LocationConfig(regions, rooms, locations);
        //    var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        //    {
        //        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        //        WriteIndented = true,
        //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        //    });
        //    File.WriteAllText("locations.json", json, new UTF8Encoding(false));
        //}

        public static void FormatText(FileInfo input)
        {
            if (!input.Exists)
                throw new FileNotFoundException($"Could not find the file '{input.FullName}'.");

            using var reader = new StreamReader(input.OpenRead());
            while (!reader.EndOfStream)
            {
                var text = reader.ReadLine();
                var hyphenatedText = s_hyphenator.HyphenateText(text);

                var result = string.Join('\n', FormatDialog(hyphenatedText));
                if (!Dialog.FitsSimple(result))
                    Console.WriteLine("<<<DOES NOT FIT SIMPLE>>>");
                if (!Dialog.FitsCompiled(result))
                    Console.WriteLine("<<<DOES NOT FIT COMPILED>>>");
                Console.WriteLine(result);
                Console.WriteLine("---");
            }
        }

        public static IEnumerable<string> FormatDialog(ReadOnlySpan<char> text, int wrap = 19)
        {
            var result = new List<string>();

            var cursor = 0;
            while (cursor < text.Length)
            {
                // Return remainder if there is not enough text left
                if (text[cursor..].Length < wrap)
                {
                    result.Add(text[cursor..].ToString().Replace(SoftHyphens, null));
                    break;
                }

                // Find the next part that fits within the line length
                var pos = text.LastIndexOfConvenientWrappingPoint(cursor, wrap);
                if (pos < 0)
                {
                    throw new ArgumentException($"The specified text cannot be broken up into parts of {wrap} characters or less. Text:\n{text.ToString()}");
                }

                var slice = text[cursor..pos].ToString().Replace(SoftHyphens, null); // Remove remaining soft hyphens
                if (text[pos] == SoftHyphen)
                    slice += '-'; // Turn the soft hyphen into a hard hyphen if we wrapped at one
                result.Add(slice);
                cursor = pos + 1; // Eat the space or hyphen
            }

            return result;
        }

        /// <summary>
        /// Returns the index of the last character in the string at which text
        /// should be wrapped.
        /// </summary>
        /// <param name="span">The span to search through.</param>
        /// <param name="start">The index at which to begin searching.</param>
        /// <param name="maxLength">
        /// The maximum number of characters to search through (excluding soft
        /// hyphens).
        /// </param>
        /// <returns>
        /// The index of the last character near the wrapping point, or -1.
        /// </returns>
        public static int LastIndexOfConvenientWrappingPoint(this ReadOnlySpan<char> span, int start, int maxLength)
        {
            var softHyphens = span.Slice(start, maxLength).ToArray().Count(x => x == SoftHyphen);
            var slice = start + maxLength + softHyphens > span.Length
                ? span.Slice(start)
                : span.Slice(start, maxLength + softHyphens);
            return start + slice.LastIndexOfAny(' ', SoftHyphen);
        }

        public static string GenerateStats(Config config, int count = 50)
        {
            var start = DateTime.Now;
            var loggerFactory = new LoggerFactory();
            var worldAccessor = new WorldAccessor();
            var filler = new StandardFiller(new Logger<StandardFiller>(loggerFactory));
            var randomizer = new Smz3Randomizer(filler, worldAccessor, new Logger<Smz3Randomizer>(loggerFactory));
            var stats = new ConcurrentBag<StatsDetails>();
            var itemCounts = new ConcurrentDictionary<(int itemId, int locationId), int>();

            Parallel.For(0, count, (iteration, state) =>
            {
                try
                {
                    var seed = randomizer.GenerateSeed(config.SeedOnly(), null);
                    var playthrough = Playthrough.Generate(seed.Worlds.Select(x => x.World).ToList(), config);
                    stats.Add(new()
                    {
                        NumSpheres = playthrough.Spheres.Count,
                        ItemLocations = seed.Worlds.First().World.Locations.Select(x => (x.Id, x.Item.Type)).ToList()
                    });
                }
                catch (Exception)
                { 
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
            sb.AppendLine("Keysanity: " + config.KeysanityMode + " | Item placement: " + config.ItemPlacementRule);
            sb.AppendLine("----------------------------------------------------------------------------------------");
            sb.AppendLine($"Num succeeded: {stats.Count} ({stats.Count * 1f / count * 100f}%)");
            sb.AppendLine("Average spheres: " + stats.Sum(x => x.NumSpheres) * 1f / stats.Count() * 1f);
            sb.AppendLine("Max spheres: " + stats.Max(x => x.NumSpheres));
            sb.AppendLine("Min spheres: " + stats.Min(x => x.NumSpheres));
            sb.AppendLine("Item with most unique locations: " + mostCommon + " with " + mostCount + " unique locations");
            sb.AppendLine("Item with least unique locations: " + leastCommon + " with " + leastCount + " unique locations");
            sb.AppendLine("Run time: " + ts.TotalSeconds + "s");
            sb.AppendLine();
            return sb.ToString();
        }

        private class StatsDetails
        {
            public int NumSpheres { get; set; }
            public List<(int, ItemType)> ItemLocations { get; set; }
        }

        private static int UniqueLocations(List<(int, ItemType, int)> itemLocationCountsList, ItemType item)
        {
            return itemLocationCountsList
               .Where(x => x.Item2 == item)
               .Count();
        }

    }

}
