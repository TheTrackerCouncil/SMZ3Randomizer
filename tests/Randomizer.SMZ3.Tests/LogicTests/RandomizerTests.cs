﻿using System;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Randomizer.Shared;
using Randomizer.SMZ3.Generation;
using Randomizer.SMZ3.Infrastructure;

using Xunit;

namespace Randomizer.SMZ3.Tests.LogicTests
{
    public class RandomizerTests
    {
        // If this test breaks, update Smz3Randomizer.Version
        [Theory]
        [InlineData("test", 2042629884)] // Smz3Randomizer v1.0
        public void StandardFillerWithSameSeedGeneratesSameWorld(string seed, int expectedHash)
        {
            var filler = new StandardFiller(GetLogger<StandardFiller>());
            var randomizer = new Smz3Randomizer(filler, new WorldAccessor(), GetLogger<Smz3Randomizer>());
            var config = new Config();

            var seedData = randomizer.GenerateSeed(config, seed, default);
            var worldHash = GetHashForWorld(seedData.Worlds[0].World);

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
                stringBuilder.AppendLine();
            }
            var serializedWorld = stringBuilder.ToString();
            return NonCryptographicHash.Fnv1a(serializedWorld);
        }

        [Fact]
        public void LocationItemConfig()
        {
            var filler = new StandardFiller(GetLogger<StandardFiller>());
            var randomizer = new Smz3Randomizer(filler, new WorldAccessor(), GetLogger<Smz3Randomizer>());

            var config = new Config();
            var region = new Regions.Zelda.LightWorld.LightWorldSouth(null, null);
            var location1 = region.LinksHouse.Id;
            var location2 = region.MazeRace.Id;
            var location3 = region.IceCave.Id;
            config.LocationItems[location1] = (int)ItemPool.Progression;
            config.LocationItems[location2] = (int)ItemPool.Junk;
            config.LocationItems[location3] = (int)ItemType.Firerod;

            for (var i = 0; i < 3; i++)
            {
                var seedData = randomizer.GenerateSeed(config, null, default);
                var world = seedData.Worlds.First().World;
                world.Locations.First(x => x.Id == location1).Item.Progression.Should().BeTrue();
                world.Locations.First(x => x.Id == location2).Item.Progression.Should().BeFalse();
                var fireRodAtLocation = world.Locations.First(x => x.Id == location3).Item.Type == ItemType.Firerod;
                var fireRodAccessible = !Logic.GetMissingRequiredItems(world.Locations.First(x => x.Item.Type == ItemType.Firerod), new Progression()).Any();
                Assert.True(fireRodAtLocation || fireRodAccessible);
            }
        }

        [Fact]
        public void EarlyItemConfig()
        {
            var filler = new StandardFiller(GetLogger<StandardFiller>());
            var randomizer = new Smz3Randomizer(filler, new WorldAccessor(), GetLogger<Smz3Randomizer>());

            var config = new Config();
            config.EarlyItems.Add(ItemType.Firerod);
            config.EarlyItems.Add(ItemType.Icerod);
            config.EarlyItems.Add(ItemType.MoonPearl);

            for (var i = 0; i < 3; i++)
            {
                var seedData = randomizer.GenerateSeed(config, null, default);
                var world = seedData.Worlds.First().World;
                var progression = new Progression();
                Logic.GetMissingRequiredItems(world.Locations.First(x => x.Item.Type == ItemType.Firerod), progression).Should().BeEmpty();
                Logic.GetMissingRequiredItems(world.Locations.First(x => x.Item.Type == ItemType.Icerod), progression).Should().BeEmpty();
                Logic.GetMissingRequiredItems(world.Locations.First(x => x.Item.Type == ItemType.MoonPearl), progression).Should().BeEmpty();
            }
        }

        private static ILogger<T> GetLogger<T>()
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(options =>
                {

                })
                .BuildServiceProvider();

            return serviceCollection.GetRequiredService<ILogger<T>>();
        }
    }
}
