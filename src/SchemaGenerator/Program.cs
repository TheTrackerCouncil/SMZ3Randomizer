using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema.Generation;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
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
        (typeof(UIConfig), "ui.json"),
    };

    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(LogEventLevel.Information)
            //.WriteTo.Debug()
            .CreateLogger();

        var services = new ServiceCollection()
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

        var settings = new JsonSchemaGeneratorSettings();
        var generator = new JsonSchemaGenerator(settings);

        var test = new Regex("""[ \t]*"description": "Represents multiple possibilities of a string.",[ \t]*?\r?\n""");

        var schemaPath = Path.Combine(outputPath, "Schemas");
        if (!Directory.Exists(schemaPath))
        {
            Directory.CreateDirectory(schemaPath);
        }
        foreach (var type in s_generationTypes)
        {
            var path = Path.Combine(schemaPath, type.Item2);
            var schema = generator.Generate(type.Item1);
            var text = test.Replace(schema.ToJson(), "").Replace("\\n", " ");

            for (var i = 0; i < 5; i++)
            {
                text = text.Replace("{" + i + "}", "\\n{" + i + "}");
            }
            File.WriteAllText(path, text);
            Log.Information("Wrote {Type} schema to {Path}", type.Item1.FullName, path);
        }

        var configProvider = services.GetRequiredService<ConfigProvider>();
        var bossConfig = configProvider.GetBossConfig(new List<string>(), null);

        var templateBossConfig = new BossConfig();
        foreach (var boss in bossConfig)
        {
            templateBossConfig.Add(new BossInfo()
            {
                Boss = boss.Boss,
                Name = new SchrodingersString(),
                WhenTracked = new SchrodingersString(),
                WhenDefeated = new SchrodingersString()
            });
        }

        var serializer = new SerializerBuilder()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults)
            .DisableAliases()
            .Build();
        var yamlText = serializer.Serialize(templateBossConfig);
        File.WriteAllText($"/home/matt/Documents/yaml/bosses.yml", yamlText);
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
