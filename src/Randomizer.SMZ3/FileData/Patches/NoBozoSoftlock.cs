using System.Collections.Generic;
using Randomizer.Abstractions;

namespace Randomizer.SMZ3.FileData.Patches
{
    /// <summary>
    /// Represents an SMZ3 ROM patch that updates the Bozo Door Time
    /// </summary>
    public class NoBozoSoftlock : RomPatch
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
            // Updates the value set by bomb_torizo.asm
            if (data.Config.CasPatches.NoBozoSoftlock)
                yield return new GeneratedPatch(Snes(0x84BA54), new byte[] { 0x28 });
            else
                yield return new GeneratedPatch(Snes(0x84BA54), new byte[] { 0x28 * 2 });
        }
    }
}
