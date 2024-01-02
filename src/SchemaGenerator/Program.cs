using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema.Generation;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Serilog;
using Serilog.Events;
using YamlDotNet.Serialization;

namespace SchemaGenerator;

public static class Program
{
    private static readonly List<(Type, string)> s_generationTypes = new()
    {
        (typeof(BossConfig), "bosses.json"),
        (typeof(DungeonConfig), "dungeons.json"),
        (typeof(GameLinesConfig), "game.json"),
        (typeof(HintTileConfig), "hint_tiles.json"),
        (typeof(ItemConfig), "items.json"),
        (typeof(LocationConfig), "locations.json"),
        (typeof(MsuConfig), "msu.json"),
        (typeof(RegionConfig), "regions.json"),
        (typeof(RequestConfig), "requests.json"),
        (typeof(ResponseConfig), "responses.json"),
        (typeof(RewardConfig), "rewards.json"),
        (typeof(RoomConfig), "rooms.json"),
        (typeof(UIConfig), "ui.json"),
    };

    private static IServiceProvider? _services;

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(LogEventLevel.Information)
            //.WriteTo.Debug()
            .CreateLogger();

        _services = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.AddSerilog(dispose: true);
            })
            .AddConfigs()
            .BuildServiceProvider();

        var outputPath = GetOutputPath();
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        Log.Information("Output path: {Path}", outputPath);

        CreateSchemas(outputPath);
        CreateTemplates(outputPath);
    }

    private static void CreateSchemas(string outputPath)
    {
        var schrodingersStringReplacement = new Regex("""[ \t]*"description": "Represents multiple possibilities of a string.",[ \t]*?\r?\n""");
        var settings = new JsonSchemaGeneratorSettings()
        {
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.Null,
            DefaultDictionaryValueReferenceTypeNullHandling = ReferenceTypeNullHandling.Null,
        };
        var generator = new JsonSchemaGenerator(settings);

        var schemaPath = Path.Combine(outputPath, "Schemas");
        if (!Directory.Exists(schemaPath))
        {
            Directory.CreateDirectory(schemaPath);
        }
        foreach (var type in s_generationTypes)
        {
            var path = Path.Combine(schemaPath, type.Item2);
            var schema = generator.Generate(type.Item1);
            var text = schrodingersStringReplacement.Replace(schema.ToJson(), "").Replace("\\n", " ");

            for (var i = 0; i < 5; i++)
            {
                text = text.Replace("{" + i + "}", "\\n{" + i + "}");
            }
            File.WriteAllText(path, text);
            Log.Information("Wrote {Type} schema to {Path}", type.Item1.FullName, path);
        }
    }

    private static void CreateTemplates(string outputPath)
    {
        var templatePath = Path.Combine(outputPath, "Profiles", "Templates");
        var configProvider = _services!.GetRequiredService<ConfigProvider>();

        // Boss Template
        var bossConfig = configProvider.GetBossConfig(new List<string>(), null);
        var templateBossConfig = new BossConfig();
        templateBossConfig.AddRange(bossConfig.Select(boss => new BossInfo { Boss = boss.Boss }));
        var exampleBossConfig = BossConfig.Example();
        WriteTemplate(templatePath, "bosses", templateBossConfig, exampleBossConfig);

        // Dungeon Template
        var dungeonConfig = configProvider.GetDungeonConfig(new List<string>(), null);
        var templateDungeonConfig = new DungeonConfig();
        templateDungeonConfig.AddRange(dungeonConfig.Select(dungeon => new DungeonInfo() { Dungeon = dungeon.Dungeon, Type = templateDungeonConfig.GetType() }));
        var exampleDungeonConfig = DungeonConfig.Example();
        WriteTemplate(templatePath, "dungeons", templateDungeonConfig, exampleDungeonConfig);

        // Item Template
        var itemConfig = configProvider.GetItemConfig(new List<string>(), null);
        var templateItemConfig = new ItemConfig();
        templateItemConfig.AddRange(itemConfig.Select(item => new ItemData() { Item = item.Item }));
        var exampleItemConfig = ItemConfig.Example();
        WriteTemplate(templatePath, "items", templateItemConfig, exampleItemConfig);

        // Location Template
        var locationConfig = configProvider.GetLocationConfig(new List<string>(), null);
        var templateLocationConfig = new LocationConfig();
        templateLocationConfig.AddRange(locationConfig.Select(loc => new LocationInfo() { LocationId = loc.LocationId }));
        var exampleLocationConfig = LocationConfig.Example();
        WriteTemplate(templatePath, "locations", templateLocationConfig, exampleLocationConfig);

        // Region Template
        var regionConfig = configProvider.GetRegionConfig(new List<string>(), null);
        var templateRegionConfig = new RegionConfig();
        templateRegionConfig.AddRange(regionConfig.Select(region => new RegionInfo() { Region = region.Region, Type = templateRegionConfig.GetType()}));
        var exampleRegionConfig = RegionConfig.Example();
        WriteTemplate(templatePath, "regions", templateRegionConfig, exampleRegionConfig);

        // Reward Template
        var rewardConfig = configProvider.GetRewardConfig(new List<string>(), null);
        var templateRewardConfig = new RewardConfig();
        templateRewardConfig.AddRange(rewardConfig.Select(reward => new RewardInfo() { Reward = reward.Reward}));
        var exampleRewardConfig = RewardConfig.Example();
        WriteTemplate(templatePath, "rewards", templateRewardConfig, exampleRewardConfig);

        // Room Template
        var roomConfig = configProvider.GetRoomConfig(new List<string>(), null);
        var templateRoomConfig = new RoomConfig();
        templateRoomConfig.AddRange(roomConfig.Select(room => new RoomInfo() { Room = room.Room, Type = templateRegionConfig.GetType()}));
        var exampleRoomConfig = RoomConfig.Example();
        WriteTemplate(templatePath, "rooms", templateRoomConfig, exampleRoomConfig);

        // Game Template
        var templateGameConfig = new GameLinesConfig();
        PopulateExample(templateGameConfig, true);
        var exampleGameConfig = GameLinesConfig.Example();
        WriteTemplate(templatePath, "game", templateGameConfig, exampleGameConfig);

        // Request Template
        var templateRequestConfig = new RequestConfig() { new BasicVoiceRequest(new List<string>() { ""}, new SchrodingersString()) };
        var exampleRequestConfig = RequestConfig.Example();
        WriteTemplate(templatePath, "requests", templateRequestConfig, exampleRequestConfig);

        // Response Template
        var templateResponseConfig = new ResponseConfig() { };
        PopulateExample(templateResponseConfig, true);
        var exampleResponseConfig = ResponseConfig.Example();
        WriteTemplate(templatePath, "responses", templateResponseConfig, exampleResponseConfig);

        // Hint Tiles Template
        var hintTileConfig = configProvider.GetHintTileConfig(new List<string>(), null);
        var templateHintTileConfig = new HintTileConfig() { HintTiles = new HintTileList()};
        templateHintTileConfig.HintTiles.AddRange(hintTileConfig.HintTiles!.Select(x => new HintTile() { HintTileKey = x.HintTileKey, TopLeftX = 0, TopLeftY = 0, Room = 0 }));
        PopulateExample(templateHintTileConfig, true);
        var exampleHintTileConfig = HintTileConfig.Example();
        WriteTemplate(templatePath, "hint_tiles", templateHintTileConfig, exampleHintTileConfig);

        // MSU Template
        var msuConfig = configProvider.GetMsuConfig(new List<string>(), null);
        var templateMsuConfig = new MsuConfig()
        {
            TrackLocations = msuConfig.TrackLocations?.ToDictionary(x => x.Key, _ => (SchrodingersString?)null) ??
                             new Dictionary<int, SchrodingersString?>()
        };
        var exampleMsuConfig = MsuConfig.Example();
        WriteTemplate(templatePath, "msu", templateMsuConfig, exampleMsuConfig);

        // UI Template
        var templateUIConfig = new UIConfig() { new("Layout Name", new List<UIGridLocation> { new() { Identifiers = new List<string> { "" }, Column = 1, Row = 1 } }) };
        var exampleUIConfig = UIConfig.Example();
        WriteTemplate(templatePath, "ui", templateUIConfig, exampleUIConfig);
    }

    private static void WriteTemplate(string templatePath, string type, object data, object? example)
    {
        var serializer = new SerializerBuilder()
            //.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .DisableAliases()
            .Build();
        var output = $"# yaml-language-server: $schema=../../Schemas/{type}.json\r\n";

        var desc = data.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description;
        if (desc != null)
        {
            var description = string.Join("\n", desc.Split("\n").Select(x => $"# {x}"));
            output += "\r\n" + description + "\r\n";
        }

        if (example != null)
        {
            var exampleYaml = string.Join("\n", serializer.Serialize(example).Split("\n").Select(x => $"# {x}"));
            exampleYaml = Regex.Replace(exampleYaml, @"#\s+Weight: 1\r?\n", "");
            output += "\r\n# Example:\r\n" + exampleYaml + "\r\n\r\n";
        }

        output += serializer.Serialize(data).Replace("[]", "").Replace("{}", "");
        output = Regex.Replace(output, @"LocationNumber: 0\r?\n\s+", "");
        var path = Path.Combine(templatePath, $"{type}.yml");
        File.WriteAllText(path, output);
        Log.Information("Wrote {Type} template to {Path}", data.GetType().Name, path);
    }

    private static void PopulateExample(object example, bool populateEmpty)
    {
        var properties = example.GetType().GetProperties();

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(SchrodingersString))
            {
                property.SetValue(example, GetPopulatedSchrodingersString(property.Name, populateEmpty));
            }
            else if (property.PropertyType.GetInterfaces().Any(x => x.Name.StartsWith("IMergeable")))
            {
                var value = property.GetValue(example);
                if (value != null)
                {
                    PopulateExample(value, populateEmpty);
                }
            }
        }
    }

    private static SchrodingersString GetPopulatedSchrodingersString(string name, bool populateEmpty)
    {
        if (populateEmpty)
        {
            return new SchrodingersString();
        }
        else
        {
            var possibilities = new List<SchrodingersString.Possibility>
            {
                new($"{name} Example 1"), new($"{name} Example 2", 0.1),
            };
            return new SchrodingersString(possibilities);
        }
    }

    private static string GetOutputPath()
    {
        var slnDirectory = new DirectoryInfo(SolutionPath);
        if (slnDirectory.Parent?.GetDirectories().Any(x => x.Name == "SMZ3CasConfigs") == true)
        {
            return slnDirectory.Parent.GetDirectories().First(x => x.Name == "SMZ3CasConfigs").FullName;
        }
        else
        {
            return Path.Combine(slnDirectory.FullName, "src", "SchemaGenerator", "Output");
        }
    }

    private static string SolutionPath
    {
        get
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return Path.Combine(directory!.FullName);
        }
    }
}
