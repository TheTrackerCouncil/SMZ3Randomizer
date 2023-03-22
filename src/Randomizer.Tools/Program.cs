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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHyphenator;
using Randomizer.Data.Logic;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
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
            //RomGenerator.GenerateRom(args);

            var output = GenerateStats(
                new Config()
                {
                    GanonsTowerCrystalCount = 7, GanonCrystalCount = 7, OpenPyramid = false, TourianBossCount = 4
                }, 10, 4, true);

            Console.Write(output);
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

        public static void GeneralAllState()
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
                    var output = GenerateStats(new Config() { KeysanityMode = keysanityMode, ItemPlacementRule = itemPlacementRule, LogicConfig = logicConfig }, 1000);
                    sb.AppendLine(output);
                    Console.WriteLine(output);
                }
            }

            Console.WriteLine("Complete!");
        }

        public static string GenerateStats(Config config, int count = 50, int playerCount = 2, bool roundRobin = true)
        {
            var start = DateTime.Now;
            var loggerFactory = new LoggerFactory();
            var stats = new ConcurrentBag<StatsDetails>();

            Parallel.For(0, count, (iteration, state) =>
            {
                try
                {
                    var worlds = new List<World>();
                    for (var i = 1; i <= playerCount; i++)
                    {
                        var worldConfig = config.SeedOnly();
                        worlds.Add(new World(worldConfig, $"Player{i}", i-1, Guid.NewGuid().ToString("N")));
                    }
                    var filler = new StandardFiller(new Logger<StandardFiller>(loggerFactory));
                    filler.RoundRobin = roundRobin;
                    filler.SetRandom(new Random(System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue)));
                    filler.Fill(worlds, config, CancellationToken.None);
                    var playthrough = Playthrough.Generate(worlds, config);

                    var generatedStats = new StatsDetails() { NumSpheres = playthrough.Spheres.Count };
                    for (var i = 0; i < playthrough.Spheres.Count; i++)
                    {
                        if (!playthrough.Spheres[i].NewlyBeatenWorlds.Any()) continue;
                        if (generatedStats.FirstSolvedSphere == 0)
                        {
                            generatedStats.FirstSolvedSphere = i + 1;
                        }
                        generatedStats.LastSolvedSphere = i + 1;
                    }

                    generatedStats.SolvedSphereSpread =
                        generatedStats.LastSolvedSphere - generatedStats.FirstSolvedSphere;

                    stats.Add(generatedStats);
                }
                catch (Exception)
                {
                }
            });

            var end = DateTime.Now;
            var ts = (end - start);

            var sb = new StringBuilder();
            sb.AppendLine("----------------------------------------------------------------------------------------");
            sb.AppendLine($"Num stats: {stats.Count}");
            sb.AppendLine($"Num players: {playerCount}");
            sb.AppendLine($"Round robin: {roundRobin}");
            sb.AppendLine($"GT crystal count: {config.GanonsTowerCrystalCount}");
            sb.AppendLine($"Ganon crystal count: {config.GanonCrystalCount}");
            sb.AppendLine($"Open pyramid: {config.OpenPyramid}");
            sb.AppendLine($"Tourian boss count: {config.TourianBossCount}");
            sb.AppendLine("");
            sb.AppendLine($"Average spheres: {stats.Average(x => x.NumSpheres)}");
            sb.AppendLine($"Max spheres: {stats.Max(x => x.NumSpheres)}");
            sb.AppendLine($"Min spheres: {stats.Min(x => x.NumSpheres)}");
            sb.AppendLine("");
            sb.AppendLine($"Average first solved sphere: {stats.Average(x => x.FirstSolvedSphere)}");
            sb.AppendLine($"Max first solved sphere: {stats.Max(x => x.FirstSolvedSphere)}");
            sb.AppendLine($"Min first solved sphere: {stats.Min(x => x.FirstSolvedSphere)}");
            sb.AppendLine("");
            sb.AppendLine($"Average last solved sphere: {stats.Average(x => x.LastSolvedSphere)}");
            sb.AppendLine($"Max last solved sphere: {stats.Max(x => x.LastSolvedSphere)}");
            sb.AppendLine($"Min last solved sphere: {stats.Min(x => x.LastSolvedSphere)}");
            sb.AppendLine("");
            sb.AppendLine($"Average solved sphere difference: {stats.Average(x => x.SolvedSphereSpread)}");
            sb.AppendLine($"Max solved sphere difference: {stats.Max(x => x.SolvedSphereSpread)}");
            sb.AppendLine($"Min solved sphere difference: {stats.Min(x => x.SolvedSphereSpread)}");
            sb.AppendLine("");
            sb.AppendLine("Run time: " + ts.TotalSeconds + "s");
            sb.AppendLine();
            return sb.ToString();
        }

        private class StatsDetails
        {
            public int NumSpheres { get; set; }
            public int FirstSolvedSphere { get; set; }
            public int LastSolvedSphere { get; set; }
            public int SolvedSphereSpread { get; set; }
        }

        private static int UniqueLocations(List<(int, ItemType, int)> itemLocationCountsList, ItemType item)
        {
            return itemLocationCountsList
                .Count(x => x.Item2 == item);
        }

    }

}
