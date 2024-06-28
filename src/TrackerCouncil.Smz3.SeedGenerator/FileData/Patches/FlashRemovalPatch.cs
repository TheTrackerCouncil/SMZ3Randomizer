using System.Collections.Generic;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

/// <summary>
/// Represents an SMZ3 ROM patch that removes flashing from A Link to the Past
/// </summary>
public class FlashRemovalPatch : RomPatch
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
        if (!data.Config.CasPatches.DisableFlashing)
            yield break;

        // Update various effects to remove flashing
        // https://github.com/ArchipelagoMW/Archipelago/blob/main/worlds/alttp/Rom.py#L1800
        yield return new GeneratedPatch(Snes(Rom.TranslateZeldaOffset(0x17E07)), new byte[] { 0x06 });
        yield return new GeneratedPatch(Snes(Rom.TranslateZeldaOffset(0x17EAB)), new byte[] { 0xD0, 0x03, 0xA9, 0x40, 0x29, 0x60 });
        yield return new GeneratedPatch(Snes(Rom.TranslateZeldaOffset(0x123FE)), new byte[] { 0x72 });
        yield return new GeneratedPatch(Snes(Rom.TranslateZeldaOffset(0x3FA7B)), new byte[] { 0x80, 0xac - 0x7b });
        yield return new GeneratedPatch(Snes(Rom.TranslateZeldaOffset(0x3FAB6)), new byte[] { 0x80 });
        yield return new GeneratedPatch(Snes(Rom.TranslateZeldaOffset(0x3FAC2)), new byte[] { 0x80 });
    }
}
