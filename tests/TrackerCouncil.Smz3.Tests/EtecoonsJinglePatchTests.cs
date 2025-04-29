using System;
using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;
using Xunit;

namespace TrackerCouncil.Smz3.Tests;

public class EtecoonsJinglePatchTests
{
    public static IEnumerable<object[]> Values()
    {
        foreach (var jingle in Enum.GetValues(typeof(EtecoonsJingle)))
        {
            yield return [jingle];
        }
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void GetJingle(EtecoonsJingle jingle)
    {
        Assert.NotNull(new EtecoonsJinglePatch().GetJingle(jingle));
    }

    [Theory]
    [MemberData(nameof(Values))]
    public void GetPatch(EtecoonsJingle jingle)
    {
        var etecoonsJinglePatch = new EtecoonsJinglePatch();

        Assert.NotEmpty(etecoonsJinglePatch.GetPatch(etecoonsJinglePatch.GetJingle(jingle)));
    }
}
