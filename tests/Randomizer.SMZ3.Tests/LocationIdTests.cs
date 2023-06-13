using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Xunit;

namespace Randomizer.SMZ3.Tests
{
    public class LocationIdTests
    {
        public LocationIdTests()
        {
            World = new World(new Config(), "", 0, "");
        }

        protected World World { get; }

        [Fact]
        public void EachLocationIdValueIsUsedExactlyOnce()
        {
            var allLocationIds = Enum.GetValues<LocationId>().ToImmutableSortedSet();
            var usedLocationIds = World.Locations.Select(x => x.Id);

            // Every value was used
            Assert.Empty(allLocationIds.Except(usedLocationIds));
            // No value was repeated
            Assert.Equal(allLocationIds.Count, usedLocationIds.Count());
        }
    }
}
