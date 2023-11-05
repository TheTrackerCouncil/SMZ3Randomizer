using System.Collections.Generic;
using Randomizer.Abstractions;
using Randomizer.Shared;

namespace Randomizer.SMZ3.FileData.Patches;

[Manual]
public class ZeldaKeysanityPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        if (data.World.Config.GameModeConfigs.KeysanityConfig.ZeldaKeysanity)
        {
            yield return new GeneratedPatch(Snes(0x40003B), new byte[] { 1 }); // MapMode #$00 = Always On (default) - #$01 = Require Map Item
            yield return new GeneratedPatch(Snes(0x400045), new byte[] { 0x0f }); // display ----dcba a: Small Keys, b: Big Key, c: Map, d: Compass
        }
        if (data.World.Config.GameModeConfigs.KeysanityConfig.KeysanityEnabled)
        {
            yield return new GeneratedPatch(Snes(0x40016A), new byte[] { 1 }); // FreeItemText: db #$01 ; #00 = Off (default) - #$01 = On
        }
    }
}
