using System.Collections.Generic;

namespace Randomizer.SMZ3.FileData.Patches;

public class SaveAndQuitFromBossRoomPatch : RomPatch
{
    public override IEnumerable<(int offset, byte[] data)> GetChanges(PatcherServiceData data)
    {
        /* Defaults to $00 at [asm]/z3/randomizer/tables.asm */
        yield return (Snes(0x308042), new byte[] { 0x01 });
    }
}
