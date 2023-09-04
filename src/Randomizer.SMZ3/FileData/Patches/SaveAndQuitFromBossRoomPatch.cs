using System.Collections.Generic;

namespace Randomizer.SMZ3.FileData.Patches;

public class SaveAndQuitFromBossRoomPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(PatcherServiceData data)
    {
        /* Defaults to $00 at [asm]/z3/randomizer/tables.asm */
        yield return new GeneratedPatch(Snes(0x308042), new byte[] { 0x01 });
    }
}
