using System.Collections.Generic;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[SkipForParsedRoms]
public class ZeldaKeysanityPatch : RomPatch
{
    private static readonly int s_mapDisplayAddress = Snes(0x40003B);
    private static readonly int s_dungeonItemDisplayAddress = Snes(0x400045);
    private static readonly int s_displayItemTextAddress = Snes(0x40016A);

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        if (data.World.Config.ZeldaKeysanity)
        {
            yield return new GeneratedPatch(s_mapDisplayAddress, [1]); // MapMode #$00 = Always On (default) - #$01 = Require Map Item
            yield return new GeneratedPatch(s_dungeonItemDisplayAddress, [0x0f]); // display ----dcba a: Small Keys, b: Big Key, c: Map, d: Compass
        }
        if (data.World.Config.Keysanity)
        {
            yield return new GeneratedPatch(s_displayItemTextAddress, [1]); // FreeItemText: db #$01 ; #00 = Off (default) - #$01 = On
        }
    }

    public static bool GetIsZeldaKeysanity(byte[] rom)
    {
        return rom[s_mapDisplayAddress] == 1 || rom[s_dungeonItemDisplayAddress] == 0x0f;
    }
}
