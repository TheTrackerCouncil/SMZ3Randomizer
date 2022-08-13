using System;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHyphenator;
using Randomizer.Shared;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using LocationConfig = Randomizer.SMZ3.Tracking.Configuration.ConfigFiles.LocationConfig;

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

        /*
        public static void ConvertConfigs()
        {
            TrackerConfigProvider trackerConfigProvider = new(null);
            var config = trackerConfigProvider.GetTrackerConfig();
            ConvertItems(config);
            ConvertResponses(config);
            ConvertRequests(config);

            var locationConfig = trackerConfigProvider.GetLocationConfig();
            ConvertLocationConfigs(locationConfig);
        }

        public static void TestConfigs()
        {
            var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();

            Console.WriteLine("Items");
            var yml = File.ReadAllText(@"D:\Documents\ConfigsTest\output\items.yml");
            var items = deserializer.Deserialize<ItemConfig>(yml);
            Console.WriteLine($"{items[0].Item} => {items[0].Name}");
            Console.WriteLine($"{items[items.Count-1].Item} => {items[items.Count - 1].Name}");

            Console.WriteLine("");

            Console.WriteLine("Responses");
            yml = File.ReadAllText(@"D:\Documents\ConfigsTest\output\responses.yml");
            var responses = deserializer.Deserialize<ResponseConfig>(yml);
            Console.WriteLine(responses.StartedTracking);
            Console.WriteLine(responses.Chat.RecognizedGreetings.First());
            Console.WriteLine(responses.Idle["5m±2m"]);

            Console.WriteLine("");

            Console.WriteLine("Requests");
            yml = File.ReadAllText(@"D:\Documents\ConfigsTest\output\requests.yml");
            var requests = deserializer.Deserialize<RequestConfig>(yml);
            Console.WriteLine($"{requests[0].Phrases[0]} => {requests[0].Response}");
            Console.WriteLine($"{requests[requests.Count - 1].Phrases[0]} => {requests[requests.Count-1].Response}");

            Console.WriteLine("");

            Console.WriteLine("Regions");
            yml = File.ReadAllText(@"D:\Documents\ConfigsTest\output\regions.yml");
            var regions = deserializer.Deserialize<RegionConfig>(yml);
            Console.WriteLine($"{regions[0].Region} => {regions[0].Name}");
            Console.WriteLine($"{regions[regions.Count - 1].Region} => {regions[regions.Count - 1].Name}");

            Console.WriteLine("");

            Console.WriteLine("Dungeons");
            yml = File.ReadAllText(@"D:\Documents\ConfigsTest\output\dungeons.yml");
            var dungeons = deserializer.Deserialize<DungeonConfig>(yml);
            Console.WriteLine($"{dungeons[0].Dungeon} => {dungeons[0].Name}");
            Console.WriteLine($"{dungeons[dungeons.Count - 1].Dungeon} => {dungeons[dungeons.Count - 1].Name}");

            Console.WriteLine("");

            Console.WriteLine("Bosses");
            yml = File.ReadAllText(@"D:\Documents\ConfigsTest\output\bosses.yml");
            var bosses = deserializer.Deserialize<BossConfig>(yml);
            Console.WriteLine($"{bosses[0].Boss} => {bosses[0].Name}");
            Console.WriteLine($"{bosses[bosses.Count - 1].Boss} => {bosses[bosses.Count - 1].Name}");

            Console.WriteLine("");

            Console.WriteLine("Rooms");
            yml = File.ReadAllText(@"D:\Documents\ConfigsTest\output\rooms.yml");
            var rooms = deserializer.Deserialize<RoomConfig>(yml);
            Console.WriteLine($"{rooms[0].Room} => {rooms[0].Name}");
            Console.WriteLine($"{rooms[rooms.Count - 1].Room} => {rooms[rooms.Count - 1].Name}");

            Console.WriteLine("");

            Console.WriteLine("Locations");
            yml = File.ReadAllText(@"D:\Documents\ConfigsTest\output\locations.yml");
            var locations = deserializer.Deserialize<Randomizer.SMZ3.Tracking.Configuration.ConfigFiles.LocationConfig>(yml);
            Console.WriteLine($"{locations[0].LocationNumber} => {locations[0].Name}");
            Console.WriteLine($"{locations[locations.Count - 1].LocationNumber} => {locations[locations.Count - 1].Name}");
        }

        public static void ConvertItems(TrackerConfig config)
        {
            var items = config.Items.ToList();

            // Add Key to old items
            foreach (var item in items)
            {
                item.Item = item.Name[0].Text.Trim();
            }

            // Add maps as they are relevant with keysanity
            foreach (var val in Enum.GetValues(typeof(ItemType)))
            {
                var item = (ItemType)val;
                if (!items.Any(x => x.InternalItemType == item) && item != ItemType.BigKey && item != ItemType.Map && item != ItemType.Key && item != ItemType.Compass)
                {
                    if (item.IsInCategory(ItemCategory.SmallKey))
                    {
                        //Console.WriteLine($"Added {item.GetDescription().Trim()}");
                        items.Add(new ItemData(new(item.GetDescription()), item, new("It opens doors.")));
                    }
                    else if (item.IsInCategory(ItemCategory.Map))
                    {
                        //Console.WriteLine($"Added {item.GetDescription().Trim()}");
                        items.Add(new ItemData(new(item.GetDescription()), item, new("It helps you find a place.")));
                    }
                    else if (item.IsInCategory(ItemCategory.Compass))
                    {
                        //Console.WriteLine($"Added {item.GetDescription().Trim()}");
                        items.Add(new ItemData(new(item.GetDescription()), item, new("It points you to the boss.")));
                    }
                    else
                    {
                        Console.WriteLine($"Skipped item {item.GetDescription()}");
                    }
                }
            }

            // Write new json
            var json = System.Text.Json.JsonSerializer.Serialize(new ItemConfig(items.OrderBy(x => x.InternalItemType).ToList()), new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
            File.WriteAllText("D:\\Documents\\ConfigsTest\\input\\items.json", json, new UTF8Encoding(false));
        }

        public static void ConvertResponses(TrackerConfig config)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(config.Responses, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
            File.WriteAllText("D:\\Documents\\ConfigsTest\\input\\responses.json", json, new UTF8Encoding(false));

            var expConverter = new ExpandoObjectConverter();
            dynamic deserializedObject = JsonConvert.DeserializeObject<ExpandoObject>(json, expConverter);

            var serializer = new SerializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();
            string yaml = serializer.Serialize(deserializedObject);

            File.WriteAllText(@"D:\Documents\ConfigsTest\output\responses.yml", yaml, new UTF8Encoding(false));
        }

        public static void ConvertRequests(TrackerConfig config)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(config.Requests, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
            File.WriteAllText("D:\\Documents\\ConfigsTest\\input\\requests.json", json, new UTF8Encoding(false));
        }

        public static void ConvertLocationConfigs(Randomizer.SMZ3.Tracking.Configuration.LocationConfig config)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            var json = System.Text.Json.JsonSerializer.Serialize(config.Regions, options);
            File.WriteAllText("D:\\Documents\\ConfigsTest\\input\\regions.json", json, new UTF8Encoding(false));

            json = System.Text.Json.JsonSerializer.Serialize(config.Dungeons, options);
            File.WriteAllText("D:\\Documents\\ConfigsTest\\input\\dungeons.json", json, new UTF8Encoding(false));

            json = System.Text.Json.JsonSerializer.Serialize(config.Bosses, options);
            File.WriteAllText("D:\\Documents\\ConfigsTest\\input\\bosses.json", json, new UTF8Encoding(false));

            json = System.Text.Json.JsonSerializer.Serialize(config.Rooms, options);
            File.WriteAllText("D:\\Documents\\ConfigsTest\\input\\rooms.json", json, new UTF8Encoding(false));

            json = System.Text.Json.JsonSerializer.Serialize(config.Locations, options);
            File.WriteAllText("D:\\Documents\\ConfigsTest\\input\\locations.json", json, new UTF8Encoding(false));
        }

        public static void TestMerge()
        {
            var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();
            var yml = "";

            Console.WriteLine("");
            Console.WriteLine("Boss Merge Start");
            var bossDefault = BossConfig.Default();
            bossDefault.ForEach(x => Console.WriteLine($"  {x.Boss} - {x.Name} - {x.WhenDefeated ?? "Null"} - {x.WhenTracked ?? "Null"}"));
            Console.WriteLine("Boss Merge Complete");
            yml = File.ReadAllText(@"D:\Source\SMZ3Randomizer\src\Randomizer.SMZ3.Tracking\Configuration\Yaml\BCU\bosses.yml");
            var bossOther = deserializer.Deserialize<BossConfig>(yml);
            ((IMergeable<BossInfo>)bossDefault).MergeFrom(bossOther);
            bossDefault.ForEach(x => Console.WriteLine($"  {x.Boss} - {x.Name} - {x.WhenDefeated ?? "Null"} - {x.WhenTracked ?? "Null"}"));

            Console.WriteLine("");
            Console.WriteLine("Dungeon Merge Start");
            var dungeonDefault = DungeonConfig.Default();
            dungeonDefault.ForEach(x => Console.WriteLine($"  {x.Dungeon} - {x.Name} - {x.Boss ?? "Null"}"));
            Console.WriteLine("Dungeon Merge Complete");
            yml = File.ReadAllText(@"D:\Source\SMZ3Randomizer\src\Randomizer.SMZ3.Tracking\Configuration\Yaml\BCU\dungeons.yml");
            var dungeonOther = deserializer.Deserialize<DungeonConfig>(yml);
            ((IMergeable<DungeonInfo>)dungeonDefault).MergeFrom(dungeonOther);
            dungeonDefault.ForEach(x => Console.WriteLine($"  {x.Dungeon} - {x.Name} - {x.Boss ?? "Null"}"));
            
            Console.WriteLine("");
            Console.WriteLine("Items Merge Start");
            var itemNames = new List<string>() { "Nothing", "Death", "Content", "Shaktool Go Mode", "Hammer", "Sword" };
            var itemsDefault = ItemConfig.Default();
            itemsDefault.Where(x => itemNames.Contains(x.Item)).ToList()
                .ForEach(x => Console.WriteLine($"  {x.Name} - {x.Plural} - {x.Hints} - {x.WhenTracked?[0] ?? "Null"} - {x.Stages?[0] ?? "Null"}"));
            Console.WriteLine("Items Merge Complete");
            yml = File.ReadAllText(@"D:\Source\SMZ3Randomizer\src\Randomizer.SMZ3.Tracking\Configuration\Yaml\BCU\items.yml");
            var itemsOther = deserializer.Deserialize<ItemConfig>(yml);
            ((IMergeable<ItemData>)itemsDefault).MergeFrom(itemsOther);
            itemsDefault.Where(x => itemNames.Contains(x.Item)).ToList().
                ForEach(x => Console.WriteLine($"  {x.Name} - {x.Plural} - {x.Hints} - {x.WhenTracked?[1] ?? "Null"} - {x.Stages?[1] ?? "Null"}"));

            Console.WriteLine("");
            Console.WriteLine("Locations Merge Start");
            var locationsDefault = LocationConfig.Default();
            locationsDefault.Where(x => x.LocationNumber is 6 or 44 or 150 or 260 or 270 or 273 or 317 or 331 or 496).ToList()
                .ForEach(x => Console.WriteLine($"  {x.LocationNumber} - {x.Name} - {x.WhenTrackingJunk?[0] ?? "Null"} - {x.WhenTrackingProgression?[0] ?? "Null"} - {x.WhenMarkingJunk?[0] ?? "Null"} - {x.WhenMarkingProgression?[0] ?? "Null"}"));
            Console.WriteLine("Locations Merge Complete");
            yml = File.ReadAllText(@"D:\Source\SMZ3Randomizer\src\Randomizer.SMZ3.Tracking\Configuration\Yaml\BCU\locations.yml");
            var locationsOther = deserializer.Deserialize<LocationConfig>(yml);
            ((IMergeable<LocationInfo>)locationsDefault).MergeFrom(locationsOther);
            locationsDefault.Where(x => x.LocationNumber is 6 or 44 or 150 or 260 or 270 or 273 or 317 or 331 or 496).ToList()
                .ForEach(x => Console.WriteLine($"  {x.LocationNumber} - {x.Name} - {x.WhenTrackingJunk?[0] ?? "Null"} - {x.WhenTrackingProgression?[0] ?? "Null"} - {x.WhenMarkingJunk?[0] ?? "Null"} - {x.WhenMarkingProgression?[0] ?? "Null"}"));


            Console.WriteLine("");
            Console.WriteLine("Regions Merge Start");
            var regionsDefault = RegionConfig.Default();
            var regionNames = new List<string>() { "Misery Mire", "Ice Palace", "Blue Brinstar" };
            regionsDefault.Where(x => regionNames.Contains(x.Region)).ToList()
                .ForEach(x => Console.WriteLine($"  {x.Region} - {x.Name} - {x.Hints ?? "Null"}"));
            Console.WriteLine("Regions Merge Complete");
            yml = File.ReadAllText(@"D:\Source\SMZ3Randomizer\src\Randomizer.SMZ3.Tracking\Configuration\Yaml\BCU\regions.yml");
            var regionsOther = deserializer.Deserialize<RegionConfig>(yml);
            ((IMergeable<RegionInfo>)regionsDefault).MergeFrom(regionsOther);
            regionsDefault.Where(x => regionNames.Contains(x.Region)).ToList()
                .ForEach(x => Console.WriteLine($"  {x.Region} - {x.Name} - {x.Hints ?? "Null"}"));

            Console.WriteLine("");
            Console.WriteLine("Requests Merge Start");
            var requestsDefault = RequestConfig.Default();
            requestsDefault.Where(x => requestsDefault.IndexOf(x) == 0 || requestsDefault.IndexOf(x) == requestsDefault.Count - 1).ToList()
                .ForEach(x => Console.WriteLine($"  {x.Phrases[0]} - {x.Response}"));
            Console.WriteLine("Requests Merge Complete");
            yml = File.ReadAllText(@"D:\Source\SMZ3Randomizer\src\Randomizer.SMZ3.Tracking\Configuration\Yaml\BCU\requests.yml");
            var requestsOther = deserializer.Deserialize<RequestConfig>(yml);
            ((IMergeable<BasicVoiceRequest>)requestsDefault).MergeFrom(requestsOther);
            requestsDefault.Where(x => requestsDefault.IndexOf(x) == 0 || requestsDefault.IndexOf(x) == requestsDefault.Count - 1).ToList()
                .ForEach(x => Console.WriteLine($"  {x.Phrases[0]} - {x.Response}"));

            Console.WriteLine("");
            Console.WriteLine("Responses Merge Start");
            var responsesDefault = ResponseConfig.Default();
            Console.WriteLine($"  {responsesDefault.StartedTracking}");
            Console.WriteLine($"  {responsesDefault.PegWorldAvailable}");
            Console.WriteLine($"  {responsesDefault.Map.NotInDarkRoom}");
            Console.WriteLine($"  {responsesDefault.Cheats.DisabledCheats}");
            Console.WriteLine("Responses Merge Complete");
            yml = File.ReadAllText(@"D:\Source\SMZ3Randomizer\src\Randomizer.SMZ3.Tracking\Configuration\Yaml\BCU\responses.yml");
            var responsesOther = deserializer.Deserialize<ResponseConfig>(yml);
            ((IMergeable<ResponseConfig>)responsesDefault).MergeFrom(responsesOther);
            Console.WriteLine($"  {responsesDefault.StartedTracking}");
            Console.WriteLine($"  {responsesDefault.PegWorldAvailable}");
            Console.WriteLine($"  {responsesDefault.Map.NotInDarkRoom}");
            Console.WriteLine($"  {responsesDefault.Cheats.DisabledCheats}");

            Console.WriteLine("");
            Console.WriteLine("Rooms Merge Start");
            var roomsDefault = RoomConfig.Default();
            roomsDefault.Where(x => roomsDefault.IndexOf(x) == 0 || roomsDefault.IndexOf(x) == roomsDefault.Count - 1).ToList()
                .ForEach(x => Console.WriteLine($"  {x.Name} - {x.Hints}"));
            Console.WriteLine("Rooms Merge Complete");
            yml = File.ReadAllText(@"D:\Source\SMZ3Randomizer\src\Randomizer.SMZ3.Tracking\Configuration\Yaml\BCU\rooms.yml");
            var roomsOther = deserializer.Deserialize<RoomConfig>(yml);
            ((IMergeable<RoomInfo>)roomsDefault).MergeFrom(roomsOther);
            roomsDefault.Where(x => roomsDefault.IndexOf(x) == 0 || roomsDefault.IndexOf(x) == roomsDefault.Count - 1).ToList()
                .ForEach(x => Console.WriteLine($"  {x.Name} - {x.Hints}"));
        }*/

    }

}
