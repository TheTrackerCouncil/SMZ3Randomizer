using System.Collections.Generic;
using Randomizer.Abstractions;
using Randomizer.Shared;

namespace Randomizer.SMZ3.FileData.Patches;

[Order(-8)]
public class DiggingGamePatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var digs = (byte)(data.Random.Next(30) + 1);
        yield return new GeneratedPatch(Snes(0x308020), new[] { digs });
        yield return new GeneratedPatch(Snes(0x1DFD95), new[] { digs });
    }
}
