using System.Collections.Generic;
using Randomizer.Abstractions;
using Randomizer.Shared;

namespace Randomizer.SMZ3.FileData.Patches;

[Order(-6)]
public class FairyPondTradePatch : RomPatch
{

    private static readonly List<byte> s_fairyPondTrades = new()
    {
        0x16, 0x2B, 0x2C, 0x2D, 0x3C, 0x3D, 0x48
    };

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        if (!data.World.Config.CasPatches.RandomizedBottles) yield break;
        yield return new GeneratedPatch(Snes(0x6C8FF), new[] { s_fairyPondTrades.Random(data.Random) });
        yield return new GeneratedPatch(Snes(0x6C93B), new[] { s_fairyPondTrades.Random(data.Random) });
    }


}
