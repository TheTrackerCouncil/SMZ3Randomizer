using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Models;
using MSURandomizerLibrary.Services;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.PatchBuilder;
using TrackerCouncil.Smz3.SeedGenerator;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared.Enums;

var serviceProvider = new ServiceCollection()
    .AddLogging(x =>
    {
        x.AddConsole();
    })
    .AddConfigs()
    .AddSingleton<ITrackerStateService, TrackerStateService>()
    .AddSingleton<TrackerSpriteService>()
    .AddMsuRandomizerServices()
    .AddSingleton<SpriteService>()
    .AddSingleton<OptionsFactory>()
    .AddRandomizerServices()
    .AddTransient<PatchBuilderService>()
    .AddTransient<RomLauncherService>()
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

InitializeMsuRandomizer(serviceProvider.GetRequiredService<IMsuRandomizerInitializationService>());

var yamlPath = "";
var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
do
{
    yamlPath = Path.Combine(directory.FullName, "patch-config.yml");
    directory = directory.Parent!;
    logger.LogInformation($"Reading config from {yamlPath}");
} while (!File.Exists(yamlPath) && !Directory.Exists(Path.Combine(directory.FullName, "TrackerCouncil.Smz3.PatchBuilder")));

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

    foreach (var reward in world.Rewards)
    {
        reward.Region = null;
    }

    foreach (var rewardRegion in world.RewardRegions)
    {
        rewardRegion.SetRewardType(rewardRegion.DefaultRewardType);
    }

    foreach (var dungeon in world.PrerequisiteRegions)
    {
        dungeon.RequiredItem = dungeon.DefaultRequiredItem;
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

static void InitializeMsuRandomizer(IMsuRandomizerInitializationService msuRandomizerInitializationService)
{
    var settingsStream =  Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("TrackerCouncil.Smz3.PatchBuilder.msu-randomizer-settings.yml");
    var msuInitializationRequest = new MsuRandomizerInitializationRequest()
    {
        MsuAppSettingsStream = settingsStream,
        LookupMsus = false
    };
    msuRandomizerInitializationService.Initialize(msuInitializationRequest);
}
