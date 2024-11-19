using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[Order(-8), SkipForParsedRoms]
public class DiggingGamePatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var digs = (byte)(data.Random.Next(30) + 1);
        yield return new GeneratedPatch(Snes(0x308020), new[] { digs });
        yield return new GeneratedPatch(Snes(0x1DFD95), new[] { digs });
    }
}
