using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Castle.Core.Logging;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Randomizer.Shared;
using Randomizer.SMZ3.Generation;

using Xunit;
using Xunit.Sdk;

namespace Randomizer.SMZ3.Tests.Logic
{
    public class RandomizerTests
    {
        // If this test breaks, update Smz3Randomizer.Version
        [Theory]
        [InlineData("test", 2042629884)] // Smz3Randomizer v1.0
        public void StandardFillerWithSameSeedGeneratesSameWorld(string seed, int expectedHash)
        {
            var logger = GetLogger<StandardFiller>();
            var filler = new StandardFiller(logger);
            var randomizer = new Smz3Randomizer(filler);
            var config = new Config();

            var seedData = randomizer.GenerateSeed(config, seed, default);
            var worldHash = GetHashForWorld(seedData.Worlds[0].World);

            worldHash.Should().Be(expectedHash);
        }

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
