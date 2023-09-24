using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using Randomizer.Data.Configuration;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.PatchBuilder;
using Randomizer.Shared;
using Randomizer.Sprites;

var serviceProvider = new ServiceCollection()
    .AddLogging(x =>
    {
        x.AddConsole();
    })
    .AddConfigs()
    .AddSingleton<ITrackerStateService, TrackerStateService>()
    .AddMsuRandomizerServices()
    .AddSingleton<SpriteService>()
    .AddSingleton<OptionsFactory>()
    .AddRandomizerServices()
    .AddTransient<PatchBuilderService>()
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

var typesStream = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("Randomizer.PatchBuilder.msu-randomizer-types.json");
var msuInitializationRequest = new MsuRandomizerInitializationRequest()
{
    MsuTypeConfigStream = typesStream,
    LookupMsus = false
};
serviceProvider.GetRequiredService<IMsuRandomizerInitializationService>().Initialize(msuInitializationRequest);

var yamlPath = "";
var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
do
{
    yamlPath = Path.Combine(directory.FullName, "patch-config.yml");
    directory = directory.Parent!;
} while (!File.Exists(yamlPath) && !Directory.Exists(Path.Combine(directory.FullName, "Randomizer.PatchBuilder")));

if (!File.Exists(yamlPath))
{
    logger.LogInformation("Creating blank patch-config.yml file at {Path}", yamlPath);
    var config = new PatchBuilderConfig();
    var world = new World(new Config(), "", 1, "");

    foreach (var location in world.Locations)
    {
        location.Item =
            new Item(location.VanillaItem == ItemType.Nothing ? ItemType.TwentyRupees : location.VanillaItem, world);
    }

    foreach (var dungeon in world.Dungeons.Where(x => x is IHasReward).Cast<IHasReward>())
    {
        dungeon.Reward = new Reward(dungeon.DefaultRewardType, world, dungeon);
    }

    foreach (var dungeon in world.Dungeons.Where(x => x is INeedsMedallion).Cast<INeedsMedallion>())
    {
        dungeon.Medallion = dungeon.DefaultMedallion;
    }

    config.PlandoConfig = new PlandoConfig(world);
    config.PatchOptions = new PatchOptions();
    var serializer = new YamlDotNet.Serialization.Serializer();
    var yamlText = serializer.Serialize(config);
    File.WriteAllText(yamlPath, yamlText);
}
else
{
    logger.LogInformation("YAML file found at {Path}", yamlPath);
    var yamlText = File.ReadAllText(yamlPath);
    var serializer = new YamlDotNet.Serialization.Deserializer();
    try
    {
        var config = serializer.Deserialize<PatchBuilderConfig>(yamlText);
        serviceProvider.GetRequiredService<PatchBuilderService>().CreatePatches(config);
    }
    catch (Exception e)
    {
        logger.LogError(e, "Error deserializing YAML file");
    }

}
