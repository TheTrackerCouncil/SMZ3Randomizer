using System.Collections.Generic;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

/// <summary>
/// Represents an SMZ3 patch that speeds up the fades to, from, and within the Metroid pause menu
/// </summary>
public class MetroidFastPauseMenuPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        if (data.Config.CasPatches.MetroidFastPauseMenu)
        {
            yield return new GeneratedPatch(Snes(0x828D30), [0]); // Fade in on pause menu
            yield return new GeneratedPatch(Snes(0x8291CB), [0]); // Fade in on equipment screen
            yield return new GeneratedPatch(Snes(0x8291EE), [0]); // Fade in on map screen
            yield return new GeneratedPatch(Snes(0x82937D), [0]); // Fade in on gameplay
            yield return new GeneratedPatch(Snes(0x82A5CA), [0]); // Fade out from pause menu
            yield return new GeneratedPatch(Snes(0x90EA6F), [0]); // Fade out from gameplay
        }
    }
}
