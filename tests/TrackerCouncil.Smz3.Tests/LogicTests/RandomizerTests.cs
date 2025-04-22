using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;
using TrackerCouncil.Smz3.SeedGenerator.Generation;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared.Enums;
using Xunit;

namespace TrackerCouncil.Smz3.Tests.LogicTests;

public class RandomizerTests
{
    // If this test breaks, update Smz3Randomizer.Version
    [Theory]
    [InlineData("test", -75398755)] // Smz3Randomizer v8
    public void StandardFillerWithSameSeedGeneratesSameWorld(string seed, int expectedHash)
    {
        var filler = new StandardFiller(GetLogger<StandardFiller>());
        var randomizer = GetRandomizer();
        var config = new Config();
        config.LogicConfig.QuarterMagic = false;
        config.CasPatches.RandomizedBottles = false;

        var seedData = randomizer.GenerateSeed(config, seed, default);
        var worldHash = GetHashForWorld(seedData.WorldGenerationData.LocalWorld.World);

        worldHash.Should().Be(expectedHash);
    }

    // Do not change the logic of this method. If you absolutely must, make
    // sure to update the expected hash in tests that rely on it
    private static int GetHashForWorld(World world)
    {
        var stringBuilder = new StringBuilder();
        foreach (var location in world.Locations.OrderBy(x => x.Id))
        {
            stringBuilder.Append(location.Id);
            stringBuilder.Append(':');
            stringBuilder.Append((int)location.Item.Type);
            stringBuilder.Append("\r\n");
        }
        var serializedWorld = stringBuilder.ToString();
        return NonCryptographicHash.Fnv1a(serializedWorld);
    }

    [Fact]
    public void LocationItemConfig()
    {
        var filler = new StandardFiller(GetLogger<StandardFiller>());
        var randomizer = GetRandomizer();

        var config = new Config();
        var world = new World(config, "", 0, "");
        var region = world.LightWorldSouth;
        var location1 = region.LinksHouse.Id;
        var location2 = region.MazeRace.Id;
        var location3 = region.IceCave.Id;
        config.LocationItems[location1] = (int)ItemPool.Progression;
        config.LocationItems[location2] = (int)ItemPool.Junk;
        config.LocationItems[location3] = (int)ItemType.Firerod;

        for (var i = 0; i < 3; i++)
        {
            var seedData = randomizer.GenerateSeed(config, null, default);
            world = seedData.WorldGenerationData.LocalWorld.World;
            world.Locations.First(x => x.Id == location1).Item.Progression.Should().BeTrue();
            world.Locations.First(x => x.Id == location2).Item.Progression.Should().BeFalse();
            var fireRodAtLocation = world.Locations.First(x => x.Id == location3).Item.Type == ItemType.Firerod;
            var fireRodAccessible = !Logic.GetMissingRequiredItems(world.Locations.First(x => x.Item.Type == ItemType.Firerod), new Progression(), out _).Any();
            Assert.True(fireRodAtLocation || fireRodAccessible);
        }
    }

    [Fact]
    public void EarlyItemConfig()
    {
        var filler = new StandardFiller(GetLogger<StandardFiller>());
        var randomizer = GetRandomizer();
        var options = ItemSettingOptions.GetOptions();

        var fireRod = options.First(opts =>
            opts.Options.Any(opt => opt.MatchingItemTypes?.Contains(ItemType.Firerod) == true));
        var iceRod = options.First(opts =>
            opts.Options.Any(opt => opt.MatchingItemTypes?.Contains(ItemType.Icerod) == true));
        var hiJump = options.First(opts =>
            opts.Options.Any(opt => opt.MatchingItemTypes?.Contains(ItemType.HiJump) == true));

        var config = new Config();
        config.ItemOptions[fireRod.Item] =
            fireRod.Options.FindIndex(x => x.MemoryValues == null && x.MatchingItemTypes != null);
        config.ItemOptions[iceRod.Item] =
            iceRod.Options.FindIndex(x => x.MemoryValues == null && x.MatchingItemTypes != null);
        config.ItemOptions[hiJump.Item] =
            hiJump.Options.FindIndex(x => x.MemoryValues == null && x.MatchingItemTypes != null);

        for (var i = 0; i < 3; i++)
        {
            var seedData = randomizer.GenerateSeed(config, null, default);
            var world = seedData.WorldGenerationData.LocalWorld.World;
            var progression = new Progression();
            world.Locations.First(x => x.Item.Type == ItemType.Firerod).UpdateAccessibility(progression, progression);
            world.Locations.First(x => x.Item.Type == ItemType.Icerod).UpdateAccessibility(progression, progression);
            world.Locations.First(x => x.Item.Type == ItemType.HiJump).UpdateAccessibility(progression, progression);
            Logic.GetMissingRequiredItems(world.Locations.First(x => x.Item.Type == ItemType.Firerod), progression, out _).Should().BeEmpty();
            Logic.GetMissingRequiredItems(world.Locations.First(x => x.Item.Type == ItemType.Icerod), progression, out _).Should().BeEmpty();
            Logic.GetMissingRequiredItems(world.Locations.First(x => x.Item.Type == ItemType.HiJump), progression, out _).Should().BeEmpty();
        }
    }

    [Fact]
    public void StartingItemConfigs()
    {
        var filler = new StandardFiller(GetLogger<StandardFiller>());
        var randomizer = GetRandomizer();
        var options = ItemSettingOptions.GetOptions();

        var fireRod = options.First(opts =>
            opts.Options.Any(opt => opt.MatchingItemTypes?.Contains(ItemType.Firerod) == true));
        var iceRod = options.First(opts =>
            opts.Options.Any(opt => opt.MatchingItemTypes?.Contains(ItemType.Icerod) == true));
        var hiJump = options.First(opts =>
            opts.Options.Any(opt => opt.MatchingItemTypes?.Contains(ItemType.HiJump) == true));

        var config = new Config();
        config.ItemOptions[fireRod.Item] =
            fireRod.Options.FindIndex(x => x.MemoryValues != null && x.MatchingItemTypes != null);
        config.ItemOptions[iceRod.Item] =
            iceRod.Options.FindIndex(x => x.MemoryValues != null && x.MatchingItemTypes != null);
        config.ItemOptions[hiJump.Item] =
            hiJump.Options.FindIndex(x => x.MemoryValues != null && x.MatchingItemTypes != null);

        for (var i = 0; i < 3; i++)
        {
            var seedData = randomizer.GenerateSeed(config, null, default);
            var world = seedData.WorldGenerationData.LocalWorld.World;
            world.AllItems.Should().NotContain(x => x.Type == ItemType.Firerod);
            world.AllItems.Should().NotContain(x => x.Type == ItemType.Icerod);
            world.AllItems.Should().NotContain(x => x.Type == ItemType.HiJump);
        }
    }

    private static ILogger<T> GetLogger<T>()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(options =>
            {

            })
            .BuildServiceProvider();
        return serviceProvider.GetRequiredService<ILogger<T>>();
    }

    private static Smz3Randomizer GetRandomizer()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(options => { })
            .AddSingleton<OptionsFactory>()
            .AddSingleton<ConfigProvider>()
            .AddSingleton<RomPatchFactory>()
            .AddSingleton<IMetadataService, MetadataService>()
            .AddSingleton<IGameHintService, GameHintService>()
            .AddSingleton<IPatcherService, PatcherService>()
            .AddSingleton<TrackerOptionsAccessor>()
            .AddSingleton<TrackerSpriteService>()
            .AddTransient<PlaythroughService>()
            .AddConfigs()
            .BuildServiceProvider();

        var filler = new StandardFiller(GetLogger<StandardFiller>());
        return new Smz3Randomizer(filler, new WorldAccessor(),
            serviceProvider.GetRequiredService<IGameHintService>(),
            GetLogger<Smz3Randomizer>(),
            serviceProvider.GetRequiredService<IPatcherService>(),
            serviceProvider.GetRequiredService<PlaythroughService>());
    }
}
