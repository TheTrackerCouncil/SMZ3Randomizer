using System.Collections.Generic;
using Randomizer.Abstractions;

namespace Randomizer.SMZ3.FileData.Patches;

public class SaveAndQuitFromBossRoomPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        /* Defaults to $00 at [asm]/z3/randomizer/tables.asm */
        yield return new GeneratedPatch(Snes(0x308042), new byte[] { 0x01 });
    }
}
