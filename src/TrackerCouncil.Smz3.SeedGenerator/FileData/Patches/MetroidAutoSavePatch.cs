using System.Collections.Generic;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

/// <summary>
/// Represents an SMZ3 ROM patch that enables auto saving upon Metroid deaths
/// </summary>
public class MetroidAutoSavePatch : RomPatch
{
    /// <summary>
    /// Returns the changes to be applied to an SMZ3 ROM file.
    /// </summary>
    /// <param name="data">Patcher Data with the world and config information</param>
    /// <returns>
    /// A collection of changes, represented by the data to overwrite at the
    /// specified ROM offset.
    /// </returns>
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        // Updates the value set in config.asm
        if (data.Config.CasPatches.MetroidAutoSave)
            yield return new GeneratedPatch(Snes(0xF4700C), UshortBytes(0x0001));
        else
            yield return new GeneratedPatch(Snes(0xF4700C), UshortBytes(0x0000));
    }
}
