using System.Collections.Generic;

namespace Randomizer.SMZ3.FileData.Patches;

public class ZeldaKeysanityPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(PatcherServiceData data)
    {
        if (data.LocalWorld.Config.ZeldaKeysanity)
        {
            yield return new GeneratedPatch(Snes(0x40003B), new byte[] { 1 }); // MapMode #$00 = Always On (default) - #$01 = Require Map Item
            yield return new GeneratedPatch(Snes(0x400045), new byte[] { 0x0f }); // display ----dcba a: Small Keys, b: Big Key, c: Map, d: Compass
        }
        if (data.LocalWorld.Config.Keysanity)
        {
            yield return new GeneratedPatch(Snes(0x40016A), new byte[] { 1 }); // FreeItemText: db #$01 ; #00 = Off (default) - #$01 = On
        }
    }
}
