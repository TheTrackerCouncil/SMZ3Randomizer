using System;
using System.Collections.Immutable;
using System.Linq;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using Xunit;

namespace TrackerCouncil.Smz3.Tests;

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
